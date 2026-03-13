using auction.Shared.Data;
using auction.Shared.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


namespace auction.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("admin_queue", true, false, false);
            await channel.QueueDeclareAsync("bids_queue", true, false, false);

            var adminConsumer = new AsyncEventingBasicConsumer(channel);
            adminConsumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<JsonElement>(json);
                string action = request.GetProperty("Action").GetString()!;

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    if (action == "CREATE")
                    {
                        var item = JsonSerializer.Deserialize<Item>(request.GetProperty("Data").GetRawText());
                        if (item != null)
                        {
                            db.Items.Add(item);
                            await db.SaveChangesAsync();
                            _logger.LogInformation("item created: {Id}", item.Id);
                        }
                    }
                    else if (action == "DELETE")
                    {
                        var id = request.GetProperty("Id").GetGuid();
                        var item = await db.Items.FindAsync(id);
                        if (item != null)
                        {
                            db.Items.Remove(item);
                            await db.SaveChangesAsync();
                            _logger.LogInformation("item deleted: {Id}", id);
                        }
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "admin queue processing error");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            var bidConsumer = new AsyncEventingBasicConsumer(channel);
            bidConsumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<JsonElement>(json);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    var bid = JsonSerializer.Deserialize<Bid>(request.GetProperty("Data").GetRawText());
                    if (bid == null) return;

                    var item = await db.Items.FindAsync(bid.ItemId);

                    if (item != null && item.IsActive && bid.Amount > item.CurrentPrice && DateTime.UtcNow < item.EndTime)
                    {
                        item.CurrentPrice = bid.Amount;
                        item.HighestBidderId = bid.BidderId;

                        db.Bids.Add(bid);
                        await db.SaveChangesAsync();
                        _logger.LogInformation("new high bid of {Amount} on item {ItemId}", bid.Amount, bid.ItemId);
                    }
                    else
                    {
                        _logger.LogWarning("rejected invalid bid on item {ItemId}", bid.ItemId);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "bids qeuue processing error");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync("admin_queue", false, adminConsumer);
            await channel.BasicConsumeAsync("bids_queue", false, bidConsumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

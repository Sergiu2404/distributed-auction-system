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
        //private readonly ILogger<Worker> _logger;

        //public Worker(ILogger<Worker> logger)
        //{
        //    _logger = logger;
        //}

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        if (_logger.IsEnabled(LogLevel.Information))
        //        {
        //            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //            //Console.WriteLine("write");
        //        }
        //        await Task.Delay(1000, stoppingToken);
        //    }
        //}

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

            await channel.QueueDeclareAsync(
                queue: "admin_queue", 
                durable: true, 
                exclusive: false, 
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<JsonElement>(json);

                string action = request.GetProperty("Action").GetString()!;

                // create scoped to access db context
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    if (action == "CREATE")
                    {
                        var item = JsonSerializer.Deserialize<Item>(request.GetProperty("Data").GetRawText());

                        db.Items.Add(item!);
                        await db.SaveChangesAsync();
                        
                        _logger.LogInformation("item created: {name}", item?.Name);
                    }
                    else if (action == "DELETE")
                    {
                        var id = request.GetProperty("Id").GetGuid();
                        var item = await db.Items.FindAsync(id);

                        if (item != null) 
                        { 
                            db.Items.Remove(item); 
                            await db.SaveChangesAsync(); 
                        }
                    }

                    // acknowledge operation's success
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Eroare la procesare");
                    // Nack, requeue true => message goes back to the queue to be (Failover)
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: "admin_queue", autoAck: false, consumer: consumer);

            // worker keeps being active
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace auction.Shared.Services.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public async Task PublishAsync(string queueName, object message)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // durable queue
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // persistent message
            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
    }
}

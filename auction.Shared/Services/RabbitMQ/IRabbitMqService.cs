using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Services.RabbitMQ
{
    public interface IRabbitMqService
    {
        Task PublishAsync(string queueName, object message);
    }
}

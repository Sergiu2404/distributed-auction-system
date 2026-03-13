using auction.Shared.Services.BidService;
using auction.Shared.Services.ItemService;
using auction.Shared.Services.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IBidService, BidService>();

            services.AddSingleton<IRabbitMqService, RabbitMqService>();

            return services;
        }
    }
}

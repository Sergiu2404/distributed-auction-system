using auction.Shared.Data;
using auction.Shared.Entities;
using auction.Shared.Services.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Services.ItemService
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _db;
        private readonly IRabbitMqService _rabbitService;

        public ItemService(AppDbContext db, IRabbitMqService rabbitService)
        {
            _db = db;
            _rabbitService = rabbitService;
        }

        public IEnumerable<Item> GetAllItems() => _db.Items.ToList();

        public Item? GetItemById(Guid id) => _db.Items.Find(id);

        public async Task CreateItemAsync(Item item, Guid ownerId)
        {
            item.Id = Guid.NewGuid();
            item.OwnerId = ownerId;
            await _rabbitService.PublishAsync("admin_queue", new { Action = "CREATE", Data = item });
        }

        public async Task UpdateItemAsync(Guid id, Item item)
        {
            item.Id = id;
            await _rabbitService.PublishAsync("admin_queue", new { Action = "UPDATE", Data = item });
        }

        public async Task DeleteItemAsync(Guid id)
        {
            await _rabbitService.PublishAsync("admin_queue", new { Action = "DELETE", Id = id });
        }
    }
}

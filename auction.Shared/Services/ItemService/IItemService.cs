using auction.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Services.ItemService
{
    public interface IItemService
    {
        IEnumerable<Item> GetAllItems();
        Item? GetItemById(Guid id);
        Task CreateItemAsync(Item item, Guid ownerId);
        Task UpdateItemAsync(Guid id, Item item);
        Task DeleteItemAsync(Guid id);
    }
}

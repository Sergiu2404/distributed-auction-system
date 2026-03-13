using auction.Shared.Data;
using auction.Shared.Entities;
using auction.Shared.Services.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace auction.Shared.Services.BidService
{
    public class BidService : IBidService
    {
        private readonly AppDbContext _db;
        private readonly IRabbitMqService _rabbitService;

        public BidService(AppDbContext db, IRabbitMqService rabbitService)
        {
            _db = db;
            _rabbitService = rabbitService;
        }

        public async Task<IEnumerable<Bid>> GetBidsByItemIdAsync(Guid itemId)
        {
            return await _db.Bids
                .Where(b => b.ItemId == itemId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task PlaceBidAsync(Bid bid)
        {
            bid.Id = Guid.NewGuid();
            bid.CreatedAt = DateTime.UtcNow;

            await _rabbitService.PublishAsync("bids_queue", new { Action = "PLACE_BID", Data = bid });
        }
    }
}

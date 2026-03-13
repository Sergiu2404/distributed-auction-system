using auction.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Services.BidService
{
    public interface IBidService
    {
        Task<IEnumerable<Bid>> GetBidsByItemIdAsync(Guid itemId);
        Task PlaceBidAsync(Bid bid);
    }
}

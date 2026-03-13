using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Entities
{
    public class Bid
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public Guid BidderId { get; set; }
        public AppUser Bidder { get; set; } = null!;

        public double Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

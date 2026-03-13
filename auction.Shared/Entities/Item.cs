using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace auction.Shared.Entities
{
    public class Item
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public double StartingPrice { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }

        public Guid OwnerId { get; set; }
        public AppUser Owner { get; set; } = null!;

        public Guid? HighestBidderId { get; set; }
        public AppUser? HighestBidder { get; set; } = null!;

        // bid history
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}

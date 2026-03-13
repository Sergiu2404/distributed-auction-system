using auction.Shared.Data;
using auction.Shared.Entities;
using auction.Shared.Services.BidService;
using auction.Shared.Services.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace auction.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;
        public BidController(IBidService bidService)
        {
            _bidService = bidService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBids(Guid itemId)
        {
            var bids = await _bidService.GetBidsByItemIdAsync(itemId);
            return Ok(bids);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceBid(Guid itemId, [FromBody] double amount)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var bid = new Bid
            {
                ItemId = itemId,
                BidderId = Guid.Parse(userIdClaim),
                Amount = amount
            };

            await _bidService.PlaceBidAsync(bid);

            return Accepted(new { Message = "bid sent for processing", BidId = bid.Id });
        }
    }
}

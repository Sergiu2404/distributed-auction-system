using auction.API.Services.Auth;
using auction.Shared.Data;
using auction.Shared.Entities;
using auction.Shared.Services.BidService;
using auction.Shared.Services.ItemService;
using auction.Shared.Services.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace auction.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_itemService.GetAllItems());

        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public IActionResult GetById(Guid id)
        {
            var item = _itemService.GetItemById(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Item item)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            await _itemService.CreateItemAsync(item, Guid.Parse(userIdClaim));
            return Accepted(new { Message = "create request sent" });
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Item item)
        {
            await _itemService.UpdateItemAsync(id, item);
            return Accepted(new { Message = "update request sent" });
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _itemService.DeleteItemAsync(id);
            return Accepted(new { Message = "delete request sent" });
        }
    }
}

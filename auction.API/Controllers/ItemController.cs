using auction.API.Services.Auth;
using auction.Shared.Data;
using auction.Shared.Entities;
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
        private readonly AppDbContext _db;
        private readonly IRabbitMqService _rabbitService;

        public ItemController(AppDbContext db, IRabbitMqService rabbitService)
        {
            _db = db;
            _rabbitService = rabbitService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_db.Items.ToList());

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var item = _db.Items.Find(id);
            return item == null ? NotFound() : Ok(item);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Item item)
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            item.OwnerId = adminId;
            item.Id = Guid.NewGuid();

            await _rabbitService.PublishAsync("admin_queue", new { Action = "CREATE", Data = item });
            return Accepted(new { Message = "Creation request sent to be processed" });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Item item)
        {
            item.Id = id;
            await _rabbitService.PublishAsync("admin_queue", new { Action = "UPDATE", Data = item });
            return Accepted(new { Message = "update request sent to be processed" });
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _rabbitService.PublishAsync("admin_queue", new { Action = "DELETE", Id = id });
            return Accepted(new { Message = "delete request sent to be processed" });
        }
    }
}

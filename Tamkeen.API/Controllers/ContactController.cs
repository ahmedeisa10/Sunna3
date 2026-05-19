using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamkeen.Application.DTOs.Contact;
using Tamkeen.Application.Interfaces.Contact;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        private string GetUserId() => User.FindFirstValue("sub")!;

        // POST /api/Contact/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendContactMessageDto dto)
        {
            var result = await _contactService.SendMessageAsync(GetUserId(), dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
        // GET /api/Contact/messages  
        [HttpGet("messages")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _contactService.GetAllMessagesAsync();
            return Ok(messages);
        }

        // PATCH /api/Contact/messages/{id}/read
        [HttpPatch("messages/{id}/read")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _contactService.MarkAsReadAsync(id);
            return Ok(new { message = "تم التحديث" });
        }
    }
}
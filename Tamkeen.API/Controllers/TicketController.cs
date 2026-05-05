using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Tamkeen.Application.DTOs.Ticket_DTOs;
using Tamkeen.Application.Interfaces.Ticket_Interface;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue("sub")!;
        }
        private string GetUserRole() => User.FindFirstValue("role")!;
        // GET /api/tickets
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? governorate = null,[FromQuery] string? city = null)
        {
            var tickets = await _ticketService.GetAllAsync(GetUserId(), GetUserRole(), governorate, city);
            return Ok(tickets);
        }

        // GET /api/tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id, GetUserId(), GetUserRole());
            return Ok(ticket);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetPending([FromQuery] string? governorate = null,[FromQuery] string? city = null)
        {
            var tickets = await _ticketService.GetPendingAsync(governorate, city);
            return Ok(tickets);
        }
        [HttpPost("{ticketId}/apply")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Apply(Guid ticketId)
        {
            await _ticketService.ApplyAsync(ticketId, GetUserId());
            return Ok(new { message = "تم التقديم بنجاح" });
        }
        [HttpPatch("{applicationId}/accept")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> AcceptApplication(Guid applicationId)
        {
            await _ticketService.AcceptApplicationAsync(applicationId, GetUserId());
            return Ok(new { message = "تم قبول الفني بنجاح" });
        }
        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> Create([FromForm] CreateTicketDto dto)
        {
            var tenantId = GetUserId();
            var ticket = await _ticketService.CreateAsync(dto, tenantId);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        //Manager See Requests
        [HttpGet("ManagerReview")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerReview()
        {
            var result = await _ticketService.GetManagerReviewAsync();
            return Ok(result);
        }

        //Manager Accept Request
        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _ticketService.ApproveAsync(id);
            return Ok(new { message = "Ticket Accepted and shown for vendor" });
        }


        //Manager Reject Request
        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Reject(Guid id )
        {
            await _ticketService.RejectAsync(id);
            return Ok(new { message = "Request has been rejected" });
        }

        #region commented out

        //[HttpPatch("{id}/assign")]
        //[Authorize(Roles = "Manager")]
        //public async Task<IActionResult> AssignVendor(Guid id, [FromBody] AssignTicketDto dto)
        //{
        //    await _ticketService.AssignVendorAsync(id, dto);
        //    return NoContent();
        //}

        // PATCH /api/tickets/{id}/accept
        //[HttpPatch("{id}/accept")]
        //[Authorize(Roles = "Vendor")]
        //public async Task<IActionResult> Accept(Guid id)
        //{
        //    await _ticketService.AcceptAsync(id, GetUserId());
        //    return NoContent();
        //}

        // PATCH /api/tickets/{id}/reject
        //[HttpPatch("{id}/reject")]
        //[Authorize(Roles = "Vendor")]
        //public async Task<IActionResult> Reject(Guid id)
        //{
        //    await _ticketService.RejectAsync(id, GetUserId());
        //    return NoContent();
        //}
        #endregion

        [HttpPatch("{ticketId}/complete")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Complete(Guid ticketId, [FromForm] CompleteTicketDto dto)
        {
            var images = await _ticketService.CompleteWithImagesAsync(ticketId, dto, GetUserId());
            return Ok(new { message = "تم إنهاء المهمة بنجاح", images });
        }

        // PATCH /api/tickets/{id}/close
        [HttpPatch("{id}/close")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> Close(Guid id)
        {
            await _ticketService.CloseAsync(id, GetUserId());
            return NoContent();
        }


        

    }
}

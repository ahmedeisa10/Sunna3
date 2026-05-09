using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tamkeen.Application.DTOs.Payment_DTOs;
using Tamkeen.Application.Interfaces.Payments;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;

        public PaymentController(IPaymentService paymentService, AppDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        [HttpPost("initiate")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var tenantId = User.FindFirstValue("sub")!;
            var result = await _paymentService.InitiatePaymentAsync(dto, tenantId);
            return Ok(result);
        }
        // في PaymentController.cs — endpoint جديد بسيط
        [HttpPost("mark-paid/{paymentId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> MarkPaid(Guid paymentId)
        {
            var tenantId = User.FindFirstValue("sub")!;

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) return NotFound();
            if (payment.TenantId != tenantId) return Forbid();
            if (payment.IsPaid)
                return Ok(new { isPaid = true, ticketId = payment.TicketId });

            payment.IsPaid = true;
            payment.PaidAt = DateTime.UtcNow;
            if (payment.Ticket != null) payment.Ticket.IsPaid = true;

            await _context.SaveChangesAsync();
            return Ok(new { isPaid = true, ticketId = payment.TicketId });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var hmac = Request.Query["hmac"].ToString();
            await _paymentService.HandleWebhookAsync(payload, hmac);
            return Ok();
        }

        [HttpGet("verify/{paymentId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> VerifyPayment(Guid paymentId)
        {
            var result = await _paymentService.VerifyAndSyncPaymentAsync(paymentId);
            return Ok(new { isPaid = result.IsPaid, ticketId = result.TicketId });
        }
    }
}
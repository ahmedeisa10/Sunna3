using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamkeen.Application.DTOs.Payment_DTOs;
using Tamkeen.Application.Interfaces.Payments;

namespace Tamkeen.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // ── Tenant يبدأ الدفع ───────────────────────────────
        // POST /api/payment/initiate
        [HttpPost("initiate")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var tenantId = User.FindFirstValue("sub")!;
            var result = await _paymentService.InitiatePaymentAsync(dto, tenantId);
            return Ok(result);
        }

        // ── Paymob بيكلم الـ endpoint ده لما الدفع يتم ─────
        // POST /api/payment/webhook
        [HttpPost("webhook")]
        [AllowAnonymous]   // Paymob مش بيبعت token, لازم AllowAnonymous
        public async Task<IActionResult> Webhook()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var hmac = Request.Query["hmac"].ToString();

            await _paymentService.HandleWebhookAsync(payload, hmac);
            return Ok();
        }
    }
}
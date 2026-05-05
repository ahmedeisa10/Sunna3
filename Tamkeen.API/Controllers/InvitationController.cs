using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tamkeen.Application.DTOs;
using Tamkeen.Application.Interfaces;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly IInvitationService _invitationService;
        private readonly AppDbContext context;

        public InvitationController(IInvitationService invitationService,AppDbContext context)
        {
            _invitationService = invitationService;
            this.context = context;
        }

        // ===== Manager Only =====
        [HttpPost("create")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationDto dto)
        {
            var link = await _invitationService.CreateInvitationAsync(dto.Phone);
            return Ok(new { link });
            // The manager take this link and send it via WhatsApp
        }

        // ===== Verify the token before opening the form =====
        [HttpGet("validate/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var invitation = await context.vendorInvitations
                .FirstOrDefaultAsync(x => x.Token == token && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow);

            if (invitation == null)
                return BadRequest(new { message = "Invalid or expired link." });

            return Ok(new { valid = true });
        }

        // =====  Vendor registers =====
        [HttpPost("register-vendor")]
        public async Task<IActionResult> RegisterVendor([FromForm] VendorRegisterDto dto)
        {
            var (success, message, token, profileImageUrl) =
                await _invitationService.RegisterVendorAsync(dto);

            if (!success)
                return BadRequest(new { message });

            return Ok(new
            {
                message = message,
                token = token,
                profileImageUrl = profileImageUrl, 
                imageUrl = profileImageUrl, 
            });
        }
    }
}

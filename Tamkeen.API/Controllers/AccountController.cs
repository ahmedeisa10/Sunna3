using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamkeen.Application.DTOs.Account;
using Tamkeen.Application.Interfaces.Account;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        private string GetUserId() => User.FindFirstValue("sub")!;

        // GET /api/Account/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _accountService.GetProfileAsync(GetUserId());
            return Ok(profile);
        }

        // PUT /api/Account/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var result = await _accountService.UpdateProfileAsync(GetUserId(), dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // POST /api/Account/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = await _accountService.ChangePasswordAsync(GetUserId(), dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
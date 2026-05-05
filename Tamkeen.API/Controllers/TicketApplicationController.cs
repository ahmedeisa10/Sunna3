using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tamkeen.Application.Interfaces;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketApplicationController(ITicketApplicationService _service) : ControllerBase
    {   
        private string GetUserId() => User.FindFirstValue("sub")!;

        // GET /api/TicketApplications/{ticketId}
        [HttpGet("{ticketId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> GetApplications(Guid ticketId)
        {
            var result = await _service.GetApplicationsAsync(ticketId, GetUserId());
            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tamkeen.Application.DTOs.AI;
using Tamkeen.Application.Interfaces.AI;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        // POST /api/AI/maintenance-advice
        [HttpPost("maintenance-advice")]
        public async Task<IActionResult> GetAdvice([FromBody] MaintenanceAdviceRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Problem))
                return BadRequest(new { message = "اكتب وصف المشكلة" });

            var result = await _aiService.GetMaintenanceAdviceAsync(dto.Problem);
            return Ok(result);
        }
        [HttpPost("maintenance-schedule")]
        public async Task<IActionResult> GetMaintenanceSchedule([FromBody] MaintenanceScheduleRequestDto dto)
        {
            if (dto == null) return BadRequest();
            var result = await _aiService.GetMaintenanceScheduleAsync(dto);
            return Ok(result);
        }
    }
}
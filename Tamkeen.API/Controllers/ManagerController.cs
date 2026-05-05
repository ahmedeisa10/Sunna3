using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tamkeen.Application.Interfaces;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        // GET /api/manager/tenants
        [HttpGet("tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var result = await _managerService.GetAllTenantsAsync();
            return Ok(result);
        }

        // GET /api/manager/vendors
        [HttpGet("vendors")]
        public async Task<IActionResult> GetAllVendors()
        {
            var result = await _managerService.GetAllVendorsAsync();
            return Ok(result);
        }
    }
}

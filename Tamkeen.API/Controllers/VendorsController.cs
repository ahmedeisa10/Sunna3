using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tamkeen.Application.DTOs;
using Tamkeen.Application.Interfaces.Vendor;

namespace Tamkeen.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _service;

        public VendorsController(IVendorService service)
        {
            _service = service;
        }

        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile([FromForm] CreateVendorProfileDto dto)
        {
            await _service.CreateProfileAsync(dto);

            return Ok(new { message = "Profile created successfully" });
        }
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllVendors()
        {
            var vendors = await _service.GetAllVendorsAsync();
            return Ok(vendors);
        }
        [HttpGet("{vendorId}/profile")]
        public async Task<IActionResult> GetProfile(string vendorId)
        {
            var profile = await _service.GetVendorProfileAsync(vendorId);
            return Ok(profile);
        }
    }
}

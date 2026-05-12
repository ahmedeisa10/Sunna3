using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tamkeen.Application.Interfaces.Manager;

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


        //Users

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



        //Payouts
        // GET /api/manager/payouts/pending  => Payments that have not yet been disbursed
        [HttpGet("payouts/pending")]
        public async Task<IActionResult> GetPendingPayouts()
        {
            var result = await _managerService.GetPendingPayoutsAsync();
            return Ok(result);
        }

        // GET /api/manager/payouts  => All Payouts 
        [HttpGet("payouts")]
        public async Task<IActionResult> GetAllPayouts()
        {
            var result = await _managerService.GetAllPayoutsAsync();
            return Ok(result);
        }

        // PATCH /api/manager/payouts/{id}/confirm  => Confirm Payout
        [HttpPatch("payouts/{id:guid}/confirm")]
        public async Task<IActionResult> ConfirmPayout(Guid id)
        {
            await _managerService.ConfirmPayoutAsync(id);
            return Ok(new { message = "تم تأكيد الصرف بنجاح" });
        }

    }
}

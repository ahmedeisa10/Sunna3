using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Tamkeen.Application.DTOs.Manager_Management;
using Tamkeen.Application.DTOs.Vendor;
using Tamkeen.Domain.Entities;

namespace Tamkeen.Infrastructure.Implementation
{
    public class ManagerService
    {
        private readonly UserManager<AppUser> _userManager;

        public ManagerService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // ── كل الـ Tenants ──────────────────────────────────
        public async Task<IEnumerable<AllTenantsDto>> GetAllTenantsAsync()
        {
            var tenants = await _userManager.GetUsersInRoleAsync("Tenant");

            return tenants.Select(u => new AllTenantsDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!,
                Phone = u.PhoneNumber!
            });
        }

        // ── كل الـ Vendors ──────────────────────────────────
        public async Task<IEnumerable<AllVendorsDto>> GetAllVendorsAsync()
        {
            var vendors = await _userManager.GetUsersInRoleAsync("Vendor");

            return vendors.Select(u => new AllVendorsDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!,
                Phone = u.PhoneNumber!,
                ImageUrl = u.ImageUrl       
            });
        }
    }
}

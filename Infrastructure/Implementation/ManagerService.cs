using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamkeen.Application.DTOs.Manager_Management;
using Tamkeen.Application.DTOs.Payment_DTOs;
using Tamkeen.Application.DTOs.Vendor;
using Tamkeen.Application.Interfaces;
using Tamkeen.Domain.Entities;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Implementation
{
    public class ManagerService: IManagerService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext context;

        public ManagerService(UserManager<AppUser> userManager,AppDbContext context)
        {
            _userManager = userManager;
            this.context = context;
        }

        //Users
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




        //Payouts
        //Payments that have not yet been disbursed
        public async Task<IEnumerable<VendorPayoutDto>> GetPendingPayoutsAsync()
        {
            var payments = await context.Payments
                .Include(p => p.Vendor)
                .Where(p => p.IsPaid && !p.IsDisbursed)
                .OrderBy(p => p.PaidAt)
                .ToListAsync();

            return payments.Select(p => new VendorPayoutDto
            {
                PaymentId = p.Id,
                TicketId = p.TicketId,
                VendorId = p.VendorId,
                VendorName = p.Vendor.FullName,
                VendorIBAN = p.Vendor.IBAN,
                VendorInstapay = p.Vendor.InstapayNumber,
                VendorWallet = p.Vendor.WalletNumber,
                VendorAmount = p.VendorAmount,
                PlatformAmount = p.PlatformAmount,
                TotalAmount = p.TotalAmount,
                PaymentMethod = p.PaymentMethod,
                PaidAt = p.PaidAt!.Value,
                IsDisbursed = p.IsDisbursed,
                DisbursedAt = p.DisbursedAt
            });
        }

        // All Payouts
        public async Task<IEnumerable<VendorPayoutDto>> GetAllPayoutsAsync()
        {
            var payments = await context.Payments
                .Include(p => p.Vendor)
                .Where(p => p.IsPaid)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            return payments.Select(p => new VendorPayoutDto
            {
                PaymentId = p.Id,
                TicketId = p.TicketId,
                VendorId = p.VendorId,
                VendorName = p.Vendor.FullName,
                VendorIBAN = p.Vendor.IBAN,
                VendorInstapay = p.Vendor.InstapayNumber,
                VendorWallet = p.Vendor.WalletNumber,
                VendorAmount = p.VendorAmount,
                PlatformAmount = p.PlatformAmount,
                TotalAmount = p.TotalAmount,
                PaymentMethod = p.PaymentMethod,
                PaidAt = p.PaidAt!.Value,
                IsDisbursed = p.IsDisbursed,
                DisbursedAt = p.DisbursedAt
            });
        }

        // Confirm Payout
        public async Task ConfirmPayoutAsync(Guid paymentId)
        {
            var payment = await context.Payments.FindAsync(paymentId)
                ?? throw new NotFoundException("Payment not found");

            if (!payment.IsPaid)
                throw new BadRequestException("الدفع لسه ما اتمش");

            if (payment.IsDisbursed)
                throw new BadRequestException("المبلغ ده اتصرف قبل كده");

            payment.IsDisbursed = true;
            payment.DisbursedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamkeen.Application.DTOs.Manager_Management;
using Tamkeen.Application.DTOs.Payment_DTOs;
using Tamkeen.Application.DTOs.Vendor;

namespace Tamkeen.Application.Interfaces
{
    public interface IManagerService
    {
            //Users
            Task<IEnumerable<AllTenantsDto>> GetAllTenantsAsync();
            Task<IEnumerable<AllVendorsDto>> GetAllVendorsAsync();

        //Payouts
        Task<IEnumerable<VendorPayoutDto>> GetPendingPayoutsAsync();   // Payments that have not yet been disbursed
        Task<IEnumerable<VendorPayoutDto>> GetAllPayoutsAsync();       // All Payouts
        Task ConfirmPayoutAsync(Guid paymentId);                       // Confirm Payout
    }
}

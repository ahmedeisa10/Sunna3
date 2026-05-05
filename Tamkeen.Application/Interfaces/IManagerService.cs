using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamkeen.Application.DTOs.Manager_Management;
using Tamkeen.Application.DTOs.Vendor;

namespace Tamkeen.Application.Interfaces
{
    public interface IManagerService
    {
            Task<IEnumerable<AllTenantsDto>> GetAllTenantsAsync();
            Task<IEnumerable<AllVendorsDto>> GetAllVendorsAsync();
        
    }
}

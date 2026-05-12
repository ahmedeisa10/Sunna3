using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamkeen.Application.DTOs;

namespace Tamkeen.Application.Interfaces.Invitation
{
    public interface IInvitationService
    {
        Task<string> CreateInvitationAsync(string phone);
        Task<(bool Success, string Message, string? Token, string? ProfileImageUrl)> RegisterVendorAsync(VendorRegisterDto dto);
    }
}

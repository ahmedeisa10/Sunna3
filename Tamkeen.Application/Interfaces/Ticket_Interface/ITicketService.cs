using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tamkeen.Application.DTOs.Ticket_DTOs;

namespace Tamkeen.Application.Interfaces.Ticket_Interface
{
    public interface ITicketService
    {
        Task<TicketResponseDto> CreateAsync(CreateTicketDto dto, string tenantId);
        Task<TicketResponseDto> GetByIdAsync(Guid id, string userId, string role);
        Task<IEnumerable<TicketResponseDto>> GetAllAsync(string userId, string role,
            string? governorate = null, string? city = null);

        //========MANAGER==============
        Task<IEnumerable<TicketResponseDto>> GetManagerReviewAsync();   // Reauests Waiting for manager approval
        Task ApproveAsync(Guid ticketId);                                  // manager accept >> forward to pending
        Task RejectAsync(Guid ticketId);                    // manager reject >> still in DB but reject

        //==============VENDOR===============
        Task<IEnumerable<TicketResponseDto>> GetPendingAsync(
            string? governorate = null, string? city = null);             // Pending Request only >> shown for vendors
        Task ApplyAsync(Guid ticketId, string vendorId);

        //============TENANT===============
        Task AcceptApplicationAsync(Guid applicationId, string tenantId);
        Task CloseAsync(Guid id, string tenantId);

        Task<List<ImageResponseDto>> CompleteWithImagesAsync(
            Guid ticketId, CompleteTicketDto dto, string vendorId);
    }

}

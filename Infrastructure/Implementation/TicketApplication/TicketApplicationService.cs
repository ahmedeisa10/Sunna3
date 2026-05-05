using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Tamkeen.Application.DTOs.TicketApplication;
using Tamkeen.Application.Interfaces;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Services
{
    public class TicketApplicationService(AppDbContext _context) : ITicketApplicationService
    {
        public async Task<IEnumerable<TicketApplicationDto>> GetApplicationsAsync(
            Guid ticketId, string tenantId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.TenantId != tenantId)
                throw new ForbiddenException("Access denied");

            var applications = await _context.TicketApplications
                .Include(a => a.Vendor)
                .Where(a => a.TicketId == ticketId)
                .OrderBy(a => a.AppliedAt)
                .ToListAsync();

            return applications.Select(a => new TicketApplicationDto
            {
                Id = a.Id,
                VendorId = a.VendorId,
                VendorName = a.Vendor.FullName,
                VendorPhone = a.Vendor.PhoneNumber,
                AppliedAt = a.AppliedAt
            });
        }

    }
}
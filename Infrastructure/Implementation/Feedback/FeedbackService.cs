using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Tamkeen.Application.DTOs.Feedback;
using Tamkeen.Application.Interfaces.Feedback;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Services
{
    public class FeedbackService(AppDbContext _context) : IFeedbackService
    {
        public async Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, string tenantId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.TenantId != tenantId)
                throw new ForbiddenException("Access denied");

            if (ticket.Status != RequestStatus.Closed)
                throw new BadRequestException("مينفعش تكتب فيدباك غير بعد إنجاز التيكيت");
            
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Comment = dto.Comment,
                TicketId = dto.TicketId,
                TenantId = tenantId,
                VendorId = dto.VendorId
            };

            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            var tenant = await _context.Users.FindAsync(tenantId);

            return new FeedbackDto
            {
                Id = feedback.Id,
                Comment = feedback.Comment ?? "",
                TenantName = tenant?.FullName ?? "مجهول",
                TenantId = tenantId
            };
        }

        public async Task<IEnumerable<FeedbackDto>> GetByTicketAsync(Guid ticketId)
        {
            return await _context.Feedbacks
                .Include(f => f.Tenant)
                .Where(f => f.TicketId == ticketId)
                .OrderByDescending(f => f.Id)
                .Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    Comment = f.Comment ?? "",
                    TenantName = f.Tenant != null ? f.Tenant.FullName : "مجهول",
                    TenantId = f.TenantId
                })
                .ToListAsync();
        }
    }
}
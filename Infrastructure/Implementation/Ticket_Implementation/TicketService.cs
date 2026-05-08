using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Tamkeen.Application.DTOs.Ticket_DTOs;
using Tamkeen.Application.Interfaces;
using Tamkeen.Application.Interfaces.Ticket_Interface;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Data;
using Tamkeen.Infrastructure.Services;

namespace Tamkeen.Infrastructure.Implementation.Ticket_Implementation
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService imageService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;

        public TicketService(
            AppDbContext context,
            IMapper mapper,
            IImageService imageService,
            INotificationService notificationService,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            this.imageService = imageService;
            _notificationService = notificationService;
            _userManager = userManager;
        }
        private async Task<List<string>> GetManagerIdsAsync()
        {
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            return managers.Select(m => m.Id).ToList();
        }

        public async Task<TicketResponseDto> CreateAsync(CreateTicketDto dto, string tenantId)
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Description = dto.Description,
                Priority = dto.Priority,
                TenantLocation = dto.TenantLocation,
                Governorate = dto.Governorate,
                City = dto.City,
                problemType = dto.problemType,
                Arrival = dto.Arrival,
                Deadline = dto.Deadline,
                CompanyId = dto.CompanyId,
                TenantId = tenantId,
                Status = RequestStatus.ManagerReview,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Images != null && dto.Images.Any())
            {
                ticket.Images = new List<Image>();
                foreach (var file in dto.Images)
                {
                    var url = await imageService.SaveImageAsync(file, "tickets");
                    ticket.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        Url = url,
                        Type = ImageType.Before,
                        TicketId = ticket.Id
                    });
                }
            }

            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
            var managerIds = await GetManagerIdsAsync();
            var notifyTasks = managerIds.Select(mid =>
                _notificationService.NotifyNewTicketAsync(mid, ticket.Id.ToString(), ticket.Description));
            await Task.WhenAll(notifyTasks);
            
            return _mapper.Map<TicketResponseDto>(ticket);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetPendingAsync(
            string? governorate = null, string? city = null)
        {
            var query = _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Images)
                .Where(t => t.Status == RequestStatus.Pending && t.VendorId == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(governorate))
                query = query.Where(t => t.Governorate == governorate);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(t => t.City == city);

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        public async Task ApplyAsync(Guid ticketId, string vendorId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.Status != RequestStatus.Pending)
                throw new BadRequestException("التيكيت دي مش متاحة للتقديم");

            if (ticket.VendorId != null)
                throw new BadRequestException("التيكيت دي اتكلفت بالفعل");

            var alreadyApplied = await _context.TicketApplications
                .AnyAsync(a => a.TicketId == ticketId && a.VendorId == vendorId);

            if (alreadyApplied)
                throw new BadRequestException("قدمت على التيكيت دي قبل كده");

            var application = new TicketApplication
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                VendorId = vendorId,
                AppliedAt = DateTime.UtcNow
            };

            await _context.TicketApplications.AddAsync(application);
            await _context.SaveChangesAsync();
            var vendor = await _userManager.FindByIdAsync(vendorId);
            var managerIds = await GetManagerIdsAsync();
            var notifyTasks = managerIds.Select(mid =>
                _notificationService.NotifyNewTicketAsync(
                    mid,
                    ticketId.ToString(),
                    $"فني {vendor?.FullName} تقدم على طلب: {ticket.Description}"));
            await Task.WhenAll(notifyTasks);
        }

        public async Task AcceptApplicationAsync(Guid applicationId, string tenantId)
        {
            var application = await _context.TicketApplications
                .Include(a => a.Ticket)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new NotFoundException("Application not found");

            if (application.Ticket.TenantId != tenantId)
                throw new ForbiddenException("Access denied");

            if (application.Ticket.Status != RequestStatus.Pending)
                throw new BadRequestException("التيكيت دي اتكلفت بالفعل");

            application.Ticket.VendorId = application.VendorId;
            application.Ticket.Status = RequestStatus.vendorAccepted;

            var otherApplications = await _context.TicketApplications.ToListAsync();
            _context.TicketApplications.RemoveRange(otherApplications);

            await _context.SaveChangesAsync();
            await _notificationService.NotifyVendorAssignedAsync(
                application.VendorId,
                application.TicketId.ToString(),
                application.Ticket.Description);
        }

        public async Task<TicketResponseDto> GetByIdAsync(Guid id, string userId, string role)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Vendor)
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException("Ticket not found");

            if (role == "Manager")
                return _mapper.Map<TicketResponseDto>(ticket);

            if (role == "Tenant" && ticket.TenantId != userId)
                throw new ForbiddenException("Access denied");

            if (role == "Vendor" && ticket.VendorId != userId)
                throw new ForbiddenException("Access denied");

            return _mapper.Map<TicketResponseDto>(ticket);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetAllAsync(
            string userId, string role,
            string? governorate = null, string? city = null)
        {
            var query = _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Vendor)
                .Include(t => t.Images)
                .AsQueryable();

            query = role switch
            {
                "Tenant" => query.Where(t => t.TenantId == userId),
                "Vendor" => query.Where(t => t.VendorId == userId),
                _ => query
            };

            if (!string.IsNullOrWhiteSpace(governorate))
                query = query.Where(t => t.Governorate == governorate);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(t => t.City == city);

            var tickets = await query.ToListAsync();
            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        public async Task<List<ImageResponseDto>> CompleteWithImagesAsync(
            Guid ticketId, CompleteTicketDto dto, string vendorId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.VendorId != vendorId)
                throw new ForbiddenException("Not your ticket");

            if (ticket.Status != RequestStatus.vendorAccepted)
                throw new BadRequestException("التيكيت لازم تكون جارٍ تنفيذها الأول");

            if (dto.Images == null || !dto.Images.Any())
                throw new BadRequestException("لازم ترفع صورة واحدة على الأقل قبل الإنهاء");

            var savedImages = new List<Image>();
            foreach (var file in dto.Images)
            {
                var url = await imageService.SaveImageAsync(file, "tickets");
                savedImages.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    Url = url,
                    Type = ImageType.After,
                    TicketId = ticketId
                });
            }
            ticket.Price = dto.Price;

            await _context.Images.AddRangeAsync(savedImages);
            ticket.Status = RequestStatus.Resolved;
            await _context.SaveChangesAsync();

            var managerIds = await GetManagerIdsAsync();
            var notifyTasks = managerIds.Select(mid =>
                _notificationService.NotifyTicketStatusChangedAsync(
                    mid, ticketId.ToString(), "Resolved"));
            await Task.WhenAll(notifyTasks);

            return _mapper.Map<List<ImageResponseDto>>(savedImages);
        }

        public async Task CloseAsync(Guid id, string tenantId)
        {
            var ticket = await _context.Tickets.FindAsync(id)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.TenantId != tenantId)
                throw new ForbiddenException("Not your ticket");

            if (ticket.Status != RequestStatus.Resolved)
                throw new BadRequestException("Ticket must be resolved first");
            if (!ticket.IsPaid)
                throw new BadRequestException("لازم تدفع الأول قبل الإغلاق");

            ticket.Status = RequestStatus.Closed;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TicketResponseDto>> GetManagerReviewAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Images)
                .Where(t => t.Status == RequestStatus.ManagerReview)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        public async Task ApproveAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.Status != RequestStatus.ManagerReview)
                throw new BadRequestException("الطلب مش في انتظار الموافقة");

            ticket.Status = RequestStatus.Pending;
            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId)
               ?? throw new NotFoundException("Ticket not found");

            if (ticket.Status != RequestStatus.ManagerReview)
                throw new BadRequestException("الطلب مش في انتظار الموافقة");

            ticket.Status = RequestStatus.Rejected;
            await _context.SaveChangesAsync();
        }
    }
}
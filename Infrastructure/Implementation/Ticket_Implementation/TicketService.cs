using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Tamkeen.Application.DTOs.Ticket_DTOs;
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

        public TicketService(AppDbContext context, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            this.imageService = imageService;
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

            // If there are pictures, save them
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

            return _mapper.Map<TicketResponseDto>(ticket);
        }

        //Shown For Vendors
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

        //   Vendor apply for a ticket 
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
        }
        // Tenant accepts a specific vendor → deletes the remaining applications
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

            // Delete all other applications on the same ticket ──
            var otherApplications = await _context.TicketApplications
                .ToListAsync();

            _context.TicketApplications.RemoveRange(otherApplications);

            // ── Delete the accepted applications too ──
            //_context.TicketApplications.Remove(application);

            await _context.SaveChangesAsync();
        }
        public async Task<TicketResponseDto> GetByIdAsync(Guid id, string userId, string role)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Vendor)
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new NotFoundException("Ticket not found");

            // Manager access any ticket
            if (role == "Manager")
                return _mapper.Map<TicketResponseDto>(ticket);

            // Tenant only access his tickets
            if (role == "Tenant" && ticket.TenantId != userId)
                throw new ForbiddenException("Access denied");

            // Vendor only sees what is assigned to him
            if (role == "Vendor" && ticket.VendorId != userId)
                throw new ForbiddenException("Access denied");

            return _mapper.Map<TicketResponseDto>(ticket);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetAllAsync(string userId, string role, string? governorate = null, string? city = null)
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
                _ => query // Manager see all
            };

            if (!string.IsNullOrWhiteSpace(governorate))
                query = query.Where(t => t.Governorate == governorate);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(t => t.City == city);

            var tickets = await query.ToListAsync();
            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        #region old methods without applications
        //public async Task VendorApplayingAsync(Guid id, AssignTicketDto dto)
        //{
        //    var ticket = await _context.Tickets.FindAsync(id)
        //        ?? throw new NotFoundException("Ticket not found");

        //    if (ticket.Status != RequestStatus.Pending)
        //        throw new BadRequestException("Only pending tickets can be assigned");
        //    ticket.VendorId = dto.VendorId;
        //    //ticket.Status = RequestStatus.Assigned;
        //    await _context.SaveChangesAsync();
        //}

        //public async Task AcceptAsync(Guid id, string vendorId)
        //{
        //    var ticket = await _context.Tickets.FindAsync(id)
        //        ?? throw new NotFoundException("Ticket not found");

        //    if (ticket.Status != RequestStatus.Pending)
        //        throw new BadRequestException("Ticket must be assigned first");
        //    ticket.VendorId = vendorId;
        //    ticket.Status = RequestStatus.InProgress;
        //    await _context.SaveChangesAsync();
        //}

        //public async Task RejectAsync(Guid id, string vendorId)
        //{
        //    var ticket = await _context.Tickets.FindAsync(id)
        //        ?? throw new NotFoundException("Ticket not found");

        //    if (ticket.VendorId != vendorId)
        //        throw new ForbiddenException("Not your ticket");

        //    if (ticket.Status != RequestStatus.Assigned)
        //        throw new BadRequestException("Ticket must be assigned first");

        //    // يرجع Pending وينزع الـ Vendor
        //    ticket.Status = RequestStatus.Pending;
        //    ticket.VendorId = null;
        //    await _context.SaveChangesAsync();
        //}
        #endregion
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

            // ── رفع صور After ──
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

            await _context.Images.AddRangeAsync(savedImages);

            // ── تغيير الـ Status لـ Resolved ──
            ticket.Status = RequestStatus.Resolved;

            await _context.SaveChangesAsync();

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
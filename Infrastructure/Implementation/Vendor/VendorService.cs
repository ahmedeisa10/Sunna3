using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Tamkeen.Application.DTOs;
using Tamkeen.Application.DTOs.Feedback;
using Tamkeen.Application.DTOs.Vendor;
using Tamkeen.Application.Interfaces.Vendor;
using Tamkeen.Domain.Entities;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Services
{
    public class VendorService(AppDbContext _context, IImageService imageService) : IVendorService
    {
        public async Task<VendorProfileDto> GetVendorProfileAsync(string vendorId)
        {
            var vendor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == vendorId)
                ?? throw new NotFoundException("Vendor not found");

            var tickets = await _context.Tickets
                .Include(t => t.Feedbacks)
                    .ThenInclude(f => f.Tenant)
                .Where(t => t.VendorId == vendorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return new VendorProfileDto
            {
                VendorId = vendor.Id,
                FullName = vendor.FullName,
                Phone = vendor.PhoneNumber,
                ImageUrl = vendor.ImageUrl,
                TicketHistory = tickets.Select(t => new VendorTicketHistoryDto
                {
                    TicketId = t.Id,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    Governorate = t.Governorate,
                    City = t.City,
                    CreatedAt = t.CreatedAt,
                    Feedbacks = t.Feedbacks.Select(f => new FeedbackDto
                    {
                        Id = f.Id,
                        Comment = f.Comment ?? "",
                        TenantName = f.Tenant?.FullName ?? "مجهول",
                        TenantId = f.TenantId
                    }).ToList()
                }).ToList()
            };

        }
        public async Task CreateProfileAsync(CreateVendorProfileDto dto)
        {
            string frontPath = await imageService.SaveIdCard(dto.IdCardFront, "front");
            string backPath = await imageService.SaveIdCard(dto.IdCardBack, "back");
            var profile = new VendorProfile
            {
                fullName = dto.FullName,

                phone = dto.PhoneNumber,
                specialty = dto.Specialization,

                yearsExperience = dto.YearsOfExperience,
                bio = dto.Bio,
                IdCardFront = frontPath,
                IdCardBack = backPath
            };

            _context.vendorProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<VendorResponseDto>> GetAllVendorsAsync()
        {
            return await _context.vendorProfiles
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VendorResponseDto
                {
                    fullName = v.fullName,
                    phone = v.phone,
                    specialty = v.specialty,
                    yearsExperience = v.yearsExperience,
                    bio = v.bio,
                    IdCardFront = v.IdCardFront,
                    IdCardBack = v.IdCardBack,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();
        }
    }
}
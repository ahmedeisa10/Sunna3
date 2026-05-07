using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tamkeen.Application.DTOs;
using Tamkeen.Application.Interfaces;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Data;

namespace Tamkeen.Infrastructure.Implementation
{
    public class InvitationService : IInvitationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public InvitationService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            AppDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        // ===== Manager make Invitation =====
        public async Task<string> CreateInvitationAsync(string phone)
        {
            var token = Guid.NewGuid().ToString("N");

            var invitation = new VendorInvitation
            {
                Phone = phone,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(3)
            };

            _context.vendorInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Registration Link
            var registrationLink = $"http://localhost:4200/vendor-register?token={token}";

            // Whatsapp Message
            var message = $"🎉 مبارك انضمامك لينا!\n\n" +
                          $"أهلاً بيك في فريقنا 👋\n" +
                          $"يسعدنا وجودك معانا 💙\n\n" +
                          $"سجل من هنا 👇\n{registrationLink}";

            // encoding message
            var encodedText = Uri.EscapeDataString(message);

            // whatsapp Link
            var whatsappLink = $"https://wa.me/{phone}?text={encodedText}";

            return whatsappLink;
        }
        public async Task<(bool Success, string Message, string? Token, string? ProfileImageUrl)> RegisterVendorAsync(VendorRegisterDto dto)
        {
            var invitation = await _context.vendorInvitations
                .FirstOrDefaultAsync(x => x.Token == dto.Token);

            if (invitation == null)
                return (false, "Invalid invitation link.", null, null);

            if (invitation.IsUsed)
                return (false, "This invitation has already been used.", null, null);

            if (invitation.ExpiresAt < DateTime.UtcNow)
                return (false, "Invitation link has expired.", null, null);

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return (false, "Email already exists.", null, null);

            if (dto.Image == null || dto.Image.Length == 0)
                return (false, "Image is required.", null, null);

            if (dto.Image.Length > 2 * 1024 * 1024)
                return (false, "Image size must be less than 2MB.", null, null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(dto.Image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return (false, "Only JPG and PNG images are allowed.", null, null);

            if (!dto.Image.ContentType.StartsWith("image/"))
                return (false, "Invalid image file.", null, null);

            if (!dto.Image.ContentType.StartsWith("image/"))
                return (false, "Invalid image file.", null, null);

            string imagePath = string.Empty;

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/vendors");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                imagePath = "images/vendors/" + fileName;
            }
            catch
            {
                return (false, "Error while uploading image.", null, null);
            }

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                ImageUrl = imagePath,
                EmailConfirmed = true,

                // Money
                IBAN = dto.IBAN,
                BankName = dto.BankName,
                InstapayNumber = dto.InstapayNumber,
                WalletNumber = dto.WalletNumber
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null, null);

            await _userManager.AddToRoleAsync(user, UserRole.Vendor.ToString());

            invitation.IsUsed = true;
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

            return (true, "Vendor registered successfully.", token, imagePath);
        }
   
    
    }
}

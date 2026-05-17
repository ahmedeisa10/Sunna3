using Microsoft.AspNetCore.Identity;
using Tamkeen.Application.DTOs.Account;
using Tamkeen.Application.Interfaces.Account;
using Tamkeen.Domain.Entities;

namespace Tamkeen.Infrastructure.Implementation.Account
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // ── GET PROFILE ───────────────────────────────────────
        public async Task<ProfileResponseDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("User not found");

            return new ProfileResponseDto
            {
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };
        }

        // ── UPDATE PROFILE ────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "المستخدم مش موجود");

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return (false, "الاسم مطلوب");

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, "تم تحديث البيانات بنجاح");
        }

        // ── CHANGE PASSWORD ───────────────────────────────────
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "المستخدم مش موجود");

            // Validate
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return (false, "ادخل كلمة السر الحالية");

            if (dto.NewPassword.Length < 6)
                return (false, "كلمة السر الجديدة 6 أحرف على الأقل");

            if (dto.NewPassword != dto.ConfirmPassword)
                return (false, "كلمتا السر مش متطابقتين");

            // Check current password
            var isCorrect = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!isCorrect)
                return (false, "كلمة السر الحالية غلط");

            // Change
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, "تم تغيير كلمة السر بنجاح");
        }
    }
}
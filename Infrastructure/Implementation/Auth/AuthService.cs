using Google.Apis.Auth;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tamkeen.Application.DTOs.Auth;
using Tamkeen.Application.Interfaces.Auth;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Setting;

namespace Tamkeen.Infrastructure.Implementation.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        // ================= REGISTER =================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return (false, "Email already exists.");

            var code = new Random().Next(100000, 999999).ToString();

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmationCode = ConfirmationCodeHasher.Hash(code),
                CodeExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, UserRole.Tenant.ToString());

            await _emailService.SendConfirmationEmail(dto.Email, code);

            return (true, "Registered successfully. Check your email.");
        }

        // ================= CONFIRM EMAIL =================
        public async Task<(bool Success, ConfirmEmailResponseDto? Data, string Message)> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.EmailConfirmed)
                return (false, null, "Invalid code.");

            // ✅ verify instead of hash compare
            var isValid = ConfirmationCodeHasher.Verify(dto.Code, user.EmailConfirmationCode);

            if (!isValid)
                return (false, null, "Invalid code.");

            if (user.CodeExpiry < DateTime.UtcNow)
                return (false, null, "Code expired.");

            user.EmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.CodeExpiry = null;

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

            return (true, new ConfirmEmailResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles
            }, "Email confirmed successfully.");
        }

        // ================= LOGIN =================
        public async Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return (false, null, "Invalid credentials.");

            if (!user.EmailConfirmed)
                return (false, null, "Please confirm your email first.");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                return (false, null, "Invalid credentials.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

            return (true, new AuthResponseDto
            {
                token = token,
                email = user.Email!,
                fullName = user.FullName,
                roles = roles
            }, "Login successful.");
        }

        // ================= RESEND CODE =================
        public async Task<(bool Success, string Message)> ResendCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (false, "User not found.");

            if (user.EmailConfirmed)
                return (false, "Email already confirmed.");

            var code = new Random().Next(100000, 999999).ToString();

            user.EmailConfirmationCode = ConfirmationCodeHasher.Hash(code);
            user.CodeExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            await _emailService.SendConfirmationEmail(email, code);

            return (true, "Code sent again.");
        }
        // sign in with google
        public async Task<(bool Success, AuthResponseDto? Data, string Message)> GoogleLoginAsync(GoogleLoginDto dto)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch
            {
                return (false, null, "Google token غير صالح");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FullName = payload.Name,
                    EmailConfirmed = true,
                    ImageUrl = payload.Picture
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return (false, null, "فشل إنشاء الحساب");

                await _userManager.AddToRoleAsync(user, "Tenant");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

            return (true, new AuthResponseDto
            {
                token = token,
                email = user.Email!,
                fullName = user.FullName,
                roles = roles
            }, "تم تسجيل الدخول");
        }
    }
}
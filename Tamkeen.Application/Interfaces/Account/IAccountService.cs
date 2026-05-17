using Tamkeen.Application.DTOs.Account;

namespace Tamkeen.Application.Interfaces.Account
{
    public interface IAccountService
    {
        Task<ProfileResponseDto> GetProfileAsync(string userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}

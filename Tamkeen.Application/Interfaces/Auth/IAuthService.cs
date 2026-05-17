using Tamkeen.Application.DTOs.Auth;
namespace Tamkeen.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, ConfirmEmailResponseDto? Data, string Message)> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginDto dto);
        Task<(bool Success, string Message)> ResendCodeAsync(string email);
        Task<(bool Success, AuthResponseDto? Data, string Message)> GoogleLoginAsync(GoogleLoginDto dto);
    }
}
// response 
// 1. throw exception (adv: trace exception , disad:performacne)
// 2. result object with success, message, data (if needed) ()
// class Result<T>
// {
//     public bool Success { get; set; }
//     public string Message { get; set; }
//     public T? Data { get; set; }
// }
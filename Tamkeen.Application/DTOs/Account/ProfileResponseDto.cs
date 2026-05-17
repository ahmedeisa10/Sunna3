namespace Tamkeen.Application.DTOs.Account
{
    public class ProfileResponseDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
    }
}

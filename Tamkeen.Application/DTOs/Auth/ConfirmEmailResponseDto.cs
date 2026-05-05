namespace Tamkeen.Application.DTOs.Auth
{
    public class ConfirmEmailResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
    }
}

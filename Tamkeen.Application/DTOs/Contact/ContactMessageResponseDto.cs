namespace Tamkeen.Application.DTOs.Contact
{
    public class ContactMessageResponseDto
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public string? TenantPhone { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}

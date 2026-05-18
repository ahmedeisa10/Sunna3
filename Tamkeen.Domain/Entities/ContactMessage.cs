namespace Tamkeen.Domain.Entities
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public AppUser Tenant { get; set; } = null!; 

        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

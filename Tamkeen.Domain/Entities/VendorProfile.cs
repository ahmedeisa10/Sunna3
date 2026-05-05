namespace Tamkeen.Domain.Entities
{
    public class VendorProfile
    {
        public Guid Id { get; set; }
        public string fullName { get; set; } = null!;
        public string phone { get; set; } = null!;
        public string specialty { get; set; } = null!;
        public int yearsExperience { get; set; }
        public string? bio { get; set; } = null!;
        public string IdCardFront { get; set; } = null!;
        public string IdCardBack { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       

    }
}

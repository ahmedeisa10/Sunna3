namespace Tamkeen.Application.DTOs.Vendor
{
    public class VendorProfileDto
    {
        public string VendorId { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string specialty { get; set; } = null!;
        public int yearsExperience { get; set; }
        public string? bio { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public List<VendorTicketHistoryDto> TicketHistory { get; set; }
    }
}

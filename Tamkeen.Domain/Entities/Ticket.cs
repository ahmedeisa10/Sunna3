using Tamkeen.Domain.Enums;
namespace Tamkeen.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string TenantLocation { get; set; }
        public string Governorate { get; set; }
        public string City { get; set; }
        public decimal Price { get; set; }
        public string problemType { get; set; }
        public RequestStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateTime Arrival { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public string TenantId { get; set; }
        public AppUser Tenant { get; set; }
        public string? VendorId { get; set; }
        public AppUser Vendor { get; set; }
        public bool IsPaid { get; set; }
        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }
        public ICollection<Image> Images { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<TicketApplication> Applications { get; set; }
       
    }
}

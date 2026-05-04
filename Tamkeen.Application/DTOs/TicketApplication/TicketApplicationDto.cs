namespace Tamkeen.Application.DTOs.TicketApplication
{
    public class TicketApplicationDto
    {
        public Guid Id { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string? VendorPhone { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}

namespace Tamkeen.Application.DTOs.Feedback
{
    public class CreateFeedbackDto
    {
        public string Comment { get; set; }
        public Guid TicketId { get; set; }
        public string VendorId { get; set; }
    }
}

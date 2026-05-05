namespace Tamkeen.Application.DTOs.Feedback
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public string TenantName { get; set; }
        public string TenantId { get; set; }
    }
}

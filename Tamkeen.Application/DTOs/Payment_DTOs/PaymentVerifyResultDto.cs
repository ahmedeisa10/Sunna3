namespace Tamkeen.Application.DTOs.Payment_DTOs
{
    // DTO جديد
    public class PaymentVerifyResultDto
    {
        public bool IsPaid { get; set; }
        public Guid? TicketId { get; set; }
    }
}

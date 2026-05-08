using Tamkeen.Application.DTOs.Payment_DTOs;

namespace Tamkeen.Application.Interfaces.Payments
{
    public interface IPaymentService
    {
        // الـ tenant يبدأ الدفع
        Task<PaymentResponseDto> InitiatePaymentAsync(
            InitiatePaymentDto dto, string tenantId);

        // Paymob بيبعت webhook لما الدفع يتم
        Task HandleWebhookAsync(string payload, string hmac);
        Task<bool> VerifyAndSyncPaymentAsync(Guid paymentId);
    }
}

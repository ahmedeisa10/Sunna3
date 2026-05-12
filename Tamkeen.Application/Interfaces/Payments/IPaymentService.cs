using Tamkeen.Application.DTOs.Payment_DTOs;

namespace Tamkeen.Application.Interfaces.Payments
{
    public interface IPaymentService
    {
        // Tenant start Payment
        Task<PaymentResponseDto> InitiatePaymentAsync(
            InitiatePaymentDto dto, string tenantId);

        // Paymob send webhook when payment is done
        Task HandleWebhookAsync(string payload, string hmac);
        Task<PaymentVerifyResultDto> VerifyAndSyncPaymentAsync(Guid paymentId);
        Task<PaymentVerifyResultDto> ConfirmFromCallbackAsync(CallbackConfirmDto dto);
    }
}

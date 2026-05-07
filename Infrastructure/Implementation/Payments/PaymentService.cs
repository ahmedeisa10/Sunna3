using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Errors.Model;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tamkeen.Application.DTOs.Payment_DTOs;
using Tamkeen.Application.Interfaces.Payments;
using Tamkeen.Domain.Entities;
using Tamkeen.Domain.Enums;
using Tamkeen.Infrastructure.Data;
using Tamkeen.Infrastructure.Setting;

namespace Tamkeen.Infrastructure.Implementation.Payments
{

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly PaymobService _paymob;
        private readonly PaymobSettings _settings;

        public PaymentService(
            AppDbContext context,
            PaymobService paymob,
            IOptions<PaymobSettings> settings)
        {
            _context = context;
            _paymob = paymob;
            _settings = settings.Value;
        }

        // ── الـ Tenant يبدأ الدفع ──────────────────────────
        public async Task<PaymentResponseDto> InitiatePaymentAsync(
            InitiatePaymentDto dto, string tenantId)
        {
            // جيب التيكيت
            var ticket = await _context.Tickets
                .Include(t => t.Tenant)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.Status != RequestStatus.Resolved)
                throw new BadRequestException("لازم الـ vendor يخلص شغله الأول");


            if (ticket.TenantId != tenantId)
                throw new ForbiddenException("Access denied");

            // احسب المبالغ
            decimal total =ticket.Price;           // لازم يبقى عندك Price في الـ Ticket
            decimal platformAmount = total * 0.10m;          // 10% للمنصة
            decimal vendorAmount = total * 0.90m;          // 90% للـ vendor
            int amountInCents = (int)(total * 100);     // Paymob بيشتغل بالقروش

            // سجل الـ Payment في الـ DB
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                TicketId = dto.TicketId,
                TenantId = tenantId,
                VendorId = ticket.VendorId!,
                TotalAmount = total,
                PlatformAmount = platformAmount,
                VendorAmount = vendorAmount,
                PaymentMethod = dto.PaymentMethod,
                WalletNumber = dto.WalletNumber,
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // ── الـ 3 خطوات مع Paymob ──────────────────────

            // خطوة 1: Auth
            var authToken = await _paymob.GetAuthTokenAsync();

            // خطوة 2: Order
            var orderId = await _paymob.RegisterOrderAsync(
                authToken, amountInCents, payment.Id.ToString());

            // اتذكر الـ orderId
            payment.PaymobOrderId = orderId;
            await _context.SaveChangesAsync();

            // خطوة 3: Payment Key
            var integrationId = dto.PaymentMethod == "card"
                ? _settings.CardIntegrationId
                : _settings.WalletIntegrationId;

            var paymentToken = await _paymob.GetPaymentKeyAsync(
                authToken, orderId, amountInCents,
                integrationId,
                ticket.Tenant.Email!,
                ticket.Tenant.FullName,
                dto.WalletNumber
            );

            // لو كارت → رجّع iframe url
            if (dto.PaymentMethod == "card")
            {
                return new PaymentResponseDto
                {
                    PaymentId = payment.Id,
                    IframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentToken}"
                };
            }

            // لو wallet → رجّع redirect url
            var redirectUrl = await _paymob.RequestWalletPaymentAsync(
                paymentToken, dto.WalletNumber!);

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                RedirectUrl = redirectUrl
            };
        }

        private object GetPayments()
        {
            return _context.Payments;
        }

        // ── Webhook: Paymob بيبعته لما الدفع يتم ──────────
        public async Task HandleWebhookAsync(string payload, string hmacHeader)
        {
            // ── تأكد إن الـ webhook ده من Paymob فعلاً ──────
            if (!VerifyHmac(payload, hmacHeader))
                throw new UnauthorizedAccessException("Invalid HMAC");

            var data = JsonSerializer.Deserialize<JsonElement>(payload);
            var obj = data.GetProperty("obj");
            var success = obj.GetProperty("success").GetBoolean();
            var orderId = obj.GetProperty("order").GetProperty("merchant_order_id").GetString();
            var transId = obj.GetProperty("id").GetInt32().ToString();

            if (!success) return;  // الدفع فشل، مش هنعمل حاجة

            // جيب الـ Payment من الـ DB
            if (!Guid.TryParse(orderId, out var paymentId)) return;

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.IsPaid) return;

            // اتذكر إن الدفع اتم
            payment.IsPaid = true;
            payment.TransactionId = transId;
            payment.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ── تحقق من HMAC عشان نتأكد إن الـ webhook من Paymob ─
        private bool VerifyHmac(string payload, string hmacHeader)
        {
            // HMAC Secret بتاخده من Paymob Dashboard → Developers → Webhooks
            var secret = _settings.HmacSecret;
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var msgBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(msgBytes);
            var computed = Convert.ToHexString(hash).ToLower();

            return computed == hmacHeader.ToLower();
        }
    }
}

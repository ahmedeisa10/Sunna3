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

        //Tenant start Payment
        public async Task<PaymentResponseDto> InitiatePaymentAsync(
            InitiatePaymentDto dto, string tenantId)
        {
            
            var ticket = await _context.Tickets
                .Include(t => t.Tenant)
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId)
                ?? throw new NotFoundException("Ticket not found");

            if (ticket.Status != RequestStatus.Resolved)
                throw new BadRequestException("لازم الـ vendor يخلص شغله الأول");


            if (ticket.TenantId != tenantId)
                throw new ForbiddenException("Access denied");

            // calcualte money
            decimal total =ticket.Price;           // لازم يبقى عندك Price في الـ Ticket
            decimal platformAmount = total * 0.10m;          // 10% للمنصة
            decimal vendorAmount = total * 0.90m;          // 90% للـ vendor
            int amountInCents = (int)(total * 100);     // Paymob بيشتغل بالقروش

            // set payment in DB
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

            // Paymob in 3 steps

            // 1- Auth
            var authToken = await _paymob.GetAuthTokenAsync();

            //  2- Order
            var orderId = await _paymob.RegisterOrderAsync(
                authToken, amountInCents, payment.Id.ToString());

            // orderId
            payment.PaymobOrderId = orderId;
            await _context.SaveChangesAsync();

            //  3- Payment Key
            var integrationId = dto.PaymentMethod == "card"
                ? _settings.CardIntegrationId
                : _settings.WalletIntegrationId;

            var paymentToken = await _paymob.GetPaymentKeyAsync(
                authToken, orderId, amountInCents,
                integrationId,
                ticket.Tenant.Email!,
                ticket.Tenant.FullName,
                dto.WalletNumber,
                "https://sunna3.vercel.app/payment/callback"
            );

            // If card => return IframeURL
            if (dto.PaymentMethod == "card")
            {
                return new PaymentResponseDto
                {
                    PaymentId = payment.Id,
                    IframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentToken}"
                };
            }

            //  If wallet => return redirect Url
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

        // Webhook: Paymob sends it when payment is made
        public async Task HandleWebhookAsync(string payload, string hmacHeader)
        {
            // Make sure this webhook is actually from Paymob
            if (!VerifyHmac(payload, hmacHeader))
                throw new UnauthorizedAccessException("Invalid HMAC");

            var data = JsonSerializer.Deserialize<JsonElement>(payload);
            var obj = data.GetProperty("obj");
            var success = obj.GetProperty("success").GetBoolean();
            var orderId = obj.GetProperty("order").GetProperty("merchant_order_id").GetString();
            var transId = obj.GetProperty("id").GetInt32().ToString();

            if (!success) return;  // الدفع فشل، مش هنعمل حاجة

            // Get Payment from DB
            if (!Guid.TryParse(orderId, out var paymentId)) return;

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.IsPaid) return;

            
            payment.IsPaid = true;
            payment.TransactionId = transId;
            payment.PaidAt = DateTime.UtcNow;
            var ticket = await _context.Tickets.FindAsync(payment.TicketId);
            if (ticket != null)
            {
                ticket.IsPaid = true;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<PaymentVerifyResultDto> VerifyAndSyncPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return new PaymentVerifyResultDto { IsPaid = false };

            if (payment.IsPaid)
            {
                // sync the ticket if you haven't updated yet
                if (payment.Ticket != null && !payment.Ticket.IsPaid)
                {
                    payment.Ticket.IsPaid = true;
                    await _context.SaveChangesAsync();
                }
                return new PaymentVerifyResultDto
                {
                    IsPaid = true,
                    TicketId = payment.TicketId
                };
            }

            return new PaymentVerifyResultDto { IsPaid = false };
        }
        //Check the HMAC to make sure the webhook is from Paymob
        private bool VerifyHmac(string payload, string hmacHeader)
        {
            var secret = _settings.HmacSecret;
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var msgBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(msgBytes);
            var computed = Convert.ToHexString(hash).ToLower();

            return computed == hmacHeader.ToLower();
        }
        public async Task<PaymentVerifyResultDto> ConfirmFromCallbackAsync(CallbackConfirmDto dto)
        {
            if (!Guid.TryParse(dto.PaymentId, out var paymentId))
                return new PaymentVerifyResultDto { IsPaid = false };

            var payment = await _context.Payments
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return new PaymentVerifyResultDto { IsPaid = false };

            // If already paid — return the data
            if (payment.IsPaid)
                return new PaymentVerifyResultDto
                {
                    IsPaid = true,
                    TicketId = payment.TicketId
                };

            // Paymob said success => update the database
            if (dto.Success)
            {
                payment.IsPaid = true;
                payment.TransactionId = dto.TransactionId;
                payment.PaidAt = DateTime.UtcNow;

                if (payment.Ticket != null)
                    payment.Ticket.IsPaid = true;

                await _context.SaveChangesAsync();

                return new PaymentVerifyResultDto
                {
                    IsPaid = true,
                    TicketId = payment.TicketId
                };
            }

            return new PaymentVerifyResultDto { IsPaid = false };
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public string TenantId { get; set; }
        public AppUser Tenant { get; set; }

        public string VendorId { get; set; }
        public AppUser Vendor { get; set; }

        public decimal TotalAmount { get; set; }   // المبلغ الكلي
        public decimal PlatformAmount { get; set; }   // 10% للمنصة
        public decimal VendorAmount { get; set; }   // 90% للـ vendor

        public string PaymentMethod { get; set; }   // "card" or "wallet"
        public string? WalletNumber { get; set; }   // لو كاش

        public string? PaymobOrderId { get; set; }   // الـ order id من Paymob
        public string? TransactionId { get; set; }   // الـ transaction id بعد الدفع

        public bool IsPaid { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}

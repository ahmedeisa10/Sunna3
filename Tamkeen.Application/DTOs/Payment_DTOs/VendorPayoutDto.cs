using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Application.DTOs.Payment_DTOs
{
    // Manager View Payouts
    public class VendorPayoutDto
    {
        public Guid PaymentId { get; set; }
        public Guid TicketId { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string? VendorIBAN { get; set; }
        public string? VendorInstapay { get; set; }
        public string? VendorWallet { get; set; }
        public decimal VendorAmount { get; set; }   // 90%
        public decimal PlatformAmount { get; set; }   // 10%
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaidAt { get; set; }
        public bool IsDisbursed { get; set; }
        public DateTime? DisbursedAt { get; set; }
    }
}

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

        public decimal TotalAmount { get; set; }  
        public decimal PlatformAmount { get; set; }   
        public decimal VendorAmount { get; set; }   

        public string PaymentMethod { get; set; }   
        public string? WalletNumber { get; set; }  

        public string? PaymobOrderId { get; set; }   //  order id from Paymob
        public string? TransactionId { get; set; }   //  transaction id after payment 

        public bool IsPaid { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        //For Vendors
        public bool IsDisbursed { get; set; } = false;
        public DateTime? DisbursedAt { get; set; }
    }
}

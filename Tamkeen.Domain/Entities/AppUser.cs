using Microsoft.AspNetCore.Identity;
namespace Tamkeen.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? ImageUrl { get; set; } 

        public string? EmailConfirmationCode { get; set; }
        public DateTime? CodeExpiry { get; set; }

        // The feedback written by Tenant
        public ICollection<Feedback> TenantFeedbacks { get; set; }

        // The feedback written for Vendor
        public ICollection<Feedback> VendorFeedbacks { get; set; }
        public ICollection<TicketApplication> Applications { get; set; }


        //Money Info
        public string? IBAN { get; set; }
        public string? BankName { get; set; }
        public string? InstapayNumber { get; set; }
        public string? WalletNumber { get; set; }
    }
}

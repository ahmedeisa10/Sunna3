using Microsoft.AspNetCore.Http;

namespace Tamkeen.Application.DTOs
{
// The vendor fills out the form
    public class VendorRegisterDto
    {
        public string Token { get; set; }   // comes from the URL
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string PhoneNumber { get; set; }   
        public IFormFile Image { get; set; }


        //Money Info
        public string? IBAN { get; set; }
        public string? BankName { get; set; }
        public string? InstapayNumber { get; set; }
        public string? WalletNumber { get; set; }

    }
}

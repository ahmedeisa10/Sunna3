using Microsoft.AspNetCore.Http;

namespace Tamkeen.Application.DTOs.VendorInvitation_DTOs
{
    public class CreateVendorProfileDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Specialization { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public IFormFile IdCardFront { get; set; } = null!;
        public IFormFile IdCardBack { get; set; } = null!;
    }
}

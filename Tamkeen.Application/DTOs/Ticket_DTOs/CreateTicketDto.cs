using Microsoft.AspNetCore.Http;
using Tamkeen.Domain.Enums;

namespace Tamkeen.Application.DTOs.Ticket_DTOs
{
    public class CreateTicketDto
    {
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public string problemType { get; set; }
        public string TenantLocation { get; set; }
        public string Governorate { get; set; }
        public string City { get; set; }
        public DateTime Arrival { get; set; }
        public DateTime Deadline { get; set; }
        public Guid? CompanyId { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}

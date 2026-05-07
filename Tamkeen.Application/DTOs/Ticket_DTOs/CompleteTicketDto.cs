// Tamkeen.Application.DTOs.Ticket_DTOs/CompleteTicketDto.cs
using Microsoft.AspNetCore.Http;

namespace Tamkeen.Application.DTOs.Ticket_DTOs
{
    public class CompleteTicketDto
    {
        public decimal Price { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
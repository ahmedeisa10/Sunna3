
using Tamkeen.Domain.Enums;
namespace Tamkeen.Domain.Entities
{
    public class Image
    {
        public Guid Id { get; set; }

        public string Url { get; set; }

        public ImageType Type { get; set; }

        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}

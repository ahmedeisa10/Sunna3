using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Application.DTOs.Payment_DTOs
{
    // Frontend send this

    public class InitiatePaymentDto
    {
        public Guid TicketId { get; set; }
        public string PaymentMethod { get; set; }  // "card" or "wallet"
        public string? WalletNumber { get; set; }  // if wallet >> must send the number
    }
}

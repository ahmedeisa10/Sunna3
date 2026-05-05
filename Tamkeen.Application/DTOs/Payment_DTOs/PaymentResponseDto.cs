using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Application.DTOs.Payment_DTOs
{
    //Backend refers it back to frontend
    public class PaymentResponseDto
    {
        public string IframeUrl { get; set; }  // if card
        public string? RedirectUrl { get; set; }  // if wallet
        public Guid PaymentId { get; set; }
    }
}

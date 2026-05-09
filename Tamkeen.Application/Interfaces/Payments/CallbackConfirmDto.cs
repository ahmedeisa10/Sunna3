using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Application.Interfaces.Payments
{
    // DTO جديد
    public class CallbackConfirmDto
    {
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public bool Success { get; set; }
    }
}

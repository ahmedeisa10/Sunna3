using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Infrastructure.Setting
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public string CardIntegrationId { get; set; }
        public string WalletIntegrationId { get; set; }
        public string IframeId { get; set; }
        public string HMCA { get; set; }
        public string BaseUrl { get; set; }
    }
}

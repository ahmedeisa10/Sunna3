using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tamkeen.Application.DTOs.AI
{
    public class ScheduleItem
    {
        public string Device { get; set; }
        public string Task { get; set; }
        public string Frequency { get; set; }
        public string NextDue { get; set; }
        public string Priority { get; set; } // High / Medium / Low
    }
}

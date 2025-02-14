using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class ScheduledTaskBlacklist
    {
        public static readonly List<string> TaskBlacklist = new List<string>
        {
            @"\ServiceData4",
            @"\Service\Data"
        };
    }
}

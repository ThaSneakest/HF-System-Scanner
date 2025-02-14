using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class CommandProcessorFix
    {
        public static void CommandProcFix(string fix)
        {
            if (fix.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase))
            {
                RegistryValueHandler.DeleteRegistryValue(@"HKLM\Software}\Microsoft\Command Processor", "AutoRun");
            }

            if (!fix.StartsWith("HKU\\", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+", "$1");
            RegistryUtils.RELOAD(user);

            string key = $"HKU\\{user}\\Software\\Microsoft\\Command Processor";
            RegistryValueHandler.DeleteRegistryValue(key, "AutoRun");

            RegistryUtils.REUNLOAD(user);
        }
    }
}

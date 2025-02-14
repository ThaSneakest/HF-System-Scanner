using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class FirewallFix
    {
        public static void DeleteFirewallRule(string fixString)
        {
            string registryKey;
            string valueName = Regex.Replace(fixString, @".+?: \[(.+?)\] =>.*", "$1");

            if (fixString.Contains("FirewallRules:"))
            {
                registryKey = $@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules";
            }
            else
            {
                string subKey = fixString.Contains("AuthorizedApplications:") ? @"\AuthorizedApplications" : @"\GloballyOpenPorts";
                string domain = Regex.Replace(fixString, @"(.+?)\..+", "$1");
                registryKey = $@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\\{domain}{subKey}\List";
            }

            RegistryValueHandler.DeleteRegistryValue(registryKey, valueName);
        }

    }
}

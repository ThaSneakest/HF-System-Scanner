using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class NameServerHandler
    {
        public static void DeleteNameServer(string fix)
        {
            string SYSTEM = "YourSystemValue"; // Replace with the actual value
            string BOOTSYSTEM = "YourBootSystemValue"; // Replace with the actual value
            string DEF = "YourDefValue"; // Replace with the actual value


            // Define key and value
            string key = @"HKLM\" + SYSTEM + BOOTSYSTEM + DEF + @"\Services\Tcpip\Parameters";

            // Extract the value using regular expression
            string val = Regex.Replace(fix, @".+?\[(.+?)\].*", "$1");

            // Call method to delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }
    }
}

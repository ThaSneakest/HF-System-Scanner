using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class PrinterMonitorFix
    {
        public static void PrintMonFix(string fix)
        {
            try
            {
                // Extract the key using Regex
                string key = Regex.Replace(fix, @"HKLM\\\.\.\.\\Print\\Monitors\\([^:]+):.*", "$1");

                // Build the full registry key path
                string registryKeyPath = @"HKLM\" + @"SYSTEM" + @"\Control\Print\Monitors\" + key;

                // Delete the registry key
                RegistryKeyHandler.DeleteRegistryKey(registryKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PrintMonFix: {ex.Message}");
            }
        }
    }
}

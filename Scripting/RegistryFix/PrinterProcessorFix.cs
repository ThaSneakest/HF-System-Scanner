using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class PrinterProcessorFix
    {
        public static void PrintProcFix(string fix)
        {
            try
            {
                // Extracting the keys using regex
                string key1 = Regex.Replace(fix, @"HKLM\\\.\.\.\\([^\\]+)\..+", "$1");
                string key2 = Regex.Replace(fix, @"HKLM\\\.\.\.\\[^\\]+\\Print Processors\\([^:]+):.*", "$1");

                string fullKey = @"HKLM\" + @"SYSTEM" + @"\Control\Print\Environments\" + key1 + @"\Print Processors\" + key2;

                // Check if the registry key exists and delete it
                RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullKey, writable: true);
                if (registryKey != null)
                {
                    registryKey.DeleteSubKeyTree(key2);  // Delete the key and its subkeys
                    Console.WriteLine($"Registry key {fullKey} deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"Registry key {fullKey} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PrintProcFix: {ex.Message}");
            }
        }
    }
}

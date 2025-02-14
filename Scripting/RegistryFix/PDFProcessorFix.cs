using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class PDFProcessorFix
    {
        public static void PDFFixFunction(string fix)
        {
            try
            {
                string clsid = Regex.Replace(fix, @"[^\{]+(\{.+\}).*", "$1");
                string key = $@"HKLM\SOFTWARE\Microsoft\Code Store Database\Distribution Units\{clsid}";

                // Deleting the registry key if it exists
                RegistryKeyHandler.DeleteRegistryKey(key);

                // Checking the CLSID in Software Classes
                key = $@"HKLM\Software\Classes\CLSID\{clsid}";
                if (RegistryKeyHandler.RegistryKeyExists(key))
                {
                    RegistryKeyHandler.DeleteRegistryKey(key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PDFFix: {ex.Message}");
            }
        }
    }
}

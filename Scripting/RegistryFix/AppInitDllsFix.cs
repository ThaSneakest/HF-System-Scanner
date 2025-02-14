using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting
{
    public class AppInitDllsFix
    {
        public static void HandleAppInitFix(string fix)
        {
            // Extract the regex from the FIX string
            string regex = Regex.Replace(fix, @"AppInit_DLLs: (.+) =>.*", "$1");

            // Open the registry key
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows", writable: true))
            {
                if (key == null)
                {
                    // Handle the case where the registry key is not found
                    return;
                }

                // Read the existing AppInit_DLLs value from the registry
                string registryValue = RegistryValueHandler.TryReadRegistryValue(key, "AppInit_DLLs");

                string regex1 = regex.Replace("\\", "\\\\");

                // If the registry value doesn't match the regex, write a log entry
                if (!Regex.IsMatch(registryValue, @"(?i)" + regex1))
                {
                    //Logger.WriteToLog($"\"{regex}\" => {Data0} {NotFound}.");
                    return;
                }

                // Remove the regex from the registry value
                registryValue = Regex.Replace(registryValue, @"(?i)" + regex1, "");
                registryValue = Regex.Replace(registryValue, @"\s{2,}", " ");

                // Write the updated value to the registry
                //RegistryValueHandler.WriteRegistryValue(key, "AppInit_DLLs", registryValue);

                // If no error, write success to the log
                if (registryValue != null)
                {
                    //Logger.WriteToLog($"\"{regex}\" => {Data0} {Deleted}.");
                    string updatedRegistryValue = RegistryValueHandler.TryReadRegistryValue(key, "AppInit_DLLs");
                    if (string.IsNullOrWhiteSpace(updatedRegistryValue))
                    {
                        // RegistryValueHandler.WriteRegistryValue(key, "AppInit_DLLs", "");
                    }
                }
                else
                {
                    //Logger.WriteToLog($"\"{regex}\" => {Data0} {NotDeleted}.");
                }
            }
        }
    }
}

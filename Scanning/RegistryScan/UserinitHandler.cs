using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class UserinitHandler
    {
        

        public static void USERINITMPR(string keyPath)
        {
            try
            {
                // Open the registry key
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    // Read the value
                    string valData = RegistryValueHandler.TryReadRegistryValue(key, "UserInitMprLogonScript");
                    if (!string.IsNullOrEmpty(valData))
                    {
                        string file = valData;
                        string company = ""; // Set company based on your logic
                        string cDate = ""; // Set the creation date if needed
                        string atten = "";

                        // Check if the file exists
                        if (File.Exists(file))
                        {
                            // Add the creation date (example, set to an empty string here)
                            cDate = " [" + cDate + "]";
                            valData = file;
                        }

                        // Check for specific process
                        if (valData.IndexOf("wmiprvse.exe", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            atten = " <==== Updated";
                        }

                        // Add to the array or process data
                        string logData = $"{keyPath}\\UserInitMprLogonScript: -> {valData}{cDate}{company}{atten}";
                        string[] myArray = Array.Empty<string>();
                        myArray = ArrayUtils.AddToArray1Arg(myArray, logData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in USERINITMPR for key {keyPath}: {ex.Message}");
            }
        }

        
        public static void UUSERINIT(string fix)
        {
            string user;
            string key;

            // Extract user from the FIX string using regular expressions
            user = System.Text.RegularExpressions.Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+Winlogon:.*", "$1");

            // Simulating the RELOAD function
            RegistryUtils.RELOAD(user);

            key = @"HKU\" + user + @"\Software\Microsoft\Windows NT\CurrentVersion\Winlogon";

            // Delete the Userinit registry key value
            RegistryValueHandler.DeleteRegistryValue(key, "Userinit");

            // Simulating the REUNLOAD function
            RegistryUtils.REUNLOAD(user);
        }

        public string GetUserinit()
        {
            string Userinit = string.Empty;

            try
            {
                // Open the registry key
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    if (CSK != null)
                    {
                        // Retrieve the value for "Userinit"
                        object A = CSK.GetValue("Userinit");

                        // If value is not null, append to Userinit string
                        if (A != null)
                        {
                            Userinit += "Userinit: " + A.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine(ex.Message);
            }

            return Userinit;
        }
    }
}
        

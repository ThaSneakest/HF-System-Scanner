using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class PoliciesFix
    {
        public void DELUPOLICIES(string FIX, string HFIXLOG)
        {
            string VAL = Regex.Replace(FIX, @"(?i).+\\Software\\Policies\\...\\system: \[(.*)\] .*", "$1");
            string KEY = @"HKLM\Software\Policies\Microsoft\Windows\System";

            string USER = string.Empty;

            if (FIX.Contains("HKU\\"))
            {
                USER = Regex.Replace(FIX, @"(?i)HKU\\(.+?)\\Software\\Policies\\...\\system:.*", "$1");
                RegistryUtils.RELOAD(USER);
                KEY = @"HKU\" + USER + @"\Software\Policies\Microsoft\Windows\System";
            }

            RegistryValueHandler.DeleteRegistryValue(KEY, VAL);

            if (FIX.Contains("HKU\\"))
            {
                RegistryUtils.REUNLOAD(USER);
            }
        }
        public static void DELETEUPOLICIES(string fix)
        {
            // Extract the value from the fix string using regex
            string val = Regex.Replace(fix, @"(?i)HKU\\.+\\Policies\\system: \[(.*)\] .*", "$1");

            // Extract the user from the fix string using regex
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+system:.*", "$1");


            // Reload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.RELOAD(user);

            // Build the registry key
            string key = @"HKU\" + user + @"\Software\Microsoft\Windows\CurrentVersion\Policies\system";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Unload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.REUNLOAD(user);
        }
        public static void DELETEUPOLICIESRUN(string fix)
        {
            // Extract the value from the fix string using regex
            string val = Regex.Replace(fix, @"(?i)HKU\\.+\\Policies\\Explorer\\Run: \[(.*)\] =>.*", "$1");


            // Extract the user from the fix string using regex
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.*", "$1");


            // Reload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.RELOAD(user);

            // Build the registry key
            string key = @"HKU\" + user + @"\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Unload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.REUNLOAD(user);
        }
        public static void DeleteEPolicies(string fix)
        {
            // Extract the value and user from the fix string using regular expressions
            string val = Regex.Replace(fix, @"(?i)HKU\\.+\\Explorer: \[([^\]]*)\].*", "$1");
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+", "$1");

            // Call the reload function
            RegistryUtils.RELOAD(user);

            // Define the registry key
            string key = $"HKU\\{user}\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Call the unload function
            RegistryUtils.REUNLOAD(user);
        }
        public static void DeleteEPoliciesLM(string fix, string software)
        {
            // Extract the value from the fix string using regular expression
            string val = Regex.Replace(fix, @"(?i)HKLM\\\.\.\.\\Policies\\Explorer: \[([^\]]*)\].*", "$1");

            // Define the registry key
            string key = $"HKLM\\{software}\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void PoliciesExp(string fix)
        {
            try
            {
                // Extract the value name using regex
                string val = Regex.Replace(fix, @"(?i)HKLM\\\.\.\.\\Policies\\Explorer\\Run: \[(.*)\] =>.*", "$1");

                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine("Error: Unable to extract the value name from the input string.");
                    return;
                }

                // Define the registry key path
                string key = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run";

                Console.WriteLine($"Attempting to delete value '{val}' from key: {key}");

                // Delete the value from the registry
                bool success = RegistryValueHandler.DeleteRegistryValueBool(key, val);

                // Provide feedback on success or failure
                if (success)
                {
                    Console.WriteLine($"Successfully deleted value '{val}' from '{key}'.");
                }
                else
                {
                    Console.WriteLine($"Failed to delete value '{val}'. Value might not exist in '{key}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PoliciesExp: {ex.Message}");
            }
        }

    }
}

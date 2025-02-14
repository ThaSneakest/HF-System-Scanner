using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class UserinitFix
    {
        public static void USERINIT(string fix)
        {
            try
            {
                // Open the registry key for SystemRoot
                using (RegistryKey systemRootKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (systemRootKey == null)
                    {
                        // Log an error if the registry key is not found
                        FileFix.FileWrite("logfile.txt", fix + " => Restore failed: SystemRoot key not found" + Environment.NewLine);
                        return;
                    }

                    // Read the SystemRoot value
                    string systemRoot = RegistryValueHandler.TryReadRegistryValue(systemRootKey, "SystemRoot");
                    if (string.IsNullOrEmpty(systemRoot))
                    {
                        // Log an error if SystemRoot value is not found
                        FileFix.FileWrite("logfile.txt", fix + " => Restore failed: SystemRoot value not found" + Environment.NewLine);
                        return;
                    }

                    // Construct the value for Userinit
                    string userInitValue = systemRoot + @"\system32\userinit.exe,";

                    // Open the registry key for Winlogon
                    using (RegistryKey winlogonKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", writable: true))
                    {
                        if (winlogonKey == null)
                        {
                            // Log an error if the Winlogon key is not found
                            FileFix.FileWrite("logfile.txt", fix + " => Restore failed: Winlogon key not found" + Environment.NewLine);
                            return;
                        }

                        try
                        {
                            // Restore the Userinit value in the registry
                            winlogonKey.SetValue("Userinit", userInitValue, RegistryValueKind.String);

                            // Log success
                            FileFix.FileWrite("logfile.txt", fix + " => Restore succeeded" + Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            // Log an error if something goes wrong
                            FileFix.FileWrite("logfile.txt", fix + $" => Restore failed: {ex.Message}" + Environment.NewLine);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                FileFix.FileWrite("logfile.txt", fix + " => Restore failed: " + ex.Message + Environment.NewLine);
            }
        }

        public static void USERINITMPRFIX(string fix)
        {
            string key;
            string user;

            if (fix.Contains("HKU\\"))
            {
                // Extract user from the FIX string
                user = System.Text.RegularExpressions.Regex.Replace(fix, @"HKU\\([^\\]+)\..+", "$1");
                key = @"HKU\" + user + @"\Environment";
            }
            else
            {
                key = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
            }

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, "UserInitMprLogonScript");
        }

    }
}

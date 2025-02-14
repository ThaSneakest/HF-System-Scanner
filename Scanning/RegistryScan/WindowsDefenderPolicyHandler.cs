using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

//Tested and Working
//Potentially Delete

namespace Wildlands_System_Scanner
{
    public class WindowsDefenderPolicyHandler
    {
        public static void ScanWindowsDefenderPolicies()
        {
            // Define the list of registry policy paths to scan
            var policies = new[]
            {
                @"Software\Policies\Microsoft\Windows Defender",
                @"Software\Policies\Microsoft\Windows\WindowsUpdate",
                @"Software\Policies\Microsoft\MRT",
                @"Software\Policies\Microsoft\Windows Defender Security Center",
                @"Software\Policies\Microsoft\WindowsFirewall"
            };

            // Iterate through each policy and scan it
            foreach (var policy in policies)
            {
                Console.WriteLine($"Scanning registry key: HKLM\\{policy}");
                try
                {
                    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(policy))
                    {
                        if (key == null)
                        {
                            continue;
                        }

                        // Retrieve all values under the key
                        foreach (var valueName in key.GetValueNames())
                        {
                            object valueData = key.GetValue(valueName);
                            if (valueData != null)
                            {
                                Logger.Instance.LogPrimary($"HKLM\\{policy}: [{valueName}] => {valueData}");
                            }
                            else
                            {
                                Logger.Instance.LogPrimary($"HKLM\\{policy}: [{valueName}] => No value set");
                            }
                        }

                        // Retrieve all subkeys for further inspection if needed
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            Logger.Instance.LogPrimary($"HKLM\\{policy}: Subkey => {subKeyName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scanning registry key HKLM\\{policy}: {ex.Message}");
                }
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class GroupPolicyScan
    {
        // This method retrieves and logs Group Policy-based startup scripts
        public string GetGroupPolicyStartupScripts()
        {
            StringBuilder startupScripts = new StringBuilder();

            // Group Policy startup registry paths for both computer and user configurations
            string[] registryPaths = new string[]
            {
                // Computer Configuration Startup scripts (HKLM)
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup", // Computer configuration

                // User Configuration Startup scripts (HKCU)
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup" // User configuration
            };

            // Iterate through each registry path to find Group Policy-based startup scripts
            foreach (string registryPath in registryPaths)
            {
                try
                {
                    // Open the registry key for both HKCU and HKLM (if applicable)
                    using (RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath) ?? Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath))
                    {
                        if (startupKey != null)
                        {
                            // Enumerate the values in the registry key (startup scripts)
                            foreach (string subKeyName in startupKey.GetValueNames())
                            {
                                try
                                {
                                    // Get the path of the script associated with the startup entry
                                    string scriptPath = (string)startupKey.GetValue(subKeyName, null);

                                    // If the script path exists, log the script name and its path in the desired format
                                    if (!string.IsNullOrEmpty(scriptPath))
                                    {
                                        startupScripts.Append("Script:\t" + subKeyName + " ");
                                        if (!string.IsNullOrEmpty(scriptPath))
                                            startupScripts.Append(scriptPath + " - ");
                                        startupScripts.Append(Environment.NewLine);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    startupScripts.AppendLine($"Error accessing script '{subKeyName}': {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    startupScripts.AppendLine($"Error accessing the registry path '{registryPath}': {ex.Message}");
                }
            }

            return startupScripts.ToString();
        }
    }
}

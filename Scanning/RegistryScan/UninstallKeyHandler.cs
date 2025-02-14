using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class UninstallKeyHandler
    {
      

        public static void InstalledProgramsScan()
        {
            StringBuilder installedPrograms = new StringBuilder();

            try
            {
                // Define registry keys to check
                string[] registryPaths = new string[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", // 64-bit programs
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", // 32-bit programs on 64-bit systems
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" // Current user's installed apps
                };

                foreach (var path in registryPaths)
                {
                    // Open registry key for each path
                    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            foreach (string subKeyName in key.GetSubKeyNames())
                            {
                                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                                {
                                    // Get display name and version information
                                    string displayName = subKey?.GetValue("DisplayName")?.ToString();
                                    string displayVersion = subKey?.GetValue("DisplayVersion")?.ToString();

                                    if (!string.IsNullOrEmpty(displayName))
                                    {
                                        installedPrograms.AppendLine($"Program: {displayName}");
                                    }
                                }
                            }
                        }
                    }
                }

                // Write the results to file
                Logger.Instance.LogPrimary(installedPrograms.ToString());
            }
            catch (Exception ex)
            {
                Logger.Instance.LogPrimary($"Error: {ex.Message}");
            }
        }
    }
}

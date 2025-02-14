using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

//Tested and Working

namespace Wildlands_System_Scanner
{
    public class SEHHandler
    {
        public static void SEH()
        {
            try
            {
                Console.WriteLine("Starting SEH method...");

                // Define the target registry key
                string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks";
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Registry key not found: HKEY_LOCAL_MACHINE\\{registryKeyPath}");
                        return;
                    }

                    string[] valueNames = registryKey.GetValueNames();
                    if (valueNames.Length == 0)
                    {
                        Console.WriteLine("No values found in ShellExecuteHooks.");
                        return;
                    }

                    foreach (string valueName in valueNames)
                    {
                        Console.WriteLine($"Processing value: {valueName}");
                        string clsid = registryKey.GetValue(valueName)?.ToString();

                        if (string.IsNullOrEmpty(clsid))
                        {
                            Console.WriteLine($"Value {valueName} is empty or null.");
                            continue;
                        }

                        if (Regex.IsMatch(clsid, @"\{.+\}"))
                        {
                            Console.WriteLine($"Found CLSID: {clsid}");

                            // Read CLSID details
                            string valN = RegistryUtils.RegRead($@"HKCR\CLSID\{clsid}", "")?.ToString();
                            string filePath = RegistryUtils.RegRead($@"HKCR\CLSID\{clsid}\InprocServer32", "")?.ToString();

                            if (!string.IsNullOrEmpty(filePath) && filePath.IndexOf("mscoree.dll", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                MSCOREEHandler.MSCOREE($@"HKCR\CLSID\{clsid}\InprocServer32", ref filePath);
                            }

                            string file = filePath;

                            string size = string.Empty;
                            string company = string.Empty;

                            if (!string.IsNullOrEmpty(file) && File.Exists(file))
                            {
                                var fileInfo = new FileInfo(file);
                                size = $"{fileInfo.Length} bytes";
                                filePath = fileInfo.FullName;
                                company = FileUtils.GetFileCompany(file);
                            }
                            else
                            {
                                filePath += " -> File Not Found";
                                company = "Unknown";
                            }

                            if (string.IsNullOrEmpty(valN))
                            {
                                valN = "Default";
                            }

                            string attention = string.Empty;
                            if (Regex.IsMatch(filePath, @"(?i)\\AppData\\(Roaming|Local)\\.+?\\[^\\]+?\.dll") ||
                                Regex.IsMatch(filePath, @"(?i)\\(ProgramData|Windows)\\[^\\]+?\.(dll|exe|dat|bat|vbs)") ||
                                Regex.IsMatch(filePath, @"(?i)mcicda64.dll"))
                            {
                                attention = " <==== Updated";
                            }

                            Logger.Instance.LogPrimary($"HKCR\\{valN} - {clsid} - {filePath} [{size}] {company} {attention}");
                        }
                        else
                        {
                            Console.WriteLine($"Invalid CLSID format: {clsid}");
                        }
                    }
                }

                Console.WriteLine("SEH method completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SEH method: {ex.Message}");
            }
        }



       

        public string GetShellExecuteHooks()
        {
            StringBuilder SEHs = new StringBuilder();

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetValueNames())
                        {
                            try
                            {
                                string A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null);
                                string B = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null);

                                if (!string.IsNullOrEmpty(B))
                                {
                                    SEHs.Append("HKLM\\SEH:\t" + S + " ");
                                    if (!string.IsNullOrEmpty(A)) SEHs.Append(A + " - ");
                                    SEHs.Append(B + Environment.NewLine);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Handle exception if necessary
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine(ex.Message);
            }

            return SEHs.ToString();
        }

    }
}

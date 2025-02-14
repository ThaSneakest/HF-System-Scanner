using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

//Needs work

namespace Wildlands_System_Scanner
{
    public class SSOHandler
    {
        public static void SSO()
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad";

            try
            {
                // Open the registry key
                using (RegistryKey hkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: false))
                {
                    if (hkey == null)
                    {
                        Console.WriteLine($"Registry key not found: HKLM\\{keyPath}");
                        return;
                    }

                    foreach (string subkey in hkey.GetValueNames())
                    {
                        try
                        {
                            ProcessSSOSubKey(subkey);
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error processing subkey '{subkey}': {innerEx.Message}");
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to read HKLM\\{keyPath}. Try running as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing registry key HKLM\\{keyPath}: {ex.Message}");
            }
        }

        private static void ProcessSSOSubKey(string subkey)
        {
            string clsidPath = $@"CLSID\{subkey}";
            string inprocServerPath = $@"CLSID\{subkey}\InprocServer32";

            // Attempt to read CLSID name
            string clsidName = RegistryValueHandler.TryReadRegistryValue(Microsoft.Win32.Registry.ClassesRoot, clsidPath, "");
            if (string.IsNullOrEmpty(clsidName))
            {
                Console.WriteLine($"SubKey not found: {clsidPath}");
                return;
            }

            // Attempt to read InprocServer32 path
            string inprocServer = RegistryValueHandler.TryReadRegistryValue(Microsoft.Win32.Registry.ClassesRoot, inprocServerPath, "");
            if (string.IsNullOrEmpty(inprocServer))
            {
                Console.WriteLine($"SubKey not found: {inprocServerPath}");
                return;
            }

            Console.WriteLine($"Processing CLSID: {subkey}");
            Console.WriteLine($"CLSID Name: {clsidName}");
            Console.WriteLine($"CLSID File Path: {inprocServer}");

            // Handle paths with mscoree.dll
            if (inprocServer.IndexOf("mscoree.dll", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MSCOREEHandler.MSCOREE($@"HKCR\CLSID\{subkey}\InprocServer32", ref inprocServer);
            }

            string filePath = inprocServer;
            string company = FileUtils.GetFileCompany(filePath); // Replace with actual implementation
            string creationDate = File.Exists(filePath) ? File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss") : "File Not Found";

            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine($"No valid file path found for CLSID: {subkey}");
            }

            Logger.Instance.LogPrimary($"ShellServiceObjects: {clsidName} -> {subkey} => {filePath} [{creationDate}] {company}");
        }

        public static void SSODL()
        {
            int index = 0;
            string valName;
            string clsid;
            string filePath = string.Empty;
            string company = string.Empty;

            try
            {
                string key = @"HKLM\Software\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad";
                using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: false))
                {
                    if (hKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {key}");
                        return;
                    }

                    while (true)
                    {
                        try
                        {
                            // Get registry value name by index
                            valName = hKey.GetValueNames().ElementAtOrDefault(index);
                            if (valName == null)
                                break; // Exit loop if no more values

                            index++;

                            // Read CLSID associated with the value name
                            clsid = RegistryValueHandler.ReadRegistryValue(key, valName);
                            if (string.IsNullOrEmpty(clsid))
                            {
                                clsid = "DefaultRegistryValue"; // Default CLSID
                                filePath = "DefaultFilePath"; // Default file path
                            }
                            else
                            {
                                // Process CLSID if it's a valid GUID
                                if (Regex.IsMatch(clsid, @"\{.+\}"))
                                {
                                    filePath = RegistryValueHandler.TryReadRegistryValue(
                                        Microsoft.Win32.Registry.LocalMachine,
                                        $@"Software\Classes\CLSID\{clsid}\InprocServer32",
                                        string.Empty
                                    );

                                    // Process paths containing "mscoree.dll"
                                    if (!string.IsNullOrEmpty(filePath) &&
                                        filePath.IndexOf("mscoree.dll", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        MSCOREEHandler.MSCOREE(
                                            $@"HKLM\Software\Classes\CLSID\{clsid}\InprocServer32",
                                            ref filePath
                                        );
                                    }

                                    if (!File.Exists(filePath))
                                    {
                                        filePath += " (File Not Found)";
                                    }
                                }
                            }

                            // Skip WebCheck with specific CLSID if file path is empty
                            if (valName == "WebCheck" &&
                                clsid == "{E6FB5E20-DE35-11CF-9C87-00AA005127ED}" &&
                                string.IsNullOrEmpty(filePath))
                            {
                                continue;
                            }

                            // Log the processed information
                            Logger.Instance.LogPrimary(
                                $"HKLM\\...\\{key}: [{valName}] -> CLSID: {clsid}, FilePath: {filePath}, Company: {company}"
                            );
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error processing value at index {index}: {innerEx.Message}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SSODL: {ex.Message}");
            }
        }

       

        public string GetShellServiceObjectDelayLoad()
        {
            StringBuilder SSODLs = new StringBuilder();

            try
            {
                using (RegistryKey CSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad"))
                {
                    if (CSK != null)
                    {
                        foreach (string S in CSK.GetValueNames())
                        {
                            try
                            {
                                string A = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad", S, null));
                                string B = Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + A + @"\InprocServer32", null, null));

                                if (!string.IsNullOrEmpty(A) && !string.IsNullOrEmpty(B))
                                {
                                    SSODLs.Append("HKLM\\SSODL:\t");
                                    SSODLs.Append(A + " " + S + " - " + B);
                                    SSODLs.Append(Environment.NewLine);
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

            return SSODLs.ToString();
        }
    }
}

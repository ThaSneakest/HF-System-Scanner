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
//Processing CLSID: {00021401-0000-0000-C000-000000000046}
//Exception thrown: 'System.OverflowException' in mscorlib.dll
//Exception thrown: 'System.TypeInitializationException' in Wildlands System Scanner.exe
//TypeInitializationException: The type initializer for 'Wildlands_System_Scanner.Utilities.NativeMethods' threw an exception.

namespace Wildlands_System_Scanner
{
    public class SHIMHandler
    {
        public static void SHIM()
        {
            string keyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags";

            try
            {
                Console.WriteLine($"Starting SHIM processing for registry key: HKLM\\{keyPath}");

                // Open the main registry key
                using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (hKey == null)
                    {
                        Console.WriteLine($"Registry key not found: HKLM\\{keyPath}");
                        return;
                    }

                    // Iterate through all subkeys under AppCompatFlags
                    foreach (string subKeyName in hKey.GetSubKeyNames())
                    {
                        Console.WriteLine($"Processing subkey: {subKeyName}");

                        // Validate if subKeyName is a CLSID
                        if (Regex.IsMatch(subKeyName, @"\{.+\}"))
                        {
                            Console.WriteLine($"Processing CLSID: {subKeyName}");

                            string clsidPath = $@"HKEY_CLASSES_ROOT\CLSID\{subKeyName}";
                            string inprocPath = $@"{clsidPath}\InprocServer32";

                            try
                            {
                                string valN = RegistryUtils.RegRead(clsidPath, "")?.ToString();
                                string filePath = RegistryUtils.RegRead(inprocPath, "")?.ToString();

                                if (string.IsNullOrEmpty(valN))
                                {
                                    Console.WriteLine($"CLSID Name: [Not Found]");
                                }
                                else
                                {
                                    Console.WriteLine($"CLSID Name: {valN}");
                                }

                                if (string.IsNullOrEmpty(filePath))
                                {
                                    Console.WriteLine($"CLSID File Path: [Not Found]");
                                }
                                else
                                {
                                    Console.WriteLine($"CLSID File Path: {filePath}");
                                }

                                if (!string.IsNullOrEmpty(filePath) &&
                                    filePath.IndexOf("mscoree.dll", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    Console.WriteLine("Detected 'mscoree.dll', processing with MSCOREEHandler...");
                                    MSCOREEHandler.MSCOREE(inprocPath, ref filePath);
                                }
                            }
                            catch (OverflowException)
                            {
                                Console.WriteLine("OverflowException encountered while processing CLSID.");
                            }
                            catch (TypeInitializationException ex)
                            {
                                Console.WriteLine($"TypeInitializationException: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing CLSID {subKeyName}: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipping non-CLSID subkey: {subKeyName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SHIM: {ex.Message}");
            }
        }

        public void SHIMU(string sKey)
        {
            string keyPath = @"HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\" + sKey;
            int i = 1;
            List<string> arrayReg = new List<string>();

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine("Registry key not found: " + keyPath);
                        return;
                    }

                    string subKey;
                    while ((subKey = RegistrySubKeyHandler.GetSubKeyByIndex(key, i)) != null)
                    {
                        string message = $"HKCU\\Software\\...\\AppCompatFlags\\{sKey}\\{subKey}: There is currently no automatic fix for this entry. Please report this entry to the developer of Farbar Recovery Scan Tool.";
                        arrayReg.Add(message);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Optionally, output the results or handle them as needed
            foreach (var entry in arrayReg)
            {
                Console.WriteLine(entry);
            }
        }
    }
}

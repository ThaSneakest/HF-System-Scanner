using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner
{
    public class MSCOREEHandler
    {
        public static bool MSCOREE(string key, ref string data)
        {
            try
            {
                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        // Read "CodeBase" from the registry key
                        object data1 = registryKey.GetValue("CodeBase");

                        if (data1 != null)
                        {
                            // Process the value if "CodeBase" exists
                            data = Regex.Replace(data1.ToString(), "(?i)file:/+", "");
                        }
                        else
                        {
                            // If "CodeBase" doesn't exist, enumerate subkeys
                            int k = 0;
                            while (true)
                            {
                                string subKey = registryKey.GetSubKeyNames().ElementAtOrDefault(k);

                                if (subKey == null)
                                {
                                    break; // Exit if no more subkeys are found
                                }

                                // Read "CodeBase" from the subkey
                                object subKeyData = registryKey.OpenSubKey(subKey)?.GetValue("CodeBase");

                                if (subKeyData != null)
                                {
                                    // Process the value if "CodeBase" exists in subkey
                                    data = Regex.Replace(subKeyData.ToString(), "(?i)file:/+", "");
                                    break; // Exit once we find the CodeBase
                                }
                                k++;
                            }
                        }

                        return true; // Successfully processed data
                    }
                    else
                    {
                        return false; // Registry key not found
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MSCOREE: {ex.Message}");
                return false; // Handle error and return false
            }
        }
    }
}

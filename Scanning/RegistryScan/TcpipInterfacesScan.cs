using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class TcpipInterfacesScan
    {
        // This method retrieves and logs the entries from the Tcpip\Interfaces registry key
        public string GetTcpipInterfaces()
        {
            StringBuilder interfacesInfo = new StringBuilder();

            // Registry path for Tcpip Interfaces
            string registryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

            try
            {
                // Open the registry key for Interfaces
                using (RegistryKey interfacesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (interfacesKey != null)
                    {
                        // Enumerate the subkeys (network interfaces) in the Tcpip Interfaces registry key
                        foreach (string subKeyName in interfacesKey.GetSubKeyNames())
                        {
                            try
                            {
                                // Open each interface subkey
                                using (RegistryKey interfaceKey = interfacesKey.OpenSubKey(subKeyName))
                                {
                                    if (interfaceKey != null)
                                    {
                                        // Get the values for the current interface (like IP addresses, subnet masks, etc.)
                                        string[] valueNames = interfaceKey.GetValueNames();
                                        interfacesInfo.AppendLine($"Interface: {subKeyName}");

                                        foreach (string valueName in valueNames)
                                        {
                                            try
                                            {
                                                string value = interfaceKey.GetValue(valueName, "Not Found").ToString();
                                                interfacesInfo.AppendLine($"\t{valueName}: {value}");
                                            }
                                            catch (Exception ex)
                                            {
                                                interfacesInfo.AppendLine($"\tError reading value '{valueName}': {ex.Message}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        interfacesInfo.AppendLine($"\tError: Could not open subkey '{subKeyName}'");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                interfacesInfo.AppendLine($"\tError opening interface subkey '{subKeyName}': {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        interfacesInfo.AppendLine("The registry key does not exist: " + registryPath);
                    }
                }
            }
            catch (Exception ex)
            {
                interfacesInfo.AppendLine($"Error accessing the registry path '{registryPath}': {ex.Message}");
            }

            return interfacesInfo.ToString();
        }
    }
}

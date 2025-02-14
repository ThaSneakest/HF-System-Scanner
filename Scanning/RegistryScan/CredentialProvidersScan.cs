using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class CredentialProvidersScan
    {
        public string GetCredentialProviders()
        {
            StringBuilder credentialProviders = new StringBuilder();
            string[] registryPaths = new string[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers", // 64-bit
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers" // 32-bit
            };

            foreach (string registryPath in registryPaths)
            {
                try
                {
                    using (RegistryKey credentialProvidersKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
                    {
                        if (credentialProvidersKey != null)
                        {
                            // Enumerate subkeys (credential providers)
                            foreach (string subKeyName in credentialProvidersKey.GetSubKeyNames())
                            {
                                try
                                {
                                    // Build full path for each credential provider
                                    string subKeyPath = registryPath + @"\" + subKeyName;
                                    using (RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKeyPath))
                                    {
                                        if (subKey != null)
                                        {
                                            // Retrieve and log the default value of the credential provider (if any)
                                            object defaultValue = subKey.GetValue("", null);  // Default value of the registry key
                                            credentialProviders.AppendLine($"Credential Provider: {subKeyName}");

                                            // If the default value exists, log it
                                            if (defaultValue != null)
                                            {
                                                credentialProviders.AppendLine($"\tDefault value: {defaultValue}");
                                            }
                                            else
                                            {
                                                credentialProviders.AppendLine("\tDefault value: Not set.");
                                            }

                                            // Attempt to fetch other details from the registry values
                                            string clsid = (string)Microsoft.Win32.Registry.GetValue(subKeyPath, "CLSID", null);
                                            string name = (string)Microsoft.Win32.Registry.GetValue(subKeyPath, "Name", null);
                                            string path = (string)Microsoft.Win32.Registry.GetValue(subKeyPath, "Path", null);

                                            // Log the raw CLSID value for debugging
                                            credentialProviders.AppendLine("\tRaw CLSID: " + clsid);

                                            // Only process if CLSID is non-empty and in the valid GUID format
                                            if (!string.IsNullOrEmpty(clsid))
                                            {
                                                // Check if CLSID contains curly braces and remove them if present
                                                string clsidFormatted = clsid.Trim('{', '}');

                                                // Validate the format of the CLSID (must be a valid GUID)
                                                Guid parsedGuid;
                                                if (Guid.TryParse(clsidFormatted, out parsedGuid))
                                                {
                                                    // Build the correct CLSID registry path
                                                    string clsidKeyPath = @"SOFTWARE\Classes\CLSID\" + clsidFormatted;
                                                    credentialProviders.AppendLine("\tAttempting to access CLSID at: " + clsidKeyPath);

                                                    try
                                                    {
                                                        string clsidDescription = (string)Microsoft.Win32.Registry.GetValue(clsidKeyPath, "", null);

                                                        // If description exists, log it
                                                        if (!string.IsNullOrEmpty(clsidDescription))
                                                        {
                                                            credentialProviders.AppendLine("\tDescription: " + clsidDescription);
                                                        }
                                                        else
                                                        {
                                                            credentialProviders.AppendLine("\tNo description found for CLSID.");
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // Log any errors accessing the CLSID key
                                                        credentialProviders.AppendLine("\tError accessing CLSID key: " + ex.Message);
                                                    }
                                                }
                                                else
                                                {
                                                    // Log invalid GUID
                                                    credentialProviders.AppendLine("\tInvalid CLSID format: " + clsid);
                                                }
                                            }

                                            // Log other provider details
                                            if (!string.IsNullOrEmpty(name))
                                                credentialProviders.AppendLine("\tName: " + name);
                                            if (!string.IsNullOrEmpty(path))
                                                credentialProviders.AppendLine("\tPath: " + path);

                                            credentialProviders.AppendLine();  // New line after each credential provider
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    credentialProviders.AppendLine($"Error accessing credential provider '{subKeyName}': {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            credentialProviders.AppendLine($"The registry key does not exist: {registryPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    credentialProviders.AppendLine($"Error accessing the registry path '{registryPath}': {ex.Message}");
                }
            }

            return credentialProviders.ToString();
        }
    }
}

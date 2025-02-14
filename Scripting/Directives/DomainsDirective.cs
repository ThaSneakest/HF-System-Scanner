using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DomainsDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("Domains::"))
                    {
                        Console.WriteLine("Domains:: directive found. Resetting registry keys.");
                        ResetDomainsRegistryKeys();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ResetDomainsRegistryKeys()
        {
            try
            {
                // Define the registry paths for EscDomains, Domains, and Ranges
                string escDomainsPath = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\EscDomains";
                string domainsPath = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains";
                string rangesPath = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Ranges";

                // Reset each registry key
                DeleteRegistryKeyTree(escDomainsPath);
                DeleteRegistryKeyTree(domainsPath);
                DeleteRegistryKeyTree(rangesPath);

                Console.WriteLine("EscDomains, Domains, and Ranges have been reset to default.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to reset registry keys. Please run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting registry keys: {ex.Message}");
            }
        }

        private static void DeleteRegistryKeyTree(string registryPath)
        {
            try
            {
                string[] pathParts = registryPath.Split(new[] { '\\' }, 2);
                string rootKeyName = pathParts[0];
                string subKeyPath = pathParts.Length > 1 ? pathParts[1] : null;

                RegistryKey rootKey;

                switch (rootKeyName.ToUpper())
                {
                    case "HKEY_LOCAL_MACHINE":
                        rootKey = Microsoft.Win32.Registry.LocalMachine;
                        break;
                    case "HKEY_CURRENT_USER":
                        rootKey = Microsoft.Win32.Registry.CurrentUser;
                        break;
                    case "HKEY_CLASSES_ROOT":
                        rootKey = Microsoft.Win32.Registry.ClassesRoot;
                        break;
                    case "HKEY_USERS":
                        rootKey = Microsoft.Win32.Registry.Users;
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        rootKey = Microsoft.Win32.Registry.CurrentConfig;
                        break;
                    default:
                        throw new ArgumentException($"Invalid root key: {rootKeyName}");
                }


                if (subKeyPath != null)
                {
                    using (RegistryKey key = rootKey.OpenSubKey(subKeyPath, writable: true))
                    {
                        if (key != null)
                        {
                            rootKey.DeleteSubKeyTree(subKeyPath);
                            Console.WriteLine($"Deleted registry key: {registryPath}");
                        }
                        else
                        {
                            Console.WriteLine($"Registry key not found: {registryPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key '{registryPath}': {ex.Message}");
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RegUnlockDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

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
                    if (line.StartsWith("RegUnlock::"))
                    {
                        string registryPath = line.Substring("RegUnlock::".Length).Trim();
                        if (!string.IsNullOrEmpty(registryPath))
                        {
                            Console.WriteLine($"RegUnlock:: directive found. Unlocking registry path: {registryPath}");
                            UnlockRegistryKey(registryPath);
                        }
                        else
                        {
                            Console.WriteLine("RegUnlock:: directive found, but no registry path was specified.");
                        }
                    }

                    // Add additional directive handling logic here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void UnlockRegistryKey(string registryPath)
        {
            try
            {
                // Parse the registry root and subkey
                string rootKeyName = registryPath.Split('\\')[0];
                string subKeyPath = registryPath.Substring(rootKeyName.Length + 1);

                RegistryKey rootKey;
                switch (rootKeyName.ToUpper())
                {
                    case "HKEY_CLASSES_ROOT":
                        rootKey = Microsoft.Win32.Registry.ClassesRoot;
                        break;
                    case "HKEY_CURRENT_USER":
                        rootKey = Microsoft.Win32.Registry.CurrentUser;
                        break;
                    case "HKEY_LOCAL_MACHINE":
                        rootKey = Microsoft.Win32.Registry.LocalMachine;
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


                // Open the subkey with write access
                using (RegistryKey subKey = rootKey.OpenSubKey(subKeyPath, writable: true))
                {
                    if (subKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {registryPath}");
                        return;
                    }

                    // Simulate unlocking by checking write permissions
                    Console.WriteLine($"Unlocking registry key: {registryPath}");
                    string[] valueNames = subKey.GetValueNames();
                    foreach (string valueName in valueNames)
                    {
                        object value = subKey.GetValue(valueName);
                        Console.WriteLine($"Value: {valueName}, Data: {value}");
                    }

                    Console.WriteLine($"Registry key {registryPath} unlocked successfully.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to registry key: {registryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking registry key '{registryPath}': {ex.Message}");
            }
        }
    }
}

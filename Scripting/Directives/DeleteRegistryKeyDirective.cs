using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DeleteRegistryKeyDirective
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
                    if (line.StartsWith("DeleteRegKey::"))
                    {
                        string registryPath = line.Substring("DeleteRegKey::".Length).Trim();
                        if (!string.IsNullOrEmpty(registryPath))
                        {
                            Console.WriteLine($"DeleteRegKey:: directive found. Deleting registry key: {registryPath}");
                            DeleteRegistryKey(registryPath);
                        }
                        else
                        {
                            Console.WriteLine("DeleteRegKey:: directive found, but no registry path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeleteRegistryKey(string registryPath)
        {
            try
            {
                // Parse the root key and subkey
                RegistryKey rootKey = GetRootKey(registryPath, out string subKeyPath);

                using (rootKey)
                {
                    if (string.IsNullOrEmpty(subKeyPath))
                    {
                        Console.WriteLine($"Invalid registry path: {registryPath}");
                        return;
                    }

                    if (rootKey.OpenSubKey(subKeyPath) == null)
                    {
                        Console.WriteLine($"Registry key not found: {registryPath}");
                        return;
                    }

                    rootKey.DeleteSubKeyTree(subKeyPath);
                    Console.WriteLine($"Successfully deleted registry key: {registryPath}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to delete registry key: {registryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key '{registryPath}': {ex.Message}");
            }
        }

        private static RegistryKey GetRootKey(string fullPath, out string subKeyPath)
        {
            string[] parts = fullPath.Split(new[] { '\\' }, 2);
            string root = parts[0].ToUpper();
            subKeyPath = parts.Length > 1 ? parts[1] : string.Empty;

            switch (root)
            {
                case "HKEY_CLASSES_ROOT":
                    return Microsoft.Win32.Registry.ClassesRoot;
                case "HKEY_CURRENT_USER":
                    return Microsoft.Win32.Registry.CurrentUser;
                case "HKEY_LOCAL_MACHINE":
                    return Microsoft.Win32.Registry.LocalMachine;
                case "HKEY_USERS":
                    return Microsoft.Win32.Registry.Users;
                case "HKEY_CURRENT_CONFIG":
                    return Microsoft.Win32.Registry.CurrentConfig;
                default:
                    throw new ArgumentException($"Invalid root key: {root}");
            }
        }
    }
}

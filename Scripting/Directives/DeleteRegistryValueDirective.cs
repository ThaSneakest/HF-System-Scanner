using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DeleteRegistryValueDirective
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
                    if (line.StartsWith("DeleteRegValue::"))
                    {
                        string command = line.Substring("DeleteRegValue::".Length).Trim();
                        if (!string.IsNullOrEmpty(command))
                        {
                            Console.WriteLine($"DeleteRegValue:: directive found. Processing: {command}");
                            DeleteRegistryValue(command);
                        }
                        else
                        {
                            Console.WriteLine("DeleteRegValue:: directive found, but no command was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeleteRegistryValue(string command)
        {
            try
            {
                // Split the command into key path and value name
                string[] parts = command.Split(new[] { '|' }, 2);
                if (parts.Length != 2)
                {
                    Console.WriteLine($"Invalid command format: {command}");
                    return;
                }

                string keyPath = parts[0].Trim();
                string valueName = parts[1].Trim();

                RegistryKey rootKey = GetRootKey(keyPath, out string subKeyPath);

                using (RegistryKey key = rootKey.OpenSubKey(subKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    if (key.GetValue(valueName) == null)
                    {
                        Console.WriteLine($"Registry value not found: {keyPath}\\{valueName}");
                        return;
                    }

                    key.DeleteValue(valueName);
                    Console.WriteLine($"Successfully deleted registry value: {keyPath}\\{valueName}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to delete registry value.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
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

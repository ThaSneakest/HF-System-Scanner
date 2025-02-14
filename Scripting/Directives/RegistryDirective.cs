using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RegistryDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives file

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
                    if (line.StartsWith("Registry::"))
                    {
                        string command = line.Substring("Registry::".Length).Trim();
                        ProcessRegistryCommand(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ProcessRegistryCommand(string command)
        {
            try
            {
                // Parse the command
                string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 4)
                {
                    // Merge remaining parts if more than 4 elements exist
                    parts = new[] { parts[0], parts[1], parts[2], string.Join(" ", parts.Skip(3)) };
                }


                string keyPath = parts[0];
                string valueName = parts.Length > 2 ? parts[1] : null;
                string operationOrType = parts.Length > 2 ? parts[2] : null;
                string data = parts.Length > 3 ? parts[3] : null;

                RegistryKey rootKey = GetRootKey(keyPath, out string subKeyPath);

                using (RegistryKey key = rootKey.OpenSubKey(subKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    if (operationOrType.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (valueName == null)
                        {
                            // Delete the entire key
                            rootKey.DeleteSubKeyTree(subKeyPath);
                            Console.WriteLine($"Registry key deleted: {keyPath}");
                        }
                        else
                        {
                            // Delete the specified value
                            key.DeleteValue(valueName, throwOnMissingValue: false);
                            Console.WriteLine($"Registry value deleted: {keyPath}\\{valueName}");
                        }
                    }
                    else
                    {
                        // Add/Modify a value
                        object value = ParseData(operationOrType, data);
                        key.SetValue(valueName, value, ParseValueKind(operationOrType));
                        Console.WriteLine($"Registry value set: {keyPath}\\{valueName} = {data} ({operationOrType})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing registry command '{command}': {ex.Message}");
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


        private static RegistryValueKind ParseValueKind(string type)
        {
            switch (type.ToUpper())
            {
                case "REG_SZ":
                    return RegistryValueKind.String;
                case "REG_DWORD":
                    return RegistryValueKind.DWord;
                case "REG_QWORD":
                    return RegistryValueKind.QWord;
                case "REG_BINARY":
                    return RegistryValueKind.Binary;
                case "REG_MULTI_SZ":
                    return RegistryValueKind.MultiString;
                default:
                    throw new ArgumentException($"Unsupported registry value type: {type}");
            }
        }


        private static object ParseData(string type, string data)
        {
            switch (type.ToUpper())
            {
                case "REG_SZ":
                    return data;
                case "REG_DWORD":
                    return int.Parse(data);
                case "REG_QWORD":
                    return long.Parse(data);
                case "REG_BINARY":
                    return Convert.FromBase64String(data);
                case "REG_MULTI_SZ":
                    return data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                default:
                    throw new ArgumentException($"Unsupported registry value type: {type}");
            }
        }

    }
}

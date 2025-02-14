using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ExportRegValueDirective
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
                    if (line.StartsWith("ExportRegValue::"))
                    {
                        string keyValueAndOutput = line.Substring("ExportRegValue::".Length).Trim();
                        if (!string.IsNullOrEmpty(keyValueAndOutput))
                        {
                            string[] parts = keyValueAndOutput.Split(new[] { '|' }, 3, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length == 3)
                            {
                                string registryKey = parts[0].Trim();
                                string registryValue = parts[1].Trim();
                                string outputFilePath = parts[2].Trim();

                                Console.WriteLine($"ExportRegValue:: directive found. Exporting value '{registryValue}' from key '{registryKey}' to {outputFilePath}");
                                ExportRegistryValue(registryKey, registryValue, outputFilePath);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid ExportRegValue:: format. Expected 'RegistryKey|ValueName|OutputFilePath', got: {keyValueAndOutput}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ExportRegValue:: directive found, but no registry key or value was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExportRegistryValue(string registryKeyPath, string valueName, string outputFilePath)
        {
            try
            {
                RegistryKey baseKey = GetBaseKey(ref registryKeyPath);
                if (baseKey == null)
                {
                    Console.WriteLine($"Invalid registry base key in: {registryKeyPath}");
                    return;
                }

                using (RegistryKey key = baseKey.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {registryKeyPath}");
                        return;
                    }

                    object value = key.GetValue(valueName);
                    if (value == null)
                    {
                        Console.WriteLine($"Registry value '{valueName}' not found in key: {registryKeyPath}");
                        return;
                    }

                    // Write the value to the output file
                    using (StreamWriter writer = new StreamWriter(outputFilePath, false))
                    {
                        writer.WriteLine($"Windows Registry Editor Version 5.00");
                        writer.WriteLine($"[{baseKey.Name}\\{registryKeyPath}]");
                        writer.WriteLine($"\"{valueName}\"={FormatRegistryValue(value, key.GetValueKind(valueName))}");
                    }

                    Console.WriteLine($"Successfully exported value '{valueName}' from key '{registryKeyPath}' to {outputFilePath}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to export registry value '{valueName}' from '{registryKeyPath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting registry value '{valueName}' from '{registryKeyPath}': {ex.Message}");
            }
        }

        private static string FormatRegistryValue(object value, RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                    return $"\"{value}\"";

                case RegistryValueKind.DWord:
                    return $"dword:{Convert.ToInt32(value):X8}";

                case RegistryValueKind.QWord:
                    return $"qword:{Convert.ToInt64(value):X16}";

                case RegistryValueKind.MultiString:
                    return string.Join("", ((string[])value).Select(s => $"\"{s}\",").ToArray()).TrimEnd(',');

                case RegistryValueKind.Binary:
                    return "hex:" + BitConverter.ToString((byte[])value).Replace("-", ",");

                default:
                    return $"\"{value}\"";
            }
        }

        private static RegistryKey GetBaseKey(ref string registryKeyPath)
        {
            if (registryKeyPath.StartsWith(@"HKEY_CLASSES_ROOT\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CLASSES_ROOT\".Length);
                return Microsoft.Win32.Registry.ClassesRoot;
            }
            if (registryKeyPath.StartsWith(@"HKEY_CURRENT_USER\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CURRENT_USER\".Length);
                return Microsoft.Win32.Registry.CurrentUser;
            }
            if (registryKeyPath.StartsWith(@"HKEY_LOCAL_MACHINE\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_LOCAL_MACHINE\".Length);
                return Microsoft.Win32.Registry.LocalMachine;
            }
            if (registryKeyPath.StartsWith(@"HKEY_USERS\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_USERS\".Length);
                return Microsoft.Win32.Registry.Users;
            }
            if (registryKeyPath.StartsWith(@"HKEY_CURRENT_CONFIG\"))
            {
                registryKeyPath = registryKeyPath.Substring(@"HKEY_CURRENT_CONFIG\".Length);
                return Microsoft.Win32.Registry.CurrentConfig;
            }

            return null;
        }
    }
}

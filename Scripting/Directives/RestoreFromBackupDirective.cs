using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RestoreFromBackupDirective
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
                    if (line.StartsWith("RestoreFromBackup::"))
                    {
                        string arguments = line.Substring("RestoreFromBackup::".Length).Trim();
                        if (!string.IsNullOrEmpty(arguments))
                        {
                            string[] parts = arguments.Split(new[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                string hive = parts[0].Trim();
                                string backupFile = parts[1].Trim();
                                Console.WriteLine($"RestoreFromBackup:: directive found. Restoring hive '{hive}' from backup: {backupFile}");
                                RestoreRegistryHive(hive, backupFile);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid RestoreFromBackup:: format. Expected 'Hive|BackupFilePath', got: {arguments}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("RestoreFromBackup:: directive found, but no hive or backup file specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RestoreRegistryHive(string hive, string backupFile)
        {
            try
            {
                if (!File.Exists(backupFile))
                {
                    Console.WriteLine($"Backup file not found: {backupFile}");
                    return;
                }

                // Validate the hive
                if (!IsValidHive(hive))
                {
                    Console.WriteLine($"Invalid registry hive: {hive}");
                    return;
                }

                // Use reg.exe to restore the hive
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"restore \"{hive}\" \"{backupFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Registry Restore Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Registry Restore Error:");
                        Console.WriteLine(error);
                    }

                    Console.WriteLine($"Registry hive '{hive}' restored from backup with exit code {process.ExitCode}.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to restore registry hive '{hive}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry hive '{hive}': {ex.Message}");
            }
        }

        private static bool IsValidHive(string hive)
        {
            string[] validHives = new[]
            {
                "HKEY_CLASSES_ROOT",
                "HKEY_CURRENT_USER",
                "HKEY_LOCAL_MACHINE",
                "HKEY_USERS",
                "HKEY_CURRENT_CONFIG"
            };

            return Array.Exists(validHives, h => h.Equals(hive, StringComparison.OrdinalIgnoreCase));
        }
    }
}

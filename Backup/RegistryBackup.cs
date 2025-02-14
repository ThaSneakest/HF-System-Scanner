using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Wildlands_System_Scanner.Backup
{
    public class RegistryBackup
    {
        /// <summary>
        /// Backs up the entire registry by exporting all hives to .reg files.
        /// </summary>
        /// <param name="backupDirectory">The directory where the backup files will be saved.</param>
        public static void BackupEntireRegistry(string backupDirectory)
        {
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            string[] registryHives = new[]
            {
                "HKEY_LOCAL_MACHINE", "HKEY_CURRENT_USER", "HKEY_CLASSES_ROOT", "HKEY_USERS", "HKEY_CURRENT_CONFIG"
            };

            foreach (var hive in registryHives)
            {
                string outputFilePath = Path.Combine(backupDirectory, $"{hive.Replace('\\', '_')}.reg");

                try
                {
                    BackupRegistryHive(hive, outputFilePath);
                    Console.WriteLine($"Successfully backed up {hive} to {outputFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error backing up {hive}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Exports a specific registry hive to a .reg file.
        /// </summary>
        /// <param name="hive">The registry hive to export (e.g., "HKEY_LOCAL_MACHINE").</param>
        /// <param name="outputFile">The output .reg file path.</param>
        private static void BackupRegistryHive(string hive, string outputFile)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{hive}\" \"{outputFile}\" /y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new InvalidOperationException($"Failed to export {hive}. Error: {error}");
                }
            }
        }
    }
}
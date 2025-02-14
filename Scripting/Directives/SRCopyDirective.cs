using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SRCopyDirective
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
                    if (line.StartsWith("SRCopy::"))
                    {
                        string targetFilePath = line.Substring("SRCopy::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetFilePath))
                        {
                            Console.WriteLine($"SRCopy:: directive found. Restoring file: {targetFilePath}");
                            RestoreFileFromRestorePoint(targetFilePath);
                        }
                        else
                        {
                            Console.WriteLine("SRCopy:: directive found, but no file path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RestoreFileFromRestorePoint(string targetFilePath)
        {
            try
            {
                // Get the original file's directory and name
                string directory = Path.GetDirectoryName(targetFilePath);
                string fileName = Path.GetFileName(targetFilePath);

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"Invalid file path: {targetFilePath}");
                    return;
                }

                Console.WriteLine($"Searching for restore points for file: {targetFilePath}");

                // Use WMI to query Volume Shadow Copies
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ShadowCopy");

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                using (ManagementObjectCollection shadowCopies = searcher.Get())
                {
                    foreach (ManagementObject shadowCopy in shadowCopies)
                    {
                        string deviceObject = shadowCopy["DeviceObject"]?.ToString();
                        if (string.IsNullOrEmpty(deviceObject))
                        {
                            continue;
                        }

                        string shadowPath = $"{deviceObject}\\{directory}";

                        if (Directory.Exists(shadowPath))
                        {
                            string backupFilePath = Path.Combine(shadowPath, fileName);

                            if (File.Exists(backupFilePath))
                            {
                                Console.WriteLine($"Backup file found in restore point: {backupFilePath}");
                                OverwriteOriginalFile(backupFilePath, targetFilePath);
                                return;
                            }
                        }
                    }
                }

                Console.WriteLine($"No backup file found in restore points for: {targetFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring file '{targetFilePath}': {ex.Message}");
            }
        }

        private static void OverwriteOriginalFile(string backupFilePath, string targetFilePath)
        {
            try
            {
                File.Copy(backupFilePath, targetFilePath, overwrite: true);
                Console.WriteLine($"File restored successfully: {targetFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error overwriting file '{targetFilePath}': {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class BackupCopyDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives text file
            string backupFolder = @"C:\BackupFolder"; // Path to the backup folder

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
                    if (line.StartsWith("BackupCopy::"))
                    {
                        string targetFilePath = line.Substring("BackupCopy::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetFilePath))
                        {
                            Console.WriteLine($"BackupCopy:: directive found. Restoring file: {targetFilePath}");
                            RestoreFileFromBackup(targetFilePath, backupFolder);
                        }
                        else
                        {
                            Console.WriteLine("BackupCopy:: directive found, but no file path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RestoreFileFromBackup(string targetFilePath, string backupFolder)
        {
            try
            {
                string fileName = Path.GetFileName(targetFilePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"Invalid file path: {targetFilePath}");
                    return;
                }

                string backupFilePath = Path.Combine(backupFolder, fileName);

                if (!File.Exists(backupFilePath))
                {
                    Console.WriteLine($"Backup file not found: {backupFilePath}");
                    return;
                }

                File.Copy(backupFilePath, targetFilePath, overwrite: true);
                Console.WriteLine($"File restored successfully from backup: {targetFilePath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to restore file: {targetFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring file '{targetFilePath}': {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class MissingDirective
    {
        // Define the backup folder where missing files can be restored from
        private static readonly string BackupFolderPath = @"C:\BackupFiles"; // Adjust to your backup folder path

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
                    if (line.StartsWith("Missing::"))
                    {
                        string command = line.Substring("Missing::".Length).Trim();
                        ProcessMissingFileCommand(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ProcessMissingFileCommand(string command)
        {
            string[] parts = command.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.WriteLine($"Invalid Missing:: directive: {command}");
                return;
            }

            string targetFilePath = parts[0].Trim();
            string backupFileName = parts[1].Trim();  // The name of the backup file

            try
            {
                if (File.Exists(targetFilePath))
                {
                    Console.WriteLine($"File already exists at {targetFilePath}. No restoration needed.");
                }
                else
                {
                    Console.WriteLine($"File missing at {targetFilePath}. Attempting to restore from backup.");
                    RestoreFileFromBackup(targetFilePath, backupFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Missing:: directive: {ex.Message}");
            }
        }

        private static void RestoreFileFromBackup(string targetFilePath, string backupFileName)
        {
            try
            {
                string backupFilePath = Path.Combine(BackupFolderPath, backupFileName);

                if (!File.Exists(backupFilePath))
                {
                    Console.WriteLine($"Backup file not found: {backupFilePath}");
                    return;
                }

                // Copy the backup file to the target location, overwriting if necessary
                File.Copy(backupFilePath, targetFilePath, overwrite: true);
                Console.WriteLine($"File successfully restored to {targetFilePath} from backup.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to restore file '{targetFilePath}' from backup.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring file '{targetFilePath}' from backup: {ex.Message}");
            }
        }
    }
}

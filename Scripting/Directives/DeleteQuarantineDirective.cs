using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DeleteQuarantineDirective
    {
        // Define the default quarantine folder path
        private static readonly string DefaultQuarantinePath = @"C:\WSS\Quarantine";

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
                    if (line.StartsWith("DeleteQuarantine::"))
                    {
                        string specifiedPath = line.Substring("DeleteQuarantine::".Length).Trim();
                        string quarantinePath = string.IsNullOrEmpty(specifiedPath) ? DefaultQuarantinePath : specifiedPath;

                        Console.WriteLine($"DeleteQuarantine:: directive found. Removing quarantine folder: {quarantinePath}");
                        RemoveQuarantineFolder(quarantinePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RemoveQuarantineFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Quarantine folder not found: {folderPath}");
                    return;
                }

                // Delete the quarantine folder and all its contents
                Directory.Delete(folderPath, recursive: true);
                Console.WriteLine($"Quarantine folder successfully removed: {folderPath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to delete quarantine folder at {folderPath}. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing quarantine folder '{folderPath}': {ex.Message}");
            }
        }
    }
}

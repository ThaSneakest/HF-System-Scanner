using System;
using System.Collections.Generic;
using System.IO;

namespace Wildlands_System_Scanner.Blacklist
{
    public class ProgramDataFolderBlacklist
    {
        public static void ScanForBlacklistedFolders()
        {
            // Predefined blacklist of folder names in ProgramData (case insensitive)
            HashSet<string> blacklistedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MalwareConfig",          // Example: Folder for malicious configuration
                "SuspiciousBackup",       // Example: Folder for suspicious backup files
                "UnverifiedPlugins",      // Example: Folder for unverified plugins
                "HiddenMalware",          // Example: Folder containing hidden malware
                "HijackedSettings"        // Example: Folder for hijacked settings
            };

            // Path to the ProgramData directory
            string programDataPath = @"C:\ProgramData\";

            Console.WriteLine($"Scanning folders in {programDataPath}...");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(programDataPath))
                {
                    Console.WriteLine($"Directory not found: {programDataPath}");
                    return;
                }

                // Get all directories in ProgramData and subdirectories
                string[] directories = Directory.GetDirectories(programDataPath, "*", SearchOption.AllDirectories);

                // Check each folder against the blacklist
                foreach (string directory in directories)
                {
                    string folderName = Path.GetFileName(directory);

                    if (blacklistedFolders.Contains(folderName))
                    {
                        Logger.Instance.LogPrimary($"Blacklisted folder found: {directory}");
                        // Optionally, take action (e.g., delete or quarantine the folder)
                        // Directory.Delete(directory, true); // Be cautious with this line!
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to some directories: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine($"Folder path too long: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during scanning: {ex.Message}");
            }

            Console.WriteLine("Scan complete.");
        }
    }
}

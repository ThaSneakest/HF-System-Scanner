using System;
using System.Collections.Generic;
using System.IO;

namespace Wildlands_System_Scanner.Blacklist
{
    public class ProgramDataFileBlacklist
    {
        public static void ScanForBlacklistedFiles()
        {
            // Predefined blacklist of file names in ProgramData (case insensitive)
            HashSet<string> blacklistedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "malicious_config.ini",   // Example: Malicious configuration file
                "hidden_payload.exe",     // Example: Executable hidden in ProgramData
                "ransom_note.txt",        // Example: Ransomware note
                "untrusted_update.exe",   // Example: Unverified update executable
                "suspicious_script.ps1"   // Example: PowerShell script
            };

            // Path to the ProgramData directory
            string programDataPath = @"C:\ProgramData\";

            Console.WriteLine($"Scanning files in {programDataPath}...");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(programDataPath))
                {
                    Console.WriteLine($"Directory not found: {programDataPath}");
                    return;
                }

                // Get all files in ProgramData directory and subdirectories
                string[] files = Directory.GetFiles(programDataPath, "*.*", SearchOption.AllDirectories);

                // Check each file against the blacklist
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);

                    if (blacklistedFiles.Contains(fileName))
                    {
                        Logger.Instance.LogPrimary($"Blacklisted file found: {file}");
                        // Optionally, take action (e.g., delete or quarantine the file)
                        // File.Delete(file); // Be cautious with this line!
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to some directories: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine($"File path too long: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during scanning: {ex.Message}");
            }

            Console.WriteLine("Scan complete.");
        }
    }
}

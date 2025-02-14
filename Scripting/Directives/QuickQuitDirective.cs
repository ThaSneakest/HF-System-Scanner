using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class QuickQuitDirective
    {
        private const string QuarantineFolderPath = @"C:\WSS_Quarantine";
        private const string QuarantineLogFilePath = @"C:\WSS_Quarantine\quarantine_log.txt";
        private static bool quickQuitMode = false;

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

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
                    if (line.StartsWith("QuickQuit::"))
                    {
                        Console.WriteLine("QuickQuit:: directive found. Application will quit after restoring from quarantine.");
                        quickQuitMode = true;
                        continue;
                    }

                    if (line.StartsWith("DeQuarantine:::"))
                    {
                        string quarantinedItem = line.Substring("DeQuarantine:::".Length).Trim();
                        if (!string.IsNullOrEmpty(quarantinedItem))
                        {
                            Console.WriteLine($"DeQuarantine::: directive found. Restoring: {quarantinedItem}");
                            RestoreFromQuarantine(quarantinedItem);
                        }
                        else
                        {
                            Console.WriteLine("DeQuarantine::: directive found, but no file or folder was specified.");
                        }
                    }

                    // Add additional directive handling logic here.
                }

                // If QuickQuit:: mode is active, exit the application early
                if (quickQuitMode)
                {
                    Console.WriteLine("QuickQuit:: mode active. Exiting application.");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RestoreFromQuarantine(string quarantinedItem)
        {
            try
            {
                if (!Directory.Exists(QuarantineFolderPath))
                {
                    Console.WriteLine($"Quarantine folder not found: {QuarantineFolderPath}");
                    return;
                }

                if (!File.Exists(QuarantineLogFilePath))
                {
                    Console.WriteLine($"Quarantine log not found: {QuarantineLogFilePath}");
                    return;
                }

                // Read the quarantine log to find the original location
                string[] logEntries = File.ReadAllLines(QuarantineLogFilePath);
                string originalPath = null;

                foreach (var entry in logEntries)
                {
                    string[] parts = entry.Split('|'); // Format: <QuarantinedPath>|<OriginalPath>
                    if (parts.Length == 2 && string.Equals(parts[0], quarantinedItem, StringComparison.OrdinalIgnoreCase))
                    {
                        originalPath = parts[1];
                        break;
                    }
                }

                if (originalPath == null)
                {
                    Console.WriteLine($"No entry found in quarantine log for: {quarantinedItem}");
                    return;
                }

                string quarantinedPath = Path.Combine(QuarantineFolderPath, quarantinedItem);

                if (!File.Exists(quarantinedPath) && !Directory.Exists(quarantinedPath))
                {
                    Console.WriteLine($"Quarantined item not found: {quarantinedPath}");
                    return;
                }

                // Ensure the original directory exists
                string originalDirectory = Path.GetDirectoryName(originalPath);
                if (!Directory.Exists(originalDirectory))
                {
                    Directory.CreateDirectory(originalDirectory);
                    Console.WriteLine($"Created missing directory: {originalDirectory}");
                }

                // Restore the item
                if (File.Exists(quarantinedPath))
                {
                    File.Move(quarantinedPath, originalPath);
                    Console.WriteLine($"Restored file: {originalPath}");
                }
                else if (Directory.Exists(quarantinedPath))
                {
                    Directory.Move(quarantinedPath, originalPath);
                    Console.WriteLine($"Restored folder: {originalPath}");
                }

                // Remove the log entry
                File.WriteAllLines(QuarantineLogFilePath, RemoveLogEntry(logEntries, quarantinedItem));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring from quarantine: {ex.Message}");
            }
        }

        private static string[] RemoveLogEntry(string[] logEntries, string quarantinedItem)
        {
            var updatedEntries = new System.Collections.Generic.List<string>();

            foreach (var entry in logEntries)
            {
                if (!entry.StartsWith($"{quarantinedItem}|", StringComparison.OrdinalIgnoreCase))
                {
                    updatedEntries.Add(entry);
                }
            }

            return updatedEntries.ToArray();
        }
    }
}

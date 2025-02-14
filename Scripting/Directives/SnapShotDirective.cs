using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SnapShotDirective
    {
        private const string SnapshotFilePath = "snapshot.txt"; // Path to store the snapshot
        private const string WindowsDirectory = @"C:\Windows"; // Target directory for snapshots

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
                    if (line.StartsWith("SnapShot::"))
                    {
                        Console.WriteLine("SnapShot:: directive found. Processing snapshot...");
                        HandleSnapshot();
                        continue;
                    }

                    // Add additional directive handling here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void HandleSnapshot()
        {
            try
            {
                if (!Directory.Exists(WindowsDirectory))
                {
                    Console.WriteLine($"The directory '{WindowsDirectory}' does not exist.");
                    return;
                }

                // Get the current file listing in the Windows directory
                List<string> currentSnapshot = Directory.GetFiles(WindowsDirectory, "*.*", SearchOption.AllDirectories).ToList();

                if (!File.Exists(SnapshotFilePath))
                {
                    // First run: Save the snapshot
                    File.WriteAllLines(SnapshotFilePath, currentSnapshot);
                    Console.WriteLine($"Initial snapshot saved with {currentSnapshot.Count} files.");
                }
                else
                {
                    // Subsequent run: Compare the snapshot
                    List<string> previousSnapshot = File.ReadAllLines(SnapshotFilePath).ToList();

                    var addedFiles = currentSnapshot.Except(previousSnapshot).ToList();
                    var removedFiles = previousSnapshot.Except(currentSnapshot).ToList();

                    // Output the comparison results
                    Console.WriteLine("Snapshot comparison results:");
                    Console.WriteLine($"Added files: {addedFiles.Count}");
                    foreach (var file in addedFiles)
                    {
                        Console.WriteLine($"  + {file}");
                    }

                    Console.WriteLine($"Removed files: {removedFiles.Count}");
                    foreach (var file in removedFiles)
                    {
                        Console.WriteLine($"  - {file}");
                    }

                    // Save the current snapshot for future comparisons
                    File.WriteAllLines(SnapshotFilePath, currentSnapshot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during snapshot processing: {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SnapShotAltDirective
    {
        private const string OldSnapshotFilePath = "old_snapshot.txt"; // File for storing the old snapshot
        private const string NewSnapshotFilePath = "new_snapshot.txt"; // File for storing the new snapshot
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
                    if (line.StartsWith("SnapShotAlt::"))
                    {
                        Console.WriteLine("SnapShotAlt:: directive found. Processing alternate snapshot...");
                        HandleSnapshotAlt();
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

        private static void HandleSnapshotAlt()
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

                if (!File.Exists(OldSnapshotFilePath))
                {
                    Console.WriteLine("No old snapshot found. Creating a new snapshot for future use.");
                    SaveSnapshot(OldSnapshotFilePath, currentSnapshot);
                    return;
                }

                // Load the old snapshot
                List<string> oldSnapshot = File.ReadAllLines(OldSnapshotFilePath).ToList();

                // Compare the current snapshot with the old snapshot
                var addedFiles = currentSnapshot.Except(oldSnapshot).ToList();
                var removedFiles = oldSnapshot.Except(currentSnapshot).ToList();

                // Output the comparison results
                Console.WriteLine("SnapshotAlt comparison results:");
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

                // Save the current snapshot for future use
                SaveSnapshot(NewSnapshotFilePath, currentSnapshot);

                // Move new snapshot to old snapshot for next run
                if (File.Exists(NewSnapshotFilePath))
                {
                    File.Copy(NewSnapshotFilePath, OldSnapshotFilePath, overwrite: true);
                    File.Delete(NewSnapshotFilePath);
                }

                Console.WriteLine("New snapshot has been saved and will be used for future comparisons.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during snapshot processing: {ex.Message}");
            }
        }

        private static void SaveSnapshot(string filePath, List<string> snapshot)
        {
            try
            {
                File.WriteAllLines(filePath, snapshot);
                Console.WriteLine($"Snapshot saved to {filePath} with {snapshot.Count} files.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving snapshot to {filePath}: {ex.Message}");
            }
        }
    }
}


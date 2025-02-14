using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class EmptyTempDirective
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
                    if (line.StartsWith("EmptyTemp::"))
                    {
                        Console.WriteLine("EmptyTemp:: directive found. Clearing temporary directories.");
                        ClearTemporaryDirectories();
                        break; // Execute only once
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ClearTemporaryDirectories()
        {
            // Common temporary directories
            string[] tempDirectories = new[]
            {
                Path.GetTempPath(), // System temp directory
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"), // User temp directory
                @"C:\Windows\Temp" // Windows temp directory
            };

            foreach (var tempDir in tempDirectories)
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Console.WriteLine($"Clearing directory: {tempDir}");
                        DeleteContents(tempDir);
                        Console.WriteLine($"Successfully cleared: {tempDir}");
                    }
                    else
                    {
                        Console.WriteLine($"Directory not found: {tempDir}");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied: Unable to clear {tempDir}. Run as administrator.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing directory '{tempDir}': {ex.Message}");
                }
            }
        }

        private static void DeleteContents(string directoryPath)
        {
            try
            {
                // Delete all files
                foreach (string file in Directory.GetFiles(directoryPath))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting file '{file}': {ex.Message}");
                    }
                }

                // Delete all subdirectories
                foreach (string subDir in Directory.GetDirectories(directoryPath))
                {
                    try
                    {
                        Directory.Delete(subDir, recursive: true);
                        Console.WriteLine($"Deleted directory: {subDir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting directory '{subDir}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing contents of '{directoryPath}': {ex.Message}");
            }
        }
    }
}

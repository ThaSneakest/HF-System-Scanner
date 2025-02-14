using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ZipDirective
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
                    if (line.StartsWith("Zip::"))
                    {
                        string arguments = line.Substring("Zip::".Length).Trim();
                        string[] parts = arguments.Split(new[] { '|' }, 2, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2)
                        {
                            string sourcePath = parts[0].Trim();
                            string destinationPath = parts[1].Trim();
                            Console.WriteLine($"Zip:: directive found. Zipping '{sourcePath}' to '{destinationPath}'.");
                            ZipFilesOrFolder(sourcePath, destinationPath);
                        }
                        else
                        {
                            Console.WriteLine($"Invalid Zip:: format. Expected 'SourcePath|DestinationZipPath', got: {arguments}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ZipFilesOrFolder(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(sourcePath) && !File.Exists(sourcePath))
                {
                    Console.WriteLine($"Source path not found: {sourcePath}");
                    return;
                }

                // If source is a file
                if (File.Exists(sourcePath))
                {
                    Console.WriteLine($"Zipping file: {sourcePath}");
                    using (var archive = ZipFile.Open(destinationPath, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath));
                    }
                }
                // If source is a directory
                else if (Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Zipping folder: {sourcePath}");
                    ZipFile.CreateFromDirectory(sourcePath, destinationPath, CompressionLevel.Optimal, includeBaseDirectory: true);
                }

                Console.WriteLine($"Successfully created zip file: {destinationPath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to create zip file '{destinationPath}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error zipping '{sourcePath}': {ex.Message}");
            }
        }
    }
}

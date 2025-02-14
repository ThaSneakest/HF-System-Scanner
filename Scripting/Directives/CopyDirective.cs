using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class CopyDirective
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
                    if (line.StartsWith("Copy::"))
                    {
                        string command = line.Substring("Copy::".Length).Trim();
                        string[] paths = command.Split(new[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                        if (paths.Length == 2)
                        {
                            string sourcePath = paths[0].Trim();
                            string destinationPath = paths[1].Trim();

                            Console.WriteLine($"Copy:: directive found. Copying from {sourcePath} to {destinationPath}");
                            CopyFileOrFolder(sourcePath, destinationPath);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Copy:: directive format. Use: Copy:: source_path to destination_path");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void CopyFileOrFolder(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    // Copy file
                    File.Copy(sourcePath, destinationPath, overwrite: true);
                    Console.WriteLine($"File copied successfully from {sourcePath} to {destinationPath}");
                }
                else if (Directory.Exists(sourcePath))
                {
                    // Copy folder
                    CopyDirectory(sourcePath, destinationPath);
                    Console.WriteLine($"Folder copied successfully from {sourcePath} to {destinationPath}");
                }
                else
                {
                    Console.WriteLine($"Source path does not exist: {sourcePath}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to copy from {sourcePath} to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying from {sourcePath} to {destinationPath}: {ex.Message}");
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Ensure the destination directory exists
            Directory.CreateDirectory(destinationDir);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFilePath = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFilePath, overwrite: true);
            }

            // Copy all subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class MoveDirective
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
                    if (line.StartsWith("Move::"))
                    {
                        string command = line.Substring("Move::".Length).Trim();
                        string[] paths = command.Split(new[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                        if (paths.Length == 2)
                        {
                            string sourcePath = paths[0].Trim();
                            string destinationPath = paths[1].Trim();

                            Console.WriteLine($"Move:: directive found. Moving from {sourcePath} to {destinationPath}");
                            MoveFileOrFolder(sourcePath, destinationPath);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Move:: directive format. Use: Move:: source_path to destination_path");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void MoveFileOrFolder(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    // Move file
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath); // Delete the destination file if it exists
                    }

                    File.Move(sourcePath, destinationPath);
                    Console.WriteLine($"File moved successfully from {sourcePath} to {destinationPath}");
                }
                else if (Directory.Exists(sourcePath))
                {
                    // Move folder
                    if (Directory.Exists(destinationPath))
                    {
                        Directory.Delete(destinationPath, recursive: true); // Delete the destination folder if it exists
                    }

                    Directory.Move(sourcePath, destinationPath);
                    Console.WriteLine($"Folder moved successfully from {sourcePath} to {destinationPath}");
                }
                else
                {
                    Console.WriteLine($"Source path does not exist: {sourcePath}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to move from {sourcePath} to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving from {sourcePath} to {destinationPath}: {ex.Message}");
            }
        }
    }
}

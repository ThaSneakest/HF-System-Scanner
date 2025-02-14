using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FindFolderDirective
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
                    if (line.StartsWith("FindFolder::"))
                    {
                        string folderName = line.Substring("FindFolder::".Length).Trim();
                        if (!string.IsNullOrEmpty(folderName))
                        {
                            Console.WriteLine($"FindFolder:: directive found. Searching for folder: {folderName}");
                            SearchForFolder(folderName, "C:\\"); // You can change the starting directory as needed
                        }
                        else
                        {
                            Console.WriteLine("FindFolder:: directive found, but no folder name was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void SearchForFolder(string folderName, string startDirectory)
        {
            try
            {
                List<string> matchingFolders = new List<string>();

                void SearchDirectory(string directory)
                {
                    try
                    {
                        // Check if the current directory matches the folder name
                        if (Path.GetFileName(directory).Equals(folderName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingFolders.Add(directory);
                        }

                        // Recursively search subdirectories
                        foreach (string subDirectory in Directory.GetDirectories(directory))
                        {
                            SearchDirectory(subDirectory);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Access denied: Unable to search directory {directory}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error searching directory '{directory}': {ex.Message}");
                    }
                }

                SearchDirectory(startDirectory);

                if (matchingFolders.Count > 0)
                {
                    Console.WriteLine($"Folders matching '{folderName}':");
                    foreach (string folder in matchingFolders)
                    {
                        Console.WriteLine($"  {folder}");
                    }
                }
                else
                {
                    Console.WriteLine($"No folders named '{folderName}' were found starting from '{startDirectory}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for folder '{folderName}': {ex.Message}");
            }
        }
    }
}

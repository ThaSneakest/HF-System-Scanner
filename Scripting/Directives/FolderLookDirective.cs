using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class FolderLookDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives file

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
                    if (line.StartsWith("FolderLook::"))
                    {
                        string folderPath = line.Substring("FolderLook::".Length).Trim();
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            Console.WriteLine($"FolderLook:: directive found. Analyzing folder: {folderPath}");
                            DisplayFolderContents(folderPath);
                        }
                        else
                        {
                            Console.WriteLine("FolderLook:: directive found, but no folder path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DisplayFolderContents(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder not found: {folderPath}");
                    return;
                }

                Console.WriteLine($"Contents of folder: {folderPath}");

                // Display subdirectories
                string[] subDirectories = Directory.GetDirectories(folderPath);
                Console.WriteLine("\nDirectories:");
                foreach (string directory in subDirectories)
                {
                    Console.WriteLine($"  {directory}");
                }

                // Display files
                string[] files = Directory.GetFiles(folderPath);
                Console.WriteLine("\nFiles:");
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    Console.WriteLine($"  {file} - Size: {fileInfo.Length} bytes, Created: {fileInfo.CreationTime}, Modified: {fileInfo.LastWriteTime}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to access folder {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing folder '{folderPath}': {ex.Message}");
            }
        }
    }
}

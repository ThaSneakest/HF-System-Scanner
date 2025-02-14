using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SymLinkDirective
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
                    if (line.StartsWith("ListSymLink::"))
                    {
                        string targetFolder = line.Substring("ListSymLink::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetFolder))
                        {
                            Console.WriteLine($"ListSymLink:: directive found. Listing symbolic links in: {targetFolder}");
                            ListSymbolicLinks(targetFolder);
                        }
                        else
                        {
                            Console.WriteLine("ListSymLink:: directive found, but no folder path specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ListSymbolicLinks(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder not found: {folderPath}");
                    return;
                }

                Console.WriteLine($"Symbolic links and junctions in folder: {folderPath}");

                var directoryInfo = new DirectoryInfo(folderPath);
                foreach (var entry in directoryInfo.GetFileSystemInfos())
                {
                    if ((entry.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        Console.WriteLine($"- {entry.Name} ({(entry is DirectoryInfo ? "Directory" : "File")})");
                    }
                }

                Console.WriteLine("Listing completed.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to list symbolic links in '{folderPath}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing symbolic links in '{folderPath}': {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DeleteJunctionsDirective
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetFileAttributes(string lpFileName);

        private const int FILE_ATTRIBUTE_REPARSE_POINT = 0x0400;

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
                    if (line.StartsWith("DeleteJunctions::"))
                    {
                        string targetDirectory = line.Substring("DeleteJunctions::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetDirectory))
                        {
                            Console.WriteLine($"DeleteJunctions:: directive found. Scanning directory: {targetDirectory}");
                            DeleteJunctionsInDirectory(targetDirectory);
                        }
                        else
                        {
                            Console.WriteLine("DeleteJunctions:: directive found, but no directory was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeleteJunctionsInDirectory(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine($"Directory not found: {directoryPath}");
                    return;
                }

                foreach (string subDirectory in Directory.GetDirectories(directoryPath))
                {
                    if (IsJunctionPoint(subDirectory))
                    {
                        try
                        {
                            Directory.Delete(subDirectory);
                            Console.WriteLine($"Junction deleted: {subDirectory}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting junction '{subDirectory}': {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Not a junction point: {subDirectory}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to scan or modify {directoryPath}. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning directory '{directoryPath}': {ex.Message}");
            }
        }

        private static bool IsJunctionPoint(string path)
        {
            try
            {
                int attributes = GetFileAttributes(path);
                return (attributes != -1) && ((attributes & FILE_ATTRIBUTE_REPARSE_POINT) != 0);
            }
            catch
            {
                return false;
            }
        }
    }
}

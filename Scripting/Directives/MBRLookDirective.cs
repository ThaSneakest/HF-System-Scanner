using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class MBRLookDirective
    {
        private const int BytesToRead = 512; // Standard MBR size (512 bytes)

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
                    if (line.StartsWith("MBRLook::"))
                    {
                        string outputFilePath = line.Substring("MBRLook::".Length).Trim();
                        if (!string.IsNullOrEmpty(outputFilePath))
                        {
                            Console.WriteLine($"MBRLook:: directive found. Saving MBR to: {outputFilePath}");
                            SaveMBR(outputFilePath);
                        }
                        else
                        {
                            Console.WriteLine("MBRLook:: directive found, but no output file path specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void SaveMBR(string outputFilePath)
        {
            try
            {
                using (FileStream fs = new FileStream(@"\\.\PhysicalDrive0", FileMode.Open, FileAccess.Read))
                {
                    byte[] mbrData = new byte[BytesToRead];
                    int bytesRead = fs.Read(mbrData, 0, BytesToRead);

                    if (bytesRead == BytesToRead)
                    {
                        File.WriteAllBytes(outputFilePath, mbrData);
                        Console.WriteLine($"Successfully saved MBR to: {outputFilePath}");
                    }
                    else
                    {
                        Console.WriteLine("Failed to read the complete MBR.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to read MBR. Run the application as administrator.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error while accessing MBR: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving MBR: {ex.Message}");
            }
        }
    }
}

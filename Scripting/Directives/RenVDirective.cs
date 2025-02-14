using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RenVDirective
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
                    if (line.StartsWith("RenV::"))
                    {
                        Console.WriteLine("RenV:: directive found. Renaming Vundo-infected files.");
                        RenameInfectedFiles();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void RenameInfectedFiles()
        {
            try
            {
                string[] filesToCheck = new string[]
                {
                    @"C:\Windows\System32\infectedFile1.exe",
                    @"C:\Windows\System32\drivers\infectedDriver.sys",
                    @"C:\Users\<username>\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\infectedStartupFile.lnk"
                    // Add other known infected files or directory paths
                };

                foreach (var file in filesToCheck)
                {
                    if (File.Exists(file))
                    {
                        string newFileName = file.Replace("infected", "safe"); // Customize as needed
                        Console.WriteLine($"Renaming {file} to {newFileName}");

                        try
                        {
                            File.Move(file, newFileName);
                            Console.WriteLine($"Successfully renamed {file} to {newFileName}");
                        }
                        catch (Exception renameEx)
                        {
                            Console.WriteLine($"Error renaming {file}: {renameEx.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"File not found: {file}");
                    }
                }

                Console.WriteLine("Renaming process completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during renaming process: {ex.Message}");
            }
        }
    }
}

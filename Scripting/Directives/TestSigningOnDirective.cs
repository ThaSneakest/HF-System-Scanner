using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class TestSigningOnDirective
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
                    if (line.StartsWith("TestSigningOn::"))
                    {
                        Console.WriteLine("TestSigningOn:: directive found. Enabling test signing mode.");
                        EnableTestSigning();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void EnableTestSigning()
        {
            try
            {
                Console.WriteLine("Attempting to enable test signing mode...");

                // Use bcdedit to enable test signing mode
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C bcdedit /set testsigning on",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Test signing mode enabled successfully.");
                        Console.WriteLine("Output:");
                        Console.WriteLine(output);
                    }
                    else
                    {
                        Console.WriteLine("Error enabling test signing mode:");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify boot configuration. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enabling test signing mode: {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class MBRDirective
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
                    if (line.StartsWith("MBR::"))
                    {
                        Console.WriteLine("MBR:: directive found. Fixing MBR infection.");
                        FixMBR();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void FixMBR()
        {
            try
            {
                Console.WriteLine("Attempting to fix the MBR...");

                // Use bootrec.exe to repair the MBR
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C bootrec /fixmbr",
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
                        Console.WriteLine("MBR repair completed successfully.");
                        Console.WriteLine("Output:");
                        Console.WriteLine(output);
                    }
                    else
                    {
                        Console.WriteLine("Error repairing MBR:");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify MBR. Run the application as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing MBR: {ex.Message}");
            }
        }
    }
}

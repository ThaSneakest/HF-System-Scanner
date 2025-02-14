using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class BatchDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "WildlandsFixScript.txt"; // Path to the directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                List<string> batchCommands = new List<string>();
                bool isBatchActive = false;

                foreach (string line in lines)
                {
                    if (line.StartsWith("StartBatch::"))
                    {
                        Console.WriteLine("StartBatch:: directive found. Beginning batch command collection.");
                        isBatchActive = true;
                        continue;
                    }

                    if (line.StartsWith("EndBatch::"))
                    {
                        Console.WriteLine("EndBatch:: directive found. Executing batch commands.");
                        ExecuteBatch(batchCommands);
                        batchCommands.Clear();
                        isBatchActive = false;
                        continue;
                    }

                    if (isBatchActive)
                    {
                        batchCommands.Add(line);
                    }
                }

                if (isBatchActive)
                {
                    Console.WriteLine("Warning: StartBatch:: directive without a matching EndBatch::. Batch script ignored.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ExecuteBatch(List<string> commands)
        {
            if (commands.Count == 0)
            {
                Console.WriteLine("No commands found in batch. Skipping execution.");
                return;
            }

            string batchFilePath = Path.Combine(Path.GetTempPath(), $"batch_{DateTime.Now:yyyyMMddHHmmss}.bat");

            try
            {
                File.WriteAllLines(batchFilePath, commands);
                Console.WriteLine($"Batch script written to: {batchFilePath}");

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{batchFilePath}\"",
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

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Batch Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Batch Error:");
                        Console.WriteLine(error);
                    }

                    Console.WriteLine($"Batch execution completed with exit code {process.ExitCode}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing batch script: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (File.Exists(batchFilePath))
                    {
                        File.Delete(batchFilePath);
                        Console.WriteLine($"Temporary batch file deleted: {batchFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting temporary batch file: {ex.Message}");
                }
            }
        }
    }
}

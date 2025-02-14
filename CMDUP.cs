using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class CMDUP
{
    /// <summary>
    /// Terminates processes whose names match specific patterns.
    /// </summary>
    public static void Execute()
    {
        // Define a compiled regex for performance and reuse
        var processPattern = new Regex(@"^(reg|cmd|conhost)\.exe$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                string processName = process.ProcessName;

                // Match the process name with the regex
                if (processPattern.IsMatch(processName))
                {
                    Console.WriteLine($"Terminating process: {processName} (ID: {process.Id})");

                    // Attempt to kill the process
                    process.Kill();

                    // Optionally wait for the process to exit
                    process.WaitForExit();
                }
            }
            catch (InvalidOperationException)
            {
                // Handle the case where the process has already exited
                Console.WriteLine($"Process {process.ProcessName} has already exited.");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Handle access denied errors (e.g., trying to kill system-critical processes)
                Console.WriteLine($"Access denied while trying to terminate process {process.ProcessName}.");
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions
                Console.WriteLine($"Error processing {process.ProcessName}: {ex.Message}");
            }
        }
    }
}
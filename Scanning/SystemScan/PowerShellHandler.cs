using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class PowerShellHandler
{
    public static void ExecutePowerShell(string fix)
    {
        try
        {
            // Prepare command
            string com = Regex.Replace(fix, @"(?i)powershell:\s*(.+)", "$1");
            com = Regex.Replace(com, @"^\s+|\s+$", "");

            // Log the command
            string logFile = "HFIXLOG.txt"; // Assuming this file will be logged
            File.AppendAllText(logFile, Environment.NewLine + "=========" + com + " =========" + Environment.NewLine + Environment.NewLine);

            // File paths
            string path1 = @"C:\FRST\pw000.txt";
            File.Delete(path1);

            // Placeholder for Timer
            int time = 3600000; // 1 hour in milliseconds
            TimerSetTimer(time);

            string scmd = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
            if (Regex.IsMatch(com, @"(?i)^""?.:\\.+\.ps1""?$"))
            {
                // Run PowerShell command with file path
                CommandHandler.RunCommand(scmd + " -ExecutionPolicy Bypass " + com + " -File > " + path1);
                if (!File.Exists(path1) || new FileInfo(path1).Length == 0)
                {
                    CommandHandler.RunCommand("Powershell -ExecutionPolicy Bypass " + com + " -File Powershell 2>&1 > " + path1);
                }
            }
            else
            {
                // Split the commands and write them to a temporary script
                if (!com.Contains(";"))
                {
                    com += ";";
                }
                string[] commands = com.Split(';');
                string path0 = @"C:\FRST\tmp.ps1";
                File.WriteAllLines(path0, commands);
                CommandHandler.RunCommand(scmd + " -ExecutionPolicy Bypass " + path0 + " -File > " + path1);
                if (!File.Exists(path1) || new FileInfo(path1).Length == 0)
                {
                    CommandHandler.RunCommand(scmd + " -ExecutionPolicy Bypass " + path0 + " -File Powershell 2>&1 > " + path1);
                }
                File.Delete(path0);
                string result = File.ReadAllText(path1);
                File.AppendAllText(logFile, result);
            }

            // Kill the timer
            TimerKillTimer();

            // Final cleanup and log end message
            File.Delete(path1);
            File.AppendAllText(logFile, Environment.NewLine + "=========" + "END" + " Powershell: =========" + Environment.NewLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecutePowerShell: {ex.Message}");
        }
    }

    // Simulate a TimerSetTimer method
    private static void TimerSetTimer(int time)
    {
        // Implement your timer logic here
        Console.WriteLine($"Timer set for {time} milliseconds.");
    }

    // Simulate a TimerKillTimer method
    private static void TimerKillTimer()
    {
        // Implement your kill timer logic here
        Console.WriteLine("Timer killed.");
    }

    public static void ExecutePowershell(ref int fileIndex)
    {
        try
        {
            // Log the beginning of the PowerShell script execution
            string logFile = "HFIXLOG.txt"; // Assuming this file will be logged
            File.AppendAllText(logFile, Environment.NewLine + "=========" + " Powershell: =========" + Environment.NewLine + Environment.NewLine);

            // Set up paths for temporary PowerShell script and result
            string path1 = @"C:\FRST\tmp000.ps1";
            string path2 = @"C:\FRST\pw000.txt";
            string scriptDirectory = @"C:\ScriptDir\"; // Assuming this is your script directory
            string fixList = @"C:\path\to\your\fixlist.txt"; // Path to your fixlist
            string cmd = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
            string batchCommand = "";

            // Create a temporary PowerShell script file
            using (StreamWriter writer = new StreamWriter(path1))
            {
                int batchIndex = fileIndex + 1;
                while (true)
                {
                    // Read the next line from the fix list
                    batchCommand = File.ReadLines(fixList).Skip(batchIndex - 1).FirstOrDefault();

                    if (string.IsNullOrEmpty(batchCommand) || batchCommand.Contains("endpowershell"))
                        break;

                    // Write the command to the temporary PowerShell script file
                    writer.WriteLine(batchCommand);
                    batchIndex++;
                }

                // Update the file index after reading
                fileIndex = batchIndex - 1;
            }

            // Run the PowerShell script
            CommandHandler.RunCommand($"{cmd} -ExecutionPolicy Bypass {path1} -File > {path2}");
            if (new FileInfo(path2).Length == 0)
            {
                CommandHandler.RunCommand($"{cmd} -ExecutionPolicy Bypass {path1} -File Powershell 2>&1 > {path2}");
            }

            // Delete the temporary PowerShell script after execution
            File.Delete(path1);

            // Read and log the output from the PowerShell script
            string result = File.ReadAllText(path2);
            File.AppendAllText(logFile, result + Environment.NewLine + "=========" + " " + "END" + " Powershell: =========" + Environment.NewLine + Environment.NewLine);

            // Delete the output file
            File.Delete(path2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecutePowershell: {ex.Message}");
        }
    }
}

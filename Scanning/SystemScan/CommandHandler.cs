using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;

public class CommandHandler
{
    private string chcp;
    string END = "END"; // Define the END constant
    string FIX = "Your command or input here"; // Replace with your actual FIX logic
    string OF = "Output File"; // Declare OF and give it a value
    private static string _cDrive = @"C:\";
    private const string SOFTWARE = @"\Software";

    public void CMD(ref int f, int batch1 = 0)
    {
        string a = "";
        string red = "";
        string com = "Batch:";

        if (batch1 == 0)
        {
            com = Regex.Replace(FIX, @"(?i)CMD:\s*(.+)", "$1");
            a = Environment.NewLine;
        }

        // Assuming HFIXLOG is the path to your log file
        //File.AppendAllText(Logger.WildlandsLogFile, Environment.NewLine + "=========" + com + "=========" + a + Environment.NewLine);

        // Timer-related code
        //int itime = _TIMER_SETTIMER(FORM1, 3600000, TIMEA);

        if (batch1 == 1)
        {
            CMDBATCH(ref f);
        }
        else
        {
            if (com.Contains(">"))
            {
                RunCommandAndWait(com);
            }
            else
            {
                if (Regex.IsMatch(com, @"(?i)reg (delete|add) ") && !com.Contains("/f"))
                {
                    com = Regex.Replace(com, @"(.+)", "$1 /f");
                }

                if (Regex.IsMatch(com, @"(?i)reg export ") && !com.Contains("/y"))
                {
                    com = Regex.Replace(com, @"(.+)", "$1 /y");
                }

                if (com.Contains("chkdsk"))
                {
                    string yes = CMDYES();
                    if (!string.IsNullOrEmpty(yes))
                    {
                        com = "ECHO " + yes + " | " + com;
                    }
                }

                if (string.IsNullOrEmpty(chcp))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Environment.GetEnvironmentVariable("ComSpec"),
                        Arguments = "/c chcp",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };

                    Process proc = Process.Start(startInfo);
                    string read1 = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    chcp = Regex.Replace(read1, @".+?(\d+)\s*", "$1");
                }

                string read = CMDRUN1(com, chcp);
                //File.AppendAllText(Logger.WildlandsLogFile, read + Environment.NewLine);
            }
        }

        //_TIMER_KILLTIMER(FORM1, itime);
        com = " Batch:";
        if (batch1 == 0)
        {
            com = " CMD:";
        }

        //File.AppendAllText(Logger.WildlandsLogFile, Environment.NewLine + "=========" + END + " " + OF + com + "=========" + Environment.NewLine + Environment.NewLine);
    }



    private void RunCommandAndWait(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("ComSpec"),
            Arguments = "/u /c " + command,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        Process.Start(startInfo).WaitForExit();
    }

    // Assuming _TIMER_SETTIMER and _TIMER_KILLTIMER are timer methods (for example, a System.Timers.Timer)
    private int _TIMER_SETTIMER(object form1, int time, object timeA)
    {
        // Implement timer logic here, returning a timer ID
        return 0; // Replace with actual implementation
    }

    private void _TIMER_KILLTIMER(object form1, int itime)
    {
        // Implement timer kill logic here
    }

    public string CMDYES()
    {
        string path = _cDrive + @"\FRST\logs\cmd" + Utility.GetRandomNumber(1000, 9999) + ".txt";

        // Create the file to simulate the FileOpen
        File.WriteAllText(path, string.Empty);

        string ret = CMDRUN("del /p " + path);
        if (!Regex.IsMatch(ret, @"\(\w/\w\)\?"))
        {
            return null;
        }

        string yes = Regex.Replace(ret, @".+\((\w)/\w\).+\v*", "$1");

        // Delete the file after usage
        File.Delete(path);

        return yes;
    }

    public string CMDRUN1(string com, string encode = "", string wdir = "")
    {
        string nchcp = "";

        if (Regex.IsMatch(encode, "866|852") || Regex.IsMatch("0419|0422|0423|0402|042F|0C1A|1C1A|281A|301A|0428|0450|082C|0843|201A|0415", GetCurrentUILang()))
        {
            nchcp = "chcp 65001 >NUL & ";
        }

        return CMDRUN(nchcp + com, wdir);
    }

    public string CMDRUN(string com, string wdir = "")
    {
        string readOutput = string.Empty;
        string readError = string.Empty;

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("ComSpec"), // Get the system command processor (e.g., cmd.exe)
            Arguments = "/u /c " + com,  // '/u' and '/c' are passed as in the original command
            WorkingDirectory = wdir,
            RedirectStandardOutput = true,  // Redirect standard output to capture it
            RedirectStandardError = true,   // Redirect error output to capture it
            UseShellExecute = false,       // Must be false to redirect output
            CreateNoWindow = true          // Do not show command window
        };

        using (Process proc = Process.Start(startInfo))
        {
            // Wait for the command to complete
            proc.WaitForExit();

            // Read output
            readOutput = proc.StandardOutput.ReadToEnd();

            // Read error
            readError = proc.StandardError.ReadToEnd();
        }

        // If there is output, process it
        if (!string.IsNullOrEmpty(readOutput))
        {
            return CMDRUN0(readOutput);
        }

        // If there is error output, process it
        if (!string.IsNullOrEmpty(readError))
        {
            return CMDRUN0(readError);
        }

        return string.Empty;
    }

    private static Random _random = new Random();

    public string CMDRUN0(string read)
    {
        // Generate a random number to append to the filename
        string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FRST\\logs\\cmd1" + _random.Next(1000, 9999) + ".txt");

        // Write the content to the file
        File.WriteAllText(path1, read);

        // Read the content from the file
        string fileContent = File.ReadAllText(path1);

        // Delete the file after reading
        File.Delete(path1);

        return fileContent;
    }

    // Method to get the current system UI language
    public string GetCurrentUILang()
    {
        // Assuming we use the CurrentCulture's name for simplicity
        // This can be adjusted based on your requirements
        return System.Globalization.CultureInfo.CurrentCulture.Name;
    }

    private static string _chcp;

    public void CMDBATCH(ref int f)
    {
        string wdir = "";

        if (string.IsNullOrEmpty(_chcp))
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c chcp",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string read1 = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            _chcp = Regex.Replace(read1, @".+(\d+)\s*", "$1");
        }

        int b = f + 1;
        string filePath = "path_to_your_file.txt"; // Replace with the actual file path

        while (true)
        {
            // Pass both the file path and line number to the method
            string com = FileUtils.ReadLineFromFile(filePath, b);
            if (com == null) break;

            if (string.IsNullOrEmpty(com) || com.IndexOf("echo off", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                b++;
                continue;
            }

            // Process the line if it's not empty or "echo off"
            Console.WriteLine($"Line {b}: {com}");
            b++;


        if (com.Contains("endbatch")) break;

            if (Regex.IsMatch(com, @"^(?i)(CD|CHDIR) "))
            {
                wdir = Regex.Replace(com, @"^(?i)(CD|CHDIR) (|/D )\s*(.+)\s*", "$3");
                wdir = wdir.Trim('"');
                if (wdir == @"CD\") wdir = _cDrive;
                if (wdir.StartsWith("Windows", StringComparison.OrdinalIgnoreCase)) wdir = _cDrive + wdir;
                if (wdir.Contains("%")) wdir = Utility.ExpandEnvironmentVariables(wdir);
                b++;
                continue;
            }

            if (Regex.IsMatch(com, @"^(?i)PUSHD"))
            {
                wdir = Regex.Replace(com, @"^(?i)PUSHD\s*(.+)\s*", "$1");
                wdir = wdir.Trim('"');
                if (wdir == @"CD\") wdir = _cDrive;
                if (wdir.StartsWith("Windows", StringComparison.OrdinalIgnoreCase)) wdir = _cDrive + wdir;
                if (wdir.Contains("%")) wdir = Utility.ExpandEnvironmentVariables(wdir);
                b++;
                continue;
            }

            if (Regex.IsMatch(com, @"^(?i)POPD"))
            {
                wdir = Utility.GetScriptDir();
                b++;
                continue;
            }

            com = Regex.Replace(com, @"%%", "%");

            if (Regex.IsMatch(com, @"(?i)reg (delete|add) ") && !com.Contains("/f"))
            {
                com = Regex.Replace(com, @"(.+)", "$1 /f");
            }

            if (Regex.IsMatch(com, @"(?i)reg export ") && !com.Contains("/y"))
            {
                com = Regex.Replace(com, @"(.+)", "$1 /y");
            }

            if (com.Contains("chkdsk") && !com.Contains("echo "))
            {
                var yes = CMDYES();
                if (yes == "Y")  // Check if the response is "Y"
                {
                    com = "ECHO " + yes + " | " + com;
                }
            }


            string read = CMDRUN1(com, _chcp, wdir);
            //Logger.Instance.LogToFile(read);
            //Logger.Instance.LogToFile(Environment.NewLine);
            b++;
        }

        f = b;
    }

    public static void CommandProc(string hive, ref List<string> arrayReg, string upd1)
    {
        string key = hive + SOFTWARE + @"\Microsoft\Command Processor";
        // Simulate GUICtrlSetData for debugging or logging purposes.
        Console.WriteLine($"Processing key: {key}");

        try
        {
            // Check if the registry key exists.
            object valData = Registry.GetValue(key, "AutoRun", null);
            if (valData == null)
            {
                RegistryUtilsScript.SetRegistryAccess(key, true);
            }
            else
            {
                arrayReg.Add($"{hive}...\\Command Processor: {valData} <==== {upd1}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing the registry: {ex.Message}");
        }
    }

    public static void CommandProcFix(string fix)
    {
        if (fix.IndexOf("HKLM\\", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            RegistryValueHandler.DeleteRegistryValue(@"HKEY_LOCAL_MACHINE" + SOFTWARE + @"\Microsoft\Command Processor", "AutoRun");
        }

        if (fix.IndexOf("HKU\\", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return;
        }

        string user = System.Text.RegularExpressions.Regex.Replace(
            fix,
            @"(?i)HKU\\(.+?)\\.+",
            "$1",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        RegistryUtils.RELOAD(user);

        string key = $@"HKEY_USERS\{user}\Software\Microsoft\Command Processor";
        RegistryValueHandler.DeleteRegistryValue(key, "AutoRun");

        RegistryUtils.REUNLOAD(user);
    }
    public static void RunCommand(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running command: {command}. {ex.Message}");
        }
    }
    private void ProcessTaskContent(List<string> taskLog, string subKeyName, string taskPath, string taskContent)
    {
        if (taskContent.Contains("<Command>"))
        {
            var commands = ExtractCommands(taskContent);
            foreach (string command in commands)
            {
                taskLog.Add($"Task: {subKeyName} - {taskPath} => Command: {command}");
            }
        }
        else
        {
            taskLog.Add($"Task: {subKeyName} - {taskPath} - No valid command found.");
        }
    }

    private IEnumerable<string> ExtractCommands(string taskContent)
    {
        var commands = new List<string>();
        int commandStartIndex = taskContent.IndexOf("<Command>");
        while (commandStartIndex >= 0)
        {
            int commandEndIndex = taskContent.IndexOf("</Command>", commandStartIndex);
            if (commandEndIndex > commandStartIndex)
            {
                string command = taskContent.Substring(commandStartIndex + "<Command>".Length, commandEndIndex - commandStartIndex - "<Command>".Length);
                commands.Add(command.Trim());
            }
            commandStartIndex = taskContent.IndexOf("<Command>", commandEndIndex);
        }
        return commands;
    }

    public static string RunCommandString(string command)
    {
        try
        {
            // Create a process to execute the command
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // Combine standard output and error (if any)
            return string.IsNullOrEmpty(error) ? output : $"{output}\nError: {error}";
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }

    /// <summary>
    /// Executes a command using cmd.exe and waits for the process to complete.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public static void RunWait(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command cannot be null or empty.", nameof(command));
        }

        try
        {
            using (var process = new Process())
            {
                // Configure process start info
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true; // Capture standard output
                process.StartInfo.RedirectStandardError = true;  // Capture standard error
                process.StartInfo.CreateNoWindow = true;

                // Start the process
                process.Start();

                // Read the standard output and error while waiting for the process to exit
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit(); // Wait for the process to complete

                // Log the output and error (optional, can be replaced with actual logging)
                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine("Output:\n" + output);
                }

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error:\n" + error);
                }

                // Check for exit code to determine success or failure
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}:\n{error}");
                }
            }
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception for the caller to handle
            Console.WriteLine($"Error executing command: {command}\nException: {ex.Message}");
            throw;
        }


    }

    /// <summary>
    /// Executes a bcdedit command and returns the output.
    /// </summary>
    /// <param name="arguments">The arguments to pass to bcdedit.exe.</param>
    /// <returns>The output of the command.</returns>
    public static string ExecuteBCDEditCommand(string arguments)
    {
        try
        {
            // Configure the process to execute bcdedit.exe
            using (var process = new Process())
            {
                process.StartInfo.FileName = "bcdedit.exe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException($"Error executing bcdedit command: {error}");
                }

                return output;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to execute bcdedit command with arguments '{arguments}'.", ex);
        }
    }
}

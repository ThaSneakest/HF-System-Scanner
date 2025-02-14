using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class TaskHandler
{
    private static bool IsWhiteListEnabled = false; // Toggle for whitelist feature

    /// <summary>
    /// Parses arguments from XML-like content.
    /// </summary>
    /// <param name="argumentContent">The XML-like string containing arguments.</param>
    /// <returns>A processed argument string.</returns>
    public static string ParseArguments(string argumentContent)
    {
        // Extract the <Arguments> tag content
        Match match = Regex.Match(argumentContent, "(?is)<Arguments>(.+?)</Arguments>");
        if (!match.Success)
            return string.Empty;

        string command = match.Groups[1].Value;

        // Check for <WorkingDirectory> following <Arguments>
        Match workingDirMatch = Regex.Match(argumentContent, @"(?is)</Arguments>\\R+\s*<WorkingDirectory>(.+?)</WorkingDirectory>");

        if (workingDirMatch.Success)
        {
            string workingDirectory = workingDirMatch.Groups[1].Value;
            return Utility.ConvertPath(Path.Combine(workingDirectory, command));
        }

        return Utility.ConvertPath(command);
    }

    /// <summary>
    /// Processes `.job` files in the specified directory and logs their content.
    /// </summary>
    public static void ProcessJobFiles()
    {
        List<string> logEntries = new List<string>();
        string taskDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Tasks");
        string[] jobFiles = Directory.GetFiles(taskDir, "*.job", SearchOption.TopDirectoryOnly);

        foreach (var jobFile in jobFiles)
        {
            string taskName = Path.GetFileNameWithoutExtension(jobFile);
            string logEntry = $"Task: {taskName} => ";

            string content = FileUtils.ReadFileContent(jobFile);
            if (string.IsNullOrEmpty(content))
            {
                logEntry += "[Unable to read content]";
                logEntries.Add(logEntry);
                continue;
            }

            string filePath = ExtractFilePath(content);
            string attentionMarker = GetAttentionMarker(taskName, filePath);
            logEntry += $"{filePath} {attentionMarker}";
            logEntries.Add(logEntry);
        }

        //Logger.WriteLogEntries(logEntries);
    }

    private static string ExtractFilePath(string content)
    {
        string pathPattern = "(?is).*?(.:\\\\.+)";
        Match match = Regex.Match(content, pathPattern);
        return match.Success ? match.Groups[1].Value : "[Invalid Path]";
    }

    private static string GetAttentionMarker(string taskName, string filePath)
    {
        // Example logic for identifying attention markers
        if (Regex.IsMatch(taskName, "\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}") && filePath.Contains("updtask.exe"))
            return "<==== ATTENTION";

        if (Regex.IsMatch(filePath, @"(?i)\\Program Files\\[^\\]+\\.exe"))
            return "<==== ATTENTION";

        return string.Empty;
    }

    public static void AnalyzeRegistryKey(string registryKey)
    {
        Console.WriteLine($"Scanning tasks from registry key: {registryKey}");
        RegistryKey regKey = Registry.LocalMachine.OpenSubKey(registryKey, writable: false);
        if (regKey == null)
        {
            Console.WriteLine("Error: Unable to access the registry key.");
            return;
        }

        try
        {
            List<string> taskLogs = new List<string>();
            foreach (string subKeyName in regKey.GetSubKeyNames())
            {
                RegistryKey subKey = regKey.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                string taskPath = subKey.GetValue("Path") as string;
                if (!string.IsNullOrEmpty(taskPath))
                {
                    string taskName = Path.GetFileName(taskPath);
                    string taskFullPath = Path.Combine(Environment.SystemDirectory, "Tasks", taskPath);

                    if (File.Exists(taskFullPath))
                    {
                        string taskContent = File.ReadAllText(taskFullPath);
                        if (!string.IsNullOrEmpty(taskContent))
                        {
                            AnalyzeTaskContent(taskName, taskFullPath, taskContent, taskLogs);
                        }
                    }
                    else
                    {
                        taskLogs.Add($"Task: {subKeyName} - {taskPath} does not exist.");
                    }
                }
            }

           // Logger.WriteLogs(taskLogs);
        }
        finally
        {
            regKey.Close();
        }
    }

    private static void AnalyzeTaskContent(string taskName, string taskPath, string taskContent, List<string> logs)
    {
        string commandPattern = "<Command>(.+?)</Command>";
        string[] commandMatches = ExtractPattern(taskContent, commandPattern);
        if (commandMatches.Length > 0)
        {
            foreach (var command in commandMatches)
            {
                string sanitizedCommand = command.Replace("http:", "hxxp:").Replace("https:", "hxxps:");
                logs.Add($"Task: {taskName} - Command: {sanitizedCommand}");
            }
        }
        else
        {
            logs.Add($"Task: {taskName} - No valid commands found in {taskPath}");
        }
    }

    private static string[] ExtractPattern(string input, string pattern)
    {
        System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(input, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        List<string> results = new List<string>();
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                results.Add(match.Groups[1].Value);
            }
        }
        return results.ToArray();
    }

}
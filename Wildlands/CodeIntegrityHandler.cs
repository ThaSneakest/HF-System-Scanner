using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class CodeIntegrityHandler
{
    public static void Execute()
    {
        string tempDir = Path.GetTempPath();
        string uniqueFileName = $"codeint{new Random().Next(1000, 9999)}";
        string eventFilePath = Path.Combine(tempDir, uniqueFileName);
        string resultPath = Path.Combine(tempDir, "codeint2");

        try
        {
            // Generate the command to extract Code Integrity events
            string cmd = $"/c wevtutil qe \"Microsoft-Windows-CodeIntegrity/Operational\" \"/q:*[System [(Level=2)]]\" /c:12 /rd:true /uni:true /f:text >> \"{eventFilePath}\"";

            // Execute the command
            CommandHandler.RunCommand(cmd);

            // Process the generated file
            if (!File.Exists(eventFilePath)) return;

            string eventData = File.ReadAllText(eventFilePath);
            if (string.IsNullOrWhiteSpace(eventData) || !eventData.Contains(":"))
            {
                File.Delete(eventFilePath);
                LogNoViolations(resultPath);
                return;
            }

            File.Delete(eventFilePath);
            eventData = CleanEventData(eventData);

            // Extract and process relevant matches
            MatchCollection matches = Regex.Matches(eventData, @"(?s)Date:[\s\d:-]+\r?\nDescription:\s*.+?\r?\n");
            if (matches.Count < 1)
            {
                LogNoViolations(resultPath);
                return;
            }

            WriteMatchesToFile(matches, resultPath);

            // Example: Append the processed data to another location
            string finalData = File.ReadAllText(resultPath);
            // Replace $HADDITION with the actual path where you want to append the data
            Logger.Instance.LogPrimary(finalData);

            File.Delete(resultPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a message indicating no Code Integrity violations were detected.
    /// </summary>
    private static void LogNoViolations(string resultPath)
    {
        string message = "No Code Integrity Violations Detected.";
        Console.WriteLine(message);

        using (StreamWriter writer = new StreamWriter(resultPath, false))
        {
            writer.WriteLine("\r\nCodeIntegrity:");
            writer.WriteLine("===============");
            writer.WriteLine(message);
            Logger.Instance.LogPrimary(message);
        }
    }

    /// <summary>
    /// Cleans the raw event data using regular expressions.
    /// </summary>
    private static string CleanEventData(string data)
    {
        data = Regex.Replace(data, @"(?m)^\s*", ""); // Remove leading whitespace
        data = Regex.Replace(data, @"(?m)^(Event\[\d\]|Log Name|Event ID|Task|Level|Opcode|Keyword|Source|User|User Name|Computer):?.*\v{2}", ""); // Remove irrelevant sections
        data = Regex.Replace(data, @"(?m)^(Date:.+\d)T(\d.+\v{2})", "\r\n$1 $2"); // Fix date format
        data = Regex.Replace(data, @"(?m)^(Date:.+?)\.\d+Z", "$1"); // Remove milliseconds and Z
        data = Regex.Replace(data, @"(?s)Date:[\s\d:-]+\r?\nDescription:\s*\r?\nN/A\r?\n", ""); // Remove "N/A" descriptions
        return data;
    }

    /// <summary>
    /// Writes unique matches to the result file, avoiding duplicate entries.
    /// </summary>
    private static void WriteMatchesToFile(MatchCollection matches, string resultPath)
    {
        // Write header to the result file
        using (StreamWriter writer = new StreamWriter(resultPath, false))
        {
            writer.WriteLine("\r\nCodeIntegrity:");
            writer.WriteLine("===============");
        }

        foreach (Match match in matches)
        {
            string description = Regex.Replace(match.Value, @"(?s).+Description:\s*(.+)", "$1");

            // Check for duplicates before writing
            string existingData = File.Exists(resultPath) ? File.ReadAllText(resultPath) : string.Empty;
            if (!existingData.Contains(description))
            {
                using (StreamWriter writer = new StreamWriter(resultPath, true))
                {
                    writer.WriteLine(match.Value);
                    Logger.Instance.LogPrimary(match.Value);
                }
            }
        }
    }
}

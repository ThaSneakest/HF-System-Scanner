using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class SystemEventLogHandler
{
    public static void MainSys(string logFilePath, string labelText)
    {
        int count = 0;

        // Open the System event log
        using (EventLog eventLog = new EventLog("System"))
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
            {
                // Write header
                writer.WriteLine("\nSystem Events:");
                writer.WriteLine("=============");

                // Iterate through event log entries in reverse order
                for (int i = eventLog.Entries.Count - 1; i >= 0; i--)
                {
                    EventLogEntry entry = eventLog.Entries[i];

                    // Update GUI label text (if applicable)
                    Console.WriteLine($"{labelText} System Event: {i + 1}");

                    // Only process error events (event type 1)
                    if (entry.EntryType == EventLogEntryType.Error)
                    {
                        string description = entry.Message;

                        // Process error codes or special patterns in the description
                        string errorCode = ExtractErrorCode(description);
                        if (!string.IsNullOrEmpty(errorCode))
                        {
                            string errorMessage = GetErrorMessage(errorCode);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                description = description.Replace(errorCode, $"{errorCode} = {errorMessage}");
                            }
                        }

                        // Write the event details to the log file
                        writer.WriteLine($"{entry.TimeGenerated}: (Category: {entry.Category}, Instance ID: {entry.InstanceId})");
                        writer.WriteLine($"Source: {entry.Source}");
                        writer.WriteLine($"EventID: {entry.InstanceId}");
                        writer.WriteLine($"User: {entry.UserName ?? "N/A"}");
                        writer.WriteLine($"Description: {description.Replace(Environment.NewLine, "")}\n");

                        count++;
                        if (count == 8)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private static string ExtractErrorCode(string description)
    {
        var match = System.Text.RegularExpressions.Regex.Match(description, @"%%-?\d+| -\d{10}\b|0x[\da-fA-F]+:");
        return match.Success ? match.Value.Trim('%', ' ', ':') : null;
    }

    private static string GetErrorMessage(string errorCode)
    {
        try
        {
            int errorCodeValue = int.Parse(errorCode);
            return new System.ComponentModel.Win32Exception(errorCodeValue).Message;
        }
        catch
        {
            return null;
        }
    }

    public static List<string> ListEventLogErrors()
    {
        List<string> eventLogErrors = new List<string>();

        // Define the log name and source
        string logName = "Application"; // You can also use "System" or other logs

        // Create an EventLog instance
        using (EventLog eventLog = new EventLog(logName))
        {
            // Get the total number of entries
            int totalEntries = eventLog.Entries.Count;

            // Process only the last 10 entries
            int entriesToProcess = Math.Min(10, totalEntries);
            for (int i = totalEntries - entriesToProcess; i < totalEntries; i++)
            {
                EventLogEntry entry = eventLog.Entries[i];

                // Check if the entry type is Error
                if (entry.EntryType == EventLogEntryType.Error)
                {
                    string logMessage = $"Source: {entry.Source}, Time: {entry.TimeGenerated}, Message: {entry.Message}";
                    eventLogErrors.Add(logMessage);
                }
            }
        }

        return eventLogErrors;
    }
}

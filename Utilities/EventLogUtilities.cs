using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;

public static class EventLogUtilities
{
  

    public static bool CloseEventLog(IntPtr eventLogHandle)
    {
        bool result = Advapi32NativeMethods.CloseEventLogNative(eventLogHandle);
        if (!result)
        {
            throw new InvalidOperationException("Failed to close event log.");
        }
        return result;
    }

    public static ushort DecodeCategory(Structs.EVENTLOGRECORD eventLogRecord)
    {
        return eventLogRecord.EventCategory;
    }

    public static string DecodeComputer(Structs.EVENTLOGRECORD eventLogRecord, IntPtr eventLogPointer)
    {
        int offset = Marshal.SizeOf<Structs.EVENTLOGRECORD>() + 2 * (DecodeSource(eventLogRecord, eventLogPointer).Length + 1);
        int length = (int)eventLogRecord.UserSidOffset - offset - 1;
        IntPtr computerNamePtr = IntPtr.Add(eventLogPointer, offset);

        return Marshal.PtrToStringUni(computerNamePtr, length);
    }

    public static byte[] DecodeData(Structs.EVENTLOGRECORD eventLogRecord, IntPtr eventLogPointer)
    {
        int offset = (int)eventLogRecord.DataOffset;
        int length = (int)eventLogRecord.DataLength;
        IntPtr dataPtr = IntPtr.Add(eventLogPointer, offset);

        byte[] data = new byte[length];
        Marshal.Copy(dataPtr, data, 0, length);

        return data;
    }

    public static string DecodeDate(uint eventTime)
    {
        long fileTimeTicks = ((long)eventTime * 10000000) + 116444736000000000;
        Structs.FILETIMEALT fileTime = new Structs.FILETIMEALT
        {
            LowDateTime = (uint)(fileTimeTicks & 0xFFFFFFFF),
            HighDateTime = (uint)(fileTimeTicks >> 32)
        };

        Structs.FILETIMEALT localFileTime = FileTimeToLocalFileTime(fileTime);
        Structs.SYSTEMTIME systemTime = FileTimeToSystemTime(localFileTime);

        return $"{systemTime.Month:D2}/{systemTime.Day:D2}/{systemTime.Year:D4}";
    }

    private static Structs.FILETIMEALT FileTimeToLocalFileTime(Structs.FILETIMEALT fileTime)
    {
        Structs.FILETIMEALT localFileTime;
        if (!Kernel32NativeMethods.FileTimeToLocalFileTimeNative(ref fileTime, out localFileTime))
        {
            throw new InvalidOperationException("Failed to convert FILETIME to local FILETIME.");
        }
        return localFileTime;
    }

    private static Structs.SYSTEMTIME FileTimeToSystemTime(Structs.FILETIMEALT fileTime)
    {
        Structs.SYSTEMTIME systemTime = new Structs.SYSTEMTIME();
        if (!Kernel32NativeMethods.FileTimeToSystemTimeNative(ref fileTime, ref systemTime))
        {
            throw new InvalidOperationException("Failed to convert FILETIME to SYSTEMTIME.");
        }
        return systemTime;
    }

    public static string DecodeDescription(Structs.EVENTLOGRECORD eventLogRecord, IntPtr eventLogPointer)
    {
        var strings = DecodeStrings(eventLogRecord, eventLogPointer);
        var source = DecodeSource(eventLogRecord, eventLogPointer);
        var eventId = eventLogRecord.EventID & 0x7FFF;
        var keyPath = RegistryConstants.EventLogKeyPath + source;

        string providerGuid = Registry.GetValue($"HKEY_LOCAL_MACHINE\\{keyPath}", "providerGuid", null) as string;
        string messageDll = Registry.GetValue($"HKEY_LOCAL_MACHINE\\{keyPath}", "EventMessageFile", null) as string;
        string parameterDll = Registry.GetValue($"HKEY_LOCAL_MACHINE\\{keyPath}", "ParameterMessageFile", null) as string;

        if (string.IsNullOrEmpty(messageDll) && !string.IsNullOrEmpty(providerGuid))
        {
            messageDll = Registry.GetValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Publishers\\{providerGuid}", "MessageFileName", null) as string ??
                         Registry.GetValue($"HKEY_CLASSES_ROOT\\CLSID\\{providerGuid}", "MessageFileName", null) as string;
        }

        var messageDllPaths = messageDll?.Split(';');
        var parameterDllPaths = parameterDll?.Split(';');

        if (messageDllPaths == null)
        {
            return $"Event-ID {eventId}";
        }

        string description = "";

        foreach (var dllPath in messageDllPaths)
        {
            IntPtr dllHandle = Kernel32NativeMethods.LoadLibraryEx(dllPath, IntPtr.Zero, 2);
            if (dllHandle == IntPtr.Zero) continue;

            uint flags = NativeMethodConstants.FORMAT_MESSAGE_IGNORE_INSERTS | NativeMethodConstants.FORMAT_MESSAGE_FROM_HMODULE;
            var buffer = new char[4096];
            uint bufferSize = (uint)buffer.Length; // Explicit cast to uint

            if (Kernel32NativeMethods.FormatMessage(flags, dllHandle, (uint)eventId, 0, buffer, bufferSize, IntPtr.Zero) > 0)
            {
                description += new string(buffer).TrimEnd('\0');
            }

            Kernel32NativeMethods.FreeLibrary(dllHandle);
        }

        if (string.IsNullOrEmpty(description))
        {
            foreach (var str in strings)
            {
                description += str;
            }
        }
        else
        {
            for (int i = strings.Length - 1; i >= 0; i--)
            {
                if (strings[i].StartsWith("%%"))
                {
                    var errorNumber = int.Parse(strings[i].Substring(2));
                    foreach (var paramDll in parameterDllPaths)
                    {
                        IntPtr paramDllHandle = Kernel32NativeMethods.LoadLibraryEx(paramDll, IntPtr.Zero, 2);
                        if (paramDllHandle == IntPtr.Zero) continue;

                        var paramBuffer = new char[4096];
                        if (Kernel32NativeMethods.FormatMessage(2048 | 512, paramDllHandle, (uint)errorNumber, 0, paramBuffer, (uint)paramBuffer.Length, IntPtr.Zero) > 0)
                        {
                            strings[i] = new string(paramBuffer).TrimEnd('\0');
                            break;
                        }


                        Kernel32NativeMethods.FreeLibrary(paramDllHandle);
                    }
                }

                description = description.Replace($"%{i}", strings[i]);
            }

            description = description.Replace("%%", "____").Replace("____", "%%");
        }

        return description.Trim();
    }

    public static int DecodeEventID(Structs.EVENTLOGRECORD eventLogRecord)
    {
        return (int)(eventLogRecord.EventID & 0x7FFF);
    }


    public static string DecodeSource(Structs.EVENTLOGRECORD eventLogRecord, IntPtr eventLogPointer)
    {
        int offset = Marshal.SizeOf<Structs.EVENTLOGRECORD>();
        IntPtr sourcePtr = IntPtr.Add(eventLogPointer, offset);
        return Marshal.PtrToStringUni(sourcePtr);
    }

    public static string[] DecodeStrings(Structs.EVENTLOGRECORD eventLogRecord, IntPtr eventLogPointer)
    {
        int numStrings = eventLogRecord.NumStrings;
        int offset = (int)eventLogRecord.StringOffset;

        List<string> strings = new List<string>();
        IntPtr stringPtr = IntPtr.Add(eventLogPointer, offset);

        for (int i = 0; i < numStrings; i++)
        {
            string str = Marshal.PtrToStringUni(stringPtr);
            strings.Add(str);

            offset += (str.Length + 1) * 2;
            stringPtr = IntPtr.Add(eventLogPointer, offset);
        }

        return strings.ToArray();
    }


    private static string _globalSourceName;

    public static string DecodeEventTime(long eventTime)
    {
        long fileTime = (eventTime * NativeMethodConstants.TicksMultiplier) + NativeMethodConstants.EpochOffset;

        // Convert FILETIME to DateTime
        DateTime dateTime = DateTime.FromFileTimeUtc(fileTime).ToLocalTime();

        int hours = dateTime.Hour;
        int minutes = dateTime.Minute;
        int seconds = dateTime.Second;
        string amPm = "AM";

        if (hours >= 12)
        {
            amPm = "PM";
            if (hours > 12)
            {
                hours -= 12;
            }
        }
        else if (hours == 0)
        {
            hours = 12;
        }

        return $"{hours:D2}:{minutes:D2}:{seconds:D2} {amPm}";
    }

    public static string DecodeEventTypeStr(int eventType)
    {
        switch (eventType)
        {
            case 0:
                return "Success";
            case 1:
                return "Error";
            case 2:
                return "Warning";
            case 4:
                return "Information";
            case 8:
                return "Success audit";
            case 16:
                return "Failure audit";
            default:
                return eventType.ToString();
        }
    }


    public static string DecodeUserName(IntPtr eventLogPtr, Structs.EVENTLOGRECORD eventLogRecord)
    {
        if (eventLogRecord.UserSidLength == 0)
        {
            return string.Empty;
        }

        IntPtr sidPtr = IntPtr.Add(eventLogPtr, (int)eventLogRecord.UserSidOffset);
        var accountInfo = SecurityLookupAccountSid(sidPtr);

        return accountInfo?.AccountName ?? string.Empty;
    }

    public static IntPtr OpenEventLog(string serverName, string sourceName)
    {
        _globalSourceName = sourceName;

        IntPtr eventLogHandle = Advapi32NativeMethods.OpenEventLogW(serverName, sourceName);
        if (eventLogHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to open event log. Last Error: {Marshal.GetLastWin32Error()}");
        }

        return eventLogHandle;
    }

    private static SecurityLookupResult? SecurityLookupAccountSid(IntPtr sid)
    {
        // Placeholder for actual implementation of LookupAccountSid
        // Implement this function to retrieve account name from SID
        return null;
    }

    public struct SecurityLookupResult
    {
        public string AccountName;
    }
    public static void MainApp(string logFilePath, string labelText)
    {
        int count = 0;

        // Open the Application event log
        using (EventLog eventLog = new EventLog("Application"))
        {
            // Get the entries in the event log
            EventLogEntryCollection eventEntries = eventLog.Entries;

            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
            {
                // Write header
                writer.WriteLine("\n==================== Events: ========================");
                writer.WriteLine("\nApplication Events:");
                writer.WriteLine("==================");

                // Iterate through event log entries
                for (int i = eventEntries.Count - 1; i >= 0; i--)
                {
                    EventLogEntry entry = eventEntries[i];

                    // Update GUI label text (if applicable)
                    Console.WriteLine($"{labelText} Application Event: {i + 1}");

                    // Only process error events (event type 1)
                    if (entry.EntryType == EventLogEntryType.Error)
                    {
                        string description = entry.Message.Replace(Environment.NewLine, "");
                        writer.WriteLine($"{entry.TimeGenerated}: (Category: {entry.Category}, Instance ID: {entry.InstanceId})");
                        writer.WriteLine($"Source: {entry.Source}");
                        writer.WriteLine($"EventID: {entry.InstanceId}");
                        writer.WriteLine($"User: {entry.UserName ?? "N/A"}");
                        writer.WriteLine($"Description: {description}\n");

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


    public static object[] ReadEventLog(IntPtr hEventLog, bool readSequentially = true, bool forwardDirection = true, uint recordOffset = 0)
    {


        uint readFlags = readSequentially ? NativeMethodConstants.EVENTLOG_SEQUENTIAL_READ : NativeMethodConstants.EVENTLOG_SEEK_READ;
        readFlags |= forwardDirection ? NativeMethodConstants.EVENTLOG_FORWARDS_READ : NativeMethodConstants.EVENTLOG_BACKWARDS_READ;

        uint bytesRead = 0;
        uint minBytesNeeded = 0;

        // Initial call to determine buffer size
        IntPtr buffer = Marshal.AllocHGlobal(1); // Dummy buffer
        bool result = Advapi32NativeMethods.ReadEventLogW(hEventLog, readFlags, recordOffset, buffer, 0, out bytesRead, out minBytesNeeded);
        Marshal.FreeHGlobal(buffer);

        if (!result && Marshal.GetLastWin32Error() != 122) // ERROR_INSUFFICIENT_BUFFER
        {
            return new object[] { false }; // Return failure
        }

        // Allocate buffer with required size
        buffer = Marshal.AllocHGlobal((int)minBytesNeeded);
        result = Advapi32NativeMethods.ReadEventLogW(hEventLog, readFlags, recordOffset, buffer, minBytesNeeded, out bytesRead, out _);

        if (!result)
        {
            Marshal.FreeHGlobal(buffer);
            return new object[] { false }; // Return failure
        }

        // Parse EVENTLOGRECORD
        Structs.EVENTLOGRECORD eventLogRecord = Marshal.PtrToStructure<Structs.EVENTLOGRECORD>(buffer);
        IntPtr eventLogPointer = buffer;

        // Decode event details
        var eventDetails = new object[15];
        eventDetails[0] = true; // Success
        eventDetails[1] = eventLogRecord.RecordNumber;
        eventDetails[2] = DecodeDate(eventLogRecord.TimeGenerated);
        eventDetails[3] = DecodeTime(eventLogRecord.TimeGenerated);
        eventDetails[4] = DecodeDate(eventLogRecord.TimeWritten);
        eventDetails[5] = DecodeTime(eventLogRecord.TimeWritten);
        eventDetails[6] = DecodeEventID(eventLogRecord);
        eventDetails[7] = eventLogRecord.EventType;
        eventDetails[8] = DecodeEventTypeStr(eventLogRecord.EventType);
        eventDetails[9] = eventLogRecord.EventCategory;
        eventDetails[10] = DecodeSource(eventLogRecord, eventLogPointer);
        eventDetails[11] = DecodeComputer(eventLogRecord, eventLogPointer);
        eventDetails[12] = DecodeUsername(eventLogRecord, eventLogPointer);
        eventDetails[13] = DecodeDescription(eventLogRecord, eventLogPointer);
        eventDetails[14] = DecodeData(eventLogRecord, eventLogPointer);

        Marshal.FreeHGlobal(buffer);
        return eventDetails;
    }

    private static string DecodeTime(uint time) => DateTimeOffset.FromUnixTimeSeconds(time).DateTime.ToLongTimeString();
    private static string DecodeEventTypeStr(ushort eventType)
    {
        switch (eventType)
        {
            case 0: return "Success";
            case 1: return "Error";
            case 2: return "Warning";
            case 4: return "Information";
            case 8: return "Success Audit";
            case 16: return "Failure Audit";
            default: return eventType.ToString();
        }
    }
    private static string DecodeUsername(Structs.EVENTLOGRECORD record, IntPtr pointer)
    {
        // Implement as per specific details of record structure
        return string.Empty;
    }

    private static void Events()
    {
        // Update the label with scanning message
        UpdateLabel($"{StringConstants.SCANB} events...");

        // Delete the temporary event file
        Kernel32NativeMethods.DeleteFile(Path.Combine(Path.GetTempPath(), "event"));

        // If the operating system version is greater than 5.2
        if (IsOsVersionGreaterThan(5.2))
        {
            CodeIntegrity();
        }

        // Clear the label text
        UpdateLabel(string.Empty);
    }

    // Helper methods
    private static void UpdateLabel(string text)
    {
        // Simulate updating a GUI label
        Console.WriteLine($"Updating label: {text}");
    }

    private static bool IsOsVersionGreaterThan(double version)
    {
        // Example check for the operating system version
        Version osVersion = Environment.OSVersion.Version;
        return osVersion.Major + osVersion.Minor / 10.0 > version;
    }

    private static void CodeIntegrity()
    {
        // Placeholder for Code Integrity logic
        Console.WriteLine("Executing CodeIntegrity...");
    }



    private static void Events1()
    {
        if (ServiceStatus("eventlog") == "R")
        {
            Events();
        }
        else
        {
            // Update the label with a scanning message
            UpdateLabel($"{StringConstants.SCANB} events...");

            // Path to sc.exe
            string scPath = Path.Combine(Environment.SystemDirectory, "sc.exe");

            // Run the command to configure the service to start automatically
            CommandHandler.RunCommand($"\"{scPath}\" config eventlog start= auto");

            // Start the eventlog service
            StartService("eventlog");

            // Wait for the service to start
            Thread.Sleep(5000);

            if (ServiceStatus("eventlog") == "R")
            {
                Events();
            }
            else
            {
                // Write to the log if the service did not start
                string logEntry = $"\n==================== {Events1Header}: ========================\n\n{Events1Body}\n\n";
                File.AppendAllText(AdditionLogPath, logEntry);

                // Path to net.exe
                string netPath = Path.Combine(Environment.SystemDirectory, "net.exe");

                // Attempt to start the service using net.exe and log the output
                string output = CommandHandler.RunCommandString($"{netPath} start eventlog");
                File.AppendAllText(AdditionLogPath, output);
            }
        }
    }

    // Helper methods
    private static string ServiceStatus(string serviceName)
    {
        // Placeholder for a method that checks the status of a service
        // "R" indicates the service is running
        return "R"; // Replace with actual service status logic
    }


    private static void StartService(string serviceName)
    {
        // Placeholder for starting a Windows service
        Console.WriteLine($"Starting service: {serviceName}");
    }

    // Paths and placeholder variables
    private static string AdditionLogPath = @"C:\Path\To\Log\File.log"; // Replace with your actual log file path
    private static string Events1Header = "EVENTS1 HEADER"; // Replace with actual header text
    private static string Events1Body = "EVENTS1 BODY"; // Replace with actual body text

    private static void ClearEventLogs()
    {
        Console.WriteLine("Clearing Event Logs...");
        try
        {
            var logs = EventLog.GetEventLogs();
            foreach (var log in logs)
            {
                log.Clear();
                log.Dispose();
            }
            Console.WriteLine("All event logs cleared.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing event logs: {ex.Message}");
        }
    }
}

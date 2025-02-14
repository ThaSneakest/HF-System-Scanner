using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class WinDefenderEventHandler
{
    public static void WinDef()
    {
        string tempDir = Path.GetTempPath();
        string path = Path.Combine(tempDir, "codeint" + new Random().Next(1000, 9999).ToString());

        // Run the first command (wevtutil for Level 3 events)
        CommandHandler.RunCommand($"wevtutil qe \"Microsoft-Windows-Windows Defender/Operational\" \"/q:*[System [(Level=3)]]\" /c:5 /rd:true /uni:true /f:text >> \"{path}\"");

        // Run the second command (wevtutil for Level 2 events)
        CommandHandler.RunCommand($"wevtutil qe \"Microsoft-Windows-Windows Defender/Operational\" \"/q:*[System [(Level=2)]]\" /c:5 /rd:true /uni:true /f:text >> \"{path}\"");

        // Read the contents of the file
        string events = File.ReadAllText(path);

        if (!events.Contains(":"))
            return;

        // Process the event data using regular expressions
        events = Regex.Replace(events, @"(?m)^\s*", "");
        events = Regex.Replace(events, @"^Event\[\d\]:?\r?\n", "");
        events = Regex.Replace(events, @"(?m)^(Log Name|(Scan|Event) ID|Task|Level|Opcode|Keyword|Source|User|User Name|Computer|ID):.*(\r\n|\r|\n)", "");
        events = Regex.Replace(events, @"(?m)^(Date:.+\d)T(\d.+\v{2})", "$1 $2");
        events = Regex.Replace(events, @"(?m)^(Date:.+?)\.\d+Z", "$1");
        events = Regex.Replace(events, @"(?s)\\R{2,}", Environment.NewLine + Environment.NewLine);


        // Write the modified events to another file or perform further action
        string additionFilePath = Path.Combine(tempDir, "defender_logs.txt");
        File.AppendAllText(additionFilePath, Environment.NewLine + "Windows Defender:" + Environment.NewLine + "================" + events);

        // Delete the temporary file
        File.Delete(path);
    }
}
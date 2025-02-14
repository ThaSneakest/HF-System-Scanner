using System;
using System.Diagnostics;
using System.IO;

public static class BCDHandler
{
    public static void LISTBCD(string systemDir, string frstDir, string frstLogPath, string osNum, string bootMode, string muiLang)
    {
        string bcdCommand = Path.Combine(systemDir, "bcdedit.exe") + " /enum all";
        string readResult;

        if (int.Parse(osNum) > 6 && bootMode != "Recovery")
        {
            // Paths for PowerShell script and output
            string tmpScriptPath = Path.Combine(frstDir, "tmp.ps1");
            string outputPath = Path.Combine(frstDir, "pw000.txt");
            string powershellPath = Path.Combine(systemDir, @"WindowsPowerShell\v1.0\powershell.exe");

            // Create PowerShell script
            File.WriteAllText(tmpScriptPath, bcdCommand + Environment.NewLine);

            // Execute PowerShell script and capture output
            CommandHandler.RunCommand($"{powershellPath} -ExecutionPolicy Bypass -File {tmpScriptPath} > {outputPath}");

            // Read the output file
            if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
            {
                readResult = File.ReadAllText(outputPath);
            }
            else
            {
                // Retry with an alternative command
                CommandHandler.RunCommand($"{powershellPath} -ExecutionPolicy Bypass -File {tmpScriptPath} 2>&1 > {outputPath}");
                readResult = File.ReadAllText(outputPath);
            }

            // Clean up temporary files
            File.Delete(tmpScriptPath);
            File.Delete(outputPath);
        }
        else
        {
            string chcpCommand = string.Empty;

            // Run CHCP to determine code page
            string cmdOutput = ExecuteCommandAndGetOutput("chcp", systemDir);

            if (cmdOutput.Contains("866") || IsCyrillicLanguage(muiLang))
            {
                chcpCommand = "chcp 1251 >NUL & ";
            }

            // Execute the command and capture output
            readResult = ExecuteCommandAndGetOutput($"{chcpCommand}{bcdCommand}", systemDir);
        }

        // Append result to the log file
        File.AppendAllText(frstLogPath, $"{Environment.NewLine}==================== BCD ================================{Environment.NewLine}{readResult}{Environment.NewLine}");
    }


    private static string ExecuteCommandAndGetOutput(string command, string workingDirectory = "")
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };

        using (var process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }

    private static bool IsCyrillicLanguage(string muiLang)
    {
        // Check if the language is Cyrillic based on specific language codes
        string[] cyrillicLangCodes = { "0419", "0422", "0423", "0402", "042F", "0C1A", "1C1A", "281A", "301A", "0428", "0450", "082C", "0843", "201A" };
        return Array.Exists(cyrillicLangCodes, code => muiLang.Contains(code));
    }
}

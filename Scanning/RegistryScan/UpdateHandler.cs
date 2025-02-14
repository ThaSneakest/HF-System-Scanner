using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public class UpdateHandler
{
    private static readonly string ScriptName = "MyScript.exe"; // Replace with the actual script name
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "FRST", "logs");
    private static readonly string UpdateLabel = "Updating...";
    private static readonly string ReadyLabel = "Ready";


    /// <summary>
    /// Updates the script by checking for new versions and downloading them if available.
    /// </summary>
    public static void UpdateScript()
    {
        if (ScriptName.Contains("beta"))
            return;

        Console.WriteLine(UpdateLabel);

        string up32Path = Path.Combine(LogsDirectory, "up32");
        if (File.Exists(up32Path))
            File.Delete(up32Path);

        // Download the update info
        using (WebClient webClient = new WebClient())
        {
            try
            {
                webClient.DownloadFile("http://download.bleepingcomputer.com/farbar/up32", up32Path);
            }
            catch
            {
                HandleUpdateFailure("1");
                return;
            }
        }

        if (!File.Exists(up32Path))
        {
            HandleUpdateFailure("1");
            return;
        }

        string ver1 = File.ReadAllText(up32Path);
        File.Delete(up32Path);

        string scriptPath = AppDomain.CurrentDomain.BaseDirectory + ScriptName;
        string ver2 = FileUtils.GetFileVersion(scriptPath);

        if (!string.IsNullOrEmpty(ver1) && !string.IsNullOrEmpty(ver2) && ver1 != ver2)
        {
            Console.WriteLine(UpdateLabel);
            string updatePath = Path.Combine(LogsDirectory, "FRSTupdate");
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile("http://www.bleepingcomputer.com/download/farbar-recovery-scan-tool/dl/81/", updatePath);
                }

                if (!File.Exists(updatePath))
                {
                    HandleUpdateFailure("3");
                    return;
                }

                string content = File.ReadAllText(updatePath);
                Match match = Regex.Match(content, "(?i)url=(https://download.bleepingcomputer.com/dl/.+/farbar-recovery-scan-tool/FRST.exe)");
                File.Delete(updatePath);

                if (!match.Success)
                {
                    HandleUpdateFailure("4");
                    return;
                }

                string downloadUrl = match.Groups[1].Value;
                string downloadedFilePath = Path.Combine(LogsDirectory, "FRST.exe");

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(downloadUrl, downloadedFilePath);
                }

                if (!File.Exists(downloadedFilePath))
                {
                    HandleUpdateFailure("5");
                    return;
                }

                string oldVersionDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FRST-OlderVersion");
                Directory.CreateDirectory(oldVersionDir);

                File.Move(scriptPath, Path.Combine(oldVersionDir, ScriptName));
                File.Move(downloadedFilePath, scriptPath);

                Console.WriteLine("Update complete");
                System.Diagnostics.Process.Start(scriptPath);
                Environment.Exit(0);
            }
            catch
            {
                HandleUpdateFailure("5");
                return;
            }
        }

        Console.WriteLine(ReadyLabel);
    }

    /// <summary>
    /// Handles update failures by displaying an error message.
    /// </summary>
    /// <param name="errorCode">The error code for the failure.</param>
    private static void HandleUpdateFailure(string errorCode)
    {
        string errorMessage = "Failed to update";

        // Simulate localization check
        if (Regex.IsMatch("0407|0807|0c07|1007|1407", "0407"))
            errorMessage = "Fehler beim Aktualisieren";

        Console.WriteLine($"{errorMessage} ({errorCode})");
        Console.WriteLine(ReadyLabel);
    }

    /// <summary>
    /// Gets the DNS server list using native Windows API calls.
    /// </summary>
    /// <returns>The primary and secondary DNS servers as a string.</returns>
    public static string GetDnsServers()
    {
        const int ErrorBufferOverflow = 111;

        int bufferSize = 0;
        uint result = IphlpapiNativeMethods.GetNetworkParams(IntPtr.Zero, ref bufferSize);

        if (result == ErrorBufferOverflow)
        {
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                result = IphlpapiNativeMethods.GetNetworkParams(buffer, ref bufferSize);
                if (result == 0)
                {
                    var dnsServerList = Marshal.PtrToStructure<Structs.DnsServerList>(buffer);
                    return dnsServerList.PrimaryDnsServer;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        return "Failed to retrieve DNS servers";
    }
}

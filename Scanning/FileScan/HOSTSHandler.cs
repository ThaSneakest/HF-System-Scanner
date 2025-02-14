using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner
{
    public static class HOSTSHandler
    {

        public static void GetHOSTS()
        {
            string hostsContent = string.Empty;
            string[] readLines = null;
            string rest = string.Empty;
            int y = 0;

            string systemDir = Environment.SystemDirectory;
            string fPath = Path.Combine(systemDir, "drivers", "etc", "hosts");

            // Update a label text (equivalent to GUICtrlSetData in AutoIt)
            // Example: label1.Text = $"{SCANB} Hosts: ";
            Console.WriteLine($"Hosts: ");

            if (File.Exists(fPath))
            {
                // Assuming `HADDITION` is a file path or stream to write output
                string hAdditionPath = "HADDITION.log";

                File.AppendAllText(hAdditionPath, "\r\n==================== Hosts CONTENT: =========================\r\n");
                File.AppendAllText(hAdditionPath, $"\r\n({hostsContent})\r\n\r\n");

                // Read hosts file
                string hosts = File.ReadAllText(fPath);

                // Use regex to extract lines with a pattern matching the AutoIt version
                MatchCollection matches = Regex.Matches(hosts, @"^\s*(\d.+?)(?:\r?\n|$)", RegexOptions.Multiline);
                readLines = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    readLines[i] = matches[i].Groups[1].Value;
                }

                for (int i = 0; i < Math.Min(readLines.Length, 30); i++) // Equivalent to limiting loop to 30 lines
                {
                    File.AppendAllText(hAdditionPath, readLines[i] + Environment.NewLine);
                }

                if (readLines.Length > 100)
                {
                    y = readLines.Length - 30;
                    File.AppendAllText(hAdditionPath, $"\r\nREST1 {y} MOLI.\r\n\r\n");
                }
            }
            else
            {
                File.AppendAllText("HADDITION.log", "\r\nNHOSTS\r\n");
            }
        }
        public static void HOSTSFILE(string fPath)
        {
            // Get file creation and modification dates
            DateTime dateCreated = File.GetCreationTime(fPath);
            DateTime dateModified = File.GetLastWriteTime(fPath);

            // Get file attributes
            FileAttributes fileAttributes = File.GetAttributes(fPath);
            string fatt = fileAttributes.ToString().Replace("Archive", string.Empty);

            // Check for reparse point (symbolic link or junction)
            if ((fileAttributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            {
                fatt += "L";
            }

            // Format attributes string
            string att = fatt.PadLeft(5, '0').Replace("0", "_");

            // Get file size
            long size = new FileInfo(fPath).Length;
            string sizes = size.ToString("D9");

            // Assuming `HADDITION` is a file path or stream to write output
            string hAdditionPath = "HADDITION.log";

            // Write information to the file
            string output = $"{dateCreated} - {dateModified} - {sizes} {att} {fPath}{Environment.NewLine}";
            File.AppendAllText(hAdditionPath, output);
        }
        

        public static void HOSTSLINE()
        {
            string labelText = $"{StringConstants.SCANB} Hosts: ";
            string systemDir = Environment.SystemDirectory;
            string hostsFilePath = Path.Combine(systemDir, "drivers", "etc", "hosts");
            string logFilePath = "FRSTLOG.log"; // Replace with the actual path to your log file

            // Update GUI label equivalent (placeholder logic for updating a label)
            Console.WriteLine(labelText);

            if (File.Exists(hostsFilePath))
            {
                // Read the hosts file
                string hostsContent = File.ReadAllText(hostsFilePath);

                if (hostsContent.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    File.AppendAllText(logFilePath, $"Hosts: {StringConstants.HOSTS3} Addition.txt <==== {StringConstants.UPD1}{Environment.NewLine}");
                }

                // Extract lines matching the regex pattern
                MatchCollection matches = Regex.Matches(hostsContent, @"^\s*(\d.+?)(?:\r?\n|$)", RegexOptions.Multiline);
                string[] readLines = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    readLines[i] = matches[i].Groups[1].Value;
                }

                int count = readLines.Length;

                if (count == 1 && !string.IsNullOrWhiteSpace(hostsContent) &&
                    !Regex.IsMatch(readLines[0], @"(?i)127\.0\.0\.1\s*localhost"))
                {
                    File.AppendAllText(logFilePath, $"Hosts: {readLines[0]}{Environment.NewLine}");
                }

                if (count > 1)
                {
                    // Equivalent to setting a variable in AutoIt
                    bool listHost = true;
                    File.AppendAllText(logFilePath, $"Hosts: {StringConstants.INTERNET6} Addition.txt{Environment.NewLine}");
                }
            }
            else
            {
                File.AppendAllText(logFilePath, $"Hosts: {StringConstants.INTERNET7}{Environment.NewLine}");
            }
        }

        private static string Hosts;

        public static void HostsScan()
        {
            int POS = 0;
            string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            using (StreamReader X8 = File.OpenText(Path.Combine(system32, "drivers", "etc", "hosts")))
            {
                string A = X8.ReadLine();
                StringBuilder B = new StringBuilder();

                while (A != null)
                {
                    if (!A.StartsWith("#") && A != "")
                    {
                        B.Append("Hosts:\t" + A + Environment.NewLine);
                    }

                    A = X8.ReadLine();
                    POS += 1;
                    if (POS == 15)
                    {
                        Logger.Instance.LogPrimary(B.ToString() +
                                         "The hosts file is more than 15 lines. Please use the hosts button." +
                                         Environment.NewLine);
                    }
                }

                Hosts = B.ToString();
            }
        }

        public static void EnumerateHostsFile()
        {
            string hostsFilePath = @"C:\WINDOWS\system32\drivers\etc\hosts";
            string hostsIcsPath = @"C:\WINDOWS\system32\drivers\etc\hosts.ics";

            // Process each file
            ProcessHostsFile(hostsFilePath);
            ProcessHostsFile(hostsIcsPath);
        }

        private static void ProcessHostsFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            // Get file metadata
            FileInfo fileInfo = new FileInfo(filePath);
            string creationTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
            string lastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            string fileSize = fileInfo.Length.ToString().PadLeft(9, '0'); // Pad file size to 9 digits

            // Log file metadata
            Logger.Instance.LogPrimary($"{creationTime} - {lastWriteTime} - {fileSize} _____ {filePath}");

            // Read file content and log each line
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Logger.Instance.LogPrimary(line.Trim());
                }
            }
        }
    }
}

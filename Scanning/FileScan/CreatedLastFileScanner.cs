using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Whitelist;

namespace Wildlands_System_Scanner.Scanning.FileScan
{
    public class CreatedLastFileScanner
    {




        public static void EnumerateRecentFiles(IEnumerable<string> directories, int days = 30)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-days);

            foreach (string directory in directories)
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Console.WriteLine($"Directory not found: {directory}");
                        continue;
                    }

                    // Enumerate all files
                    foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(file);

                            if (IsWhitelisted(fileInfo.FullName) || IsWhitelistedFolder(fileInfo.DirectoryName))
                            {
                                // Skip whitelisted files or files in whitelisted folders
                                continue;
                            }

                            string logEntry = FormatFileLogEntry(fileInfo);

                            if (fileInfo.CreationTime >= cutoffDate)
                            {
                                if (IsBlacklisted(fileInfo.FullName) || IsBlacklistedFolder(fileInfo.DirectoryName))
                                {
                                    logEntry += " <---- Malicious Entry Located";
                                }

                                Logger.Instance.LogPrimary(logEntry);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine($"Access denied to file: {file}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing file {file}: {ex.Message}");
                        }
                    }

                    // Enumerate all directories
                    foreach (string subDirectory in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(subDirectory);

                            if (IsWhitelistedFolder(dirInfo.FullName))
                            {
                                // Skip whitelisted folders
                                continue;
                            }

                            string logEntry = FormatDirectoryLogEntry(dirInfo);

                            if (dirInfo.CreationTime >= cutoffDate)
                            {
                                if (IsBlacklistedFolder(dirInfo.FullName))
                                {
                                    logEntry += " <---- Malicious Entry Located";
                                }

                                Logger.Instance.LogPrimary(logEntry);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine($"Access denied to directory: {subDirectory}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing directory {subDirectory}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing directory {directory}: {ex.Message}");
                }
            }

            Console.WriteLine("File enumeration completed. Results logged.");
        }

        private static string FormatFileLogEntry(FileInfo fileInfo)
        {
            string creationTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
            string lastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            string size = fileInfo.Length.ToString("D11"); // Right-aligned, padded to 11 characters
            string filePath = fileInfo.FullName;

            return $"{creationTime} - {lastWriteTime} - {size} _____ {filePath}";
        }

        private static string FormatDirectoryLogEntry(DirectoryInfo dirInfo)
        {
            string creationTime = dirInfo.CreationTime.ToString("yyyy-MM-dd HH:mm");
            string lastWriteTime = dirInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            string size = "000000000"; // Directories do not have sizes
            string dirPath = dirInfo.FullName;

            return $"{creationTime} - {lastWriteTime} - {size} ___HD {dirPath}";
        }

        private static bool IsWhitelisted(string filePath)
        {
            return FileWhitelist.WhitelistFile.Contains(filePath);
        }

        private static bool IsWhitelistedFolder(string folderPath)
        {
            return FolderWhitelist.WhitelistFolders.Any(whitelist => folderPath.StartsWith(whitelist, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBlacklisted(string filePath)
        {
            return BlacklistedFiles.FileBlacklist.Contains(filePath);
        }

        private static bool IsBlacklistedFolder(string folderPath)
        {
            return FolderBlacklist.BlacklistFolders.Any(blacklist => folderPath.StartsWith(blacklist, StringComparison.OrdinalIgnoreCase));
        }
    }
}

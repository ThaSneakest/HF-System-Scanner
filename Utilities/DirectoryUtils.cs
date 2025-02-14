using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Scripting;

namespace Wildlands_System_Scanner.Utilities
{
    public class DirectoryUtils
    {

        private static readonly string SearchLabel = "Searching folders...";
        private static readonly string FixLabel = "Fix completed.";


        public static bool BackupCheck(string folderPath)
        {
            // Example: Check if the folder name contains "backup" (case-insensitive)
            string folderName = Path.GetFileName(folderPath);
            return folderName != null && folderName.ToLower().Contains("backup".ToLower());

        }

        public static void PerformFolderSearch(string searchFilePath, string pattern)
        {
            try
            {
                // Get all directories in the current directory and subdirectories
                var directories = Directory.GetDirectories(Environment.CurrentDirectory, pattern, SearchOption.AllDirectories);

                foreach (var directory in directories)
                {
                    // Append each folder path to the search file
                    File.AppendAllText(searchFilePath, directory + Environment.NewLine);
                }

                Console.WriteLine("Folder search completed successfully.");
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., unauthorized access)
                Console.WriteLine($"Error during folder search: {ex.Message}");
            }
        }


        public static string FormatAttributes(FileAttributes attributes)
        {
            // Format attributes string and add 'L' for reparse points
            string attributeString = attributes.ToString().Replace("0", "_");
            if (attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                attributeString += "L";
            }

            return attributeString.PadLeft(5, '_');
        }


        public static string GetCompanyName(string filePath)
        {
            try
            {
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                return versionInfo.CompanyName?.Trim() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        public static readonly string SearchFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Search.txt");

        public static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
        
        public static void FindFolder(string searchPattern, string rootDirectory)
        {
            Console.WriteLine($"{SearchLabel} Searching for: {searchPattern}");

            // Write header to the log
            //Logger.WriteToLog($"================== FindFolder: \"{searchPattern}\" ===================");

            // Normalize search pattern
            searchPattern = searchPattern.Replace("|", "");

            // Search for folders matching the pattern
            var directories = DirectoryHandler.SearchDirectories(rootDirectory, searchPattern);

            if (directories.Count == 0)
            {
                //Logger.Instance.WriteToLog(NotFoundMessage);
            }
            else
            {
                foreach (var directory in directories)
                {
                    string creationDate = FileUtils.GetFileTime(directory, true);
                    string modificationDate = FileUtils.GetFileTime(directory, false);
                    string attributes = GetFormattedAttributes(directory);

                    //Logger.Instance.WriteToLog($"{creationDate} - {modificationDate} {attributes} {directory}");
                }
            }

            // Write footer to the log
           //Logger.Instance.WriteToLog($"=== {EndMessage} ===");
            Console.WriteLine(FixLabel);
        }

       

        public static string GetFormattedAttributes(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                string attrString = attributes.ToString().Replace("Directory", "");
                return attrString.Replace("0", "_").Trim();
            }
            catch
            {
                return "_____";
            }
        }

        public static void EnsureFolderAccessibility(string path)
        {
            try
            {
                // Attempt to create a test file to ensure the folder is writable
                string testFile = Path.Combine(path, "test.tmp");
                File.WriteAllText(testFile, string.Empty);
                File.Delete(testFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to ensure folder accessibility: {path}. {ex.Message}");
            }
        }

        
   

        public static bool IsFolderAccessible(string folderPath)
        {
            try
            {
                Directory.GetAccessControl(folderPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

       

        public static string[] GetAllSubFolders(string folderPath)
        {
            try
            {
                return Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve subfolders for: {folderPath}. Error: {ex.Message}");
                return null;
            }
        }

        // Backup the directory by copying its contents to a backup folder
        public static bool BackupFolder(string sourceFolder)
        {
            try
            {
                string backupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Backup");
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // Create a timestamped backup folder name to avoid overwriting previous backups
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string destinationFolder = Path.Combine(backupFolder, Path.GetFileName(sourceFolder) + "_" + timestamp);

                // Recursively copy the directory and its contents to the backup folder
                DirectoryFix.CopyDirectory(sourceFolder, destinationFolder);

                Console.WriteLine($"Backup completed: {destinationFolder}");
                return true;  // Return true if backup was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during backup: {ex.Message}");
                return false; // Return false if an error occurred
            }
        }
    }
}
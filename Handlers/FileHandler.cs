using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner.Handlers
{
    public class FileHandler
    {

        private static readonly string _baseDirectory;
        private static readonly string _recoveryDrive;
        private static readonly bool _isRecoveryMode;
        private static readonly string _scriptDirectory;

        /// <summary>
        /// Recursively retrieves all files in a directory and its subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search.</param>
        /// <returns>An array of file paths.</returns>
        public static string[] FileListToArrayRec(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory does not exist: {directoryPath}");
            }

            try
            {
                // Use a List to store file paths
                List<string> fileList = new List<string>();

                // Recursively get files
                GetFilesRecursively(directoryPath, fileList);

                // Convert the list to an array and return
                return fileList.ToArray();
            }
            catch (Exception ex)
            {
                throw new IOException($"Error retrieving file list: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to recursively add files to the list.
        /// </summary>
        /// <param name="directoryPath">The current directory to search.</param>
        /// <param name="fileList">The list to add file paths to.</param>
        public static void GetFilesRecursively(string directoryPath, List<string> fileList)
        {
            // Add files in the current directory
            fileList.AddRange(Directory.GetFiles(directoryPath));

            // Recursively search subdirectories
            foreach (string subDir in Directory.GetDirectories(directoryPath))
            {
                GetFilesRecursively(subDir, fileList);
            }
        }

        public static void FileSearcher(string baseDirectory, string recoveryDrive, bool isRecoveryMode)
        {
            // Perform the file search logic here
            Console.WriteLine($"Base Directory: {baseDirectory}");
            Console.WriteLine($"Recovery Drive: {recoveryDrive}");
            Console.WriteLine($"Is Recovery Mode: {isRecoveryMode}");
        }


        /// <summary>
        /// Searches for files in a directory and logs the results.
        /// </summary>
        /// <param name="directoryPath">The directory to search.</param>
        /// <param name="searchPattern">The search pattern (e.g., "*.txt").</param>
        /// <param name="logFilePath">The file path where results are logged.</param>
        public static void FileSearchLog(string directoryPath, string searchPattern, string logFilePath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine($"Directory does not exist: {directoryPath}");
                    return;
                }

                string[] files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);

                using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                {
                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        string logEntry = $"File: {fileInfo.FullName}, Size: {fileInfo.Length} bytes, Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}";
                        writer.WriteLine(logEntry);
                        Console.WriteLine(logEntry);
                    }
                }

                Console.WriteLine($"Search completed. Results logged to: {logFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file search: {ex.Message}");
            }
        }

        private static List<string> GetFilesRecursively(string directory, string searchPattern)
        {
            var fileList = new List<string>();

            try
            {
                if (Directory.Exists(directory))
                {
                    fileList.AddRange(Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Log or handle unauthorized access exceptions
            }
            catch (Exception ex)
            {
                // Log or handle unexpected exceptions
                Console.WriteLine($"Error while searching files: {ex.Message}");
            }

            return fileList;
        }

        public static void LogFileSearch(string[] filePaths)
        {
            using (var writer = new StreamWriter(FileConstants.SearchFilePath, append: true))
            {
                foreach (var filePath in filePaths)
                {
                    try
                    {
                        if (!File.Exists(filePath))
                            continue;

                        // File attributes
                        var attributes = File.GetAttributes(filePath);
                        string attributesFormatted = FileUtils.FormatAttributes(attributes);

                        // File size
                        long fileSize = new FileInfo(filePath).Length;
                        string fileSizeFormatted = fileSize.ToString("D9");

                        // File hash (MD5)
                        string fileHash = Utility.GetMD5Hash(filePath);

                        // Creation and modification times
                        var creationTime = File.GetCreationTime(filePath);
                        var modificationTime = File.GetLastWriteTime(filePath);

                        // File version (CompanyName)
                        string companyName = FileUtils.GetFileVersionCompanyName(filePath);

                        // Additional attention messages
                        string attention = "";
                        if (CryptUtils.IsCryptEnabled())
                        {
                            if (FileUtils.IsSignatureValid(filePath)) // Example signature check
                            {
                                attention = " [Valid Signature]";
                            }
                            else
                            {
                                attention = " [No Signature]";
                                if (!FileUtils.HasAccess(filePath))
                                {
                                    attention = " [No Access]";
                                }
                                else if (FileUtils.IsFileInUse(filePath))
                                {
                                    attention = " [In Use]";
                                }
                            }
                        }

                        // Write the log
                        writer.WriteLine(filePath);
                        writer.WriteLine($"[{creationTime:yyyy-MM-dd HH:mm:ss}][{modificationTime:yyyy-MM-dd HH:mm:ss}] {fileSizeFormatted} {attributesFormatted} ({companyName}) {fileHash}{attention}");
                        writer.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions gracefully (e.g., file access issues)
                        writer.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    }
                }
            }
        }


    }
}

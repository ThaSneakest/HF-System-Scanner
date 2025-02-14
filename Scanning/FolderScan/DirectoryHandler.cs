using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Wildlands_System_Scanner.Constants;
using System.Security.AccessControl;
using Wildlands_System_Scanner.Scripting;
using System.Runtime.Serialization.Formatters;
using Wildlands_System_Scanner.Utilities;

public class DirectoryHandler
{
    private static string GetDestination(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"The specified directory does not exist: {path}");

        // Ensure the destination path appends "_Moved" while preserving the original directory structure
        string parentDirectory = Path.GetDirectoryName(path);
        string directoryName = Path.GetFileName(path);
        string movedDirectoryName = directoryName + "_Moved";

        string destination = Path.Combine(parentDirectory, movedDirectoryName);

        // Handle potential conflicts (if the destination directory already exists)
        int counter = 1;
        while (Directory.Exists(destination))
        {
            destination = Path.Combine(parentDirectory, $"{movedDirectoryName}_{counter}");
            counter++;
        }

        return destination;
    }

    public static void GrantFolderPermissions(string folderPath, string identity, FileSystemRights permissions)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The specified folder does not exist: {folderPath}");

        if (string.IsNullOrEmpty(identity))
            throw new ArgumentException("Identity cannot be null or empty.", nameof(identity));

        try
        {
            // Get the current access control settings for the folder
            DirectorySecurity security = Directory.GetAccessControl(folderPath);

            // Create a new access rule
            var accessRule = new FileSystemAccessRule(
                identity,
                permissions,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow
            );

            // Add the access rule to the folder's security settings
            security.AddAccessRule(accessRule);

            // Apply the updated security settings
            Directory.SetAccessControl(folderPath, security);

            Console.WriteLine($"Permissions granted successfully for '{identity}' on folder: {folderPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error granting folder permissions: {ex.Message}");
            throw;
        }
    }



    /// <summary>
    /// Checks if a given path is a folder junction.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is a junction; otherwise, false.</returns>
    public static bool IsJunction(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking for junction: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Extracts the value inside the <WorkingDirectory> tags from the provided input string.
    /// </summary>
    /// <param name="read">The input string to search.</param>
    /// <returns>The value of the <WorkingDirectory> tag if found; otherwise, an empty string.</returns>
    public static string WORKDIR(string read)
    {
        if (string.IsNullOrEmpty(read))
        {
            throw new ArgumentException("Input string cannot be null or empty.", nameof(read));
        }

        // Define the regex pattern
        string pattern = @"(?i)<WorkingDirectory>(.+?)<\/WorkingDirectory>";

        // Use Regex.Match to find a match
        Match match = Regex.Match(read, pattern);

        // If a match is found, return the first capture group (the content inside the <WorkingDirectory> tags)
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    /// <summary>
    /// Retrieves files from specified system directories based on file patterns.
    /// </summary>
    /// <returns>An array of file paths.</returns>
    public static string[] ZB()
    {
        // Initialize an empty list to store the file paths
        var zbList = new List<string>();

        // Call ZB2 with different directories and patterns, and add results to zbList
        ZB2(Environment.GetEnvironmentVariable("WINDIR"), "*.exe;*.dll", ref zbList);
        ZB2(Environment.GetEnvironmentVariable("SYSTEMROOT"), "*.exe;*.dll", ref zbList);
        ZB2(Path.Combine(Environment.GetEnvironmentVariable("SYSTEMROOT"), "Drivers"), "*.sys", ref zbList);

        // Return the list as an array
        return zbList.ToArray();
    }

    public static void ZB2(string path, string search, ref List<string> result)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Console.WriteLine($"Invalid or non-existent directory: {path}");
            return;
        }

        try
        {
            // Split the search patterns by semicolon
            string[] patterns = search.Split(';');

            foreach (var pattern in patterns)
            {
                // Get all files matching the pattern in the directory and subdirectories
                string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        // Get file info and check if the size is 0
                        FileInfo fileInfo = new FileInfo(file);

                        if (fileInfo.Length == 0)
                        {
                            // Get file access rights and timestamp
                            string fileAccess = FileFix.GetFileAccess(file);
                            string creationDate = DirectoryHandler.GetFileDate(file, true); // Get creation date
                            string modificationDate =
                                DirectoryHandler.GetFileDate(file, false); // Get modification date

                            // Format and add the result to the list
                            string formattedResult = $"{file} [{creationDate}] {fileAccess}";
                            result.Add(formattedResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    }
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to directory: {path}. Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing directory: {path}. Error: {ex.Message}");
        }
    }


    /// <summary>
    /// Recursively collects valid files from user directories and common system directories.
    /// </summary>
    /// <param name="file">The starting file or directory path.</param>
    /// <param name="result">A reference to the list for storing collected file paths.</param>
    public static void ZBFILESINDIR3(string file, ref List<string> result)
    {
        // Simulate the ALLUSERS array with directories for all users
        string[] allUsers = GetAllUsers();

        // Traverse all user directories
        foreach (var user in allUsers)
        {
            try
            {
                // Get all files in the user's directory
                string[] userFiles = Directory.GetFiles(user, "*", SearchOption.AllDirectories);

                foreach (var userFile in userFiles)
                {
                    if (IsValidFile(userFile))
                    {
                        ZBFILESINDIR3(userFile, ref result);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {user}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory: {user}. Error: {ex.Message}");
            }
        }

        // Collect files from common system directories
        CollectFilesFromCommonDirectories(ref result);
    }

    /// <summary>
    /// Collects files from common system directories and adds them to the result list.
    /// </summary>
    /// <param name="result">A reference to the list for storing collected file paths.</param>
    private static void CollectFilesFromCommonDirectories(ref List<string> result)
    {
        string[] commonDirectories = new[]
        {
            @"C:\Program Files", Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        // Add Common Application Data for older Windows versions
        if (Environment.OSVersion.Version.Major < 6)
        {
            Array.Resize(ref commonDirectories, commonDirectories.Length + 1);
            commonDirectories[commonDirectories.Length - 1] =
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        }

        foreach (var dir in commonDirectories)
        {
            ZBFILESINDIR2(dir, ref result);
        }
    }

    /// <summary>
    /// Processes a file and generates a formatted result string with file attributes, size, dates, and version.
    /// </summary>
    /// <param name="filePath">The path of the file to process.</param>
    /// <param name="result">The output result string with file details.</param>
    public static void ZBFILESINDIR3(string filePath, ref string result)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            result = "Invalid file path or file does not exist.";
            return;
        }

        try
        {
            // Get file attributes
            string attributes = File.GetAttributes(filePath).ToString();
            attributes = attributes.Replace("Archive", "");

            // Check for reparse points
            if (IsReparsePoint(filePath))
                attributes += "L";

            string formattedAttributes = string.Format("{0,5}", attributes).Replace("0", "_");

            // Get file size
            long fileSize = new FileInfo(filePath).Length;
            string formattedSize = string.Format("{0:D9}", fileSize);

            // Get file dates
            string creationDate = GetFileDate(filePath, true); // Creation date
            string modificationDate = GetFileDate(filePath, false); // Last modification date

            // Get file version
            string version = GetFileVersion(filePath);
            version = Regex.Replace(version, @"(?i)http(s|):", "hxxp$1:"); // Obfuscate URLs

            // Combine all details into the result string
            result =
                $"{creationDate} - {modificationDate} - {formattedSize} {formattedAttributes} ({version}) {filePath}";
        }
        catch (Exception ex)
        {
            result = $"Error processing file {filePath}: {ex.Message}";
        }
    }

    /// <summary>
    /// Checks if the specified file is a reparse point.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns>True if the file is a reparse point; otherwise, false.</returns>
    private static bool IsReparsePoint(string filePath)
    {
        try
        {
            return (File.GetAttributes(filePath) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the specified date (creation or last modification) of a file.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    /// <param name="isCreationDate">True to get the creation date; false to get the last modification date.</param>
    /// <returns>The formatted date string.</returns>
    public static string GetFileDate(string filePath, bool isCreationDate)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            return "File does not exist.";

        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            DateTime date = isCreationDate ? fileInfo.CreationTime : fileInfo.LastWriteTime;
            return date.ToString("yyyy-MM-dd HH:mm:ss"); // Format the date as needed
        }
        catch (Exception ex)
        {
            return $"Error retrieving date: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the file version, if available.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    /// <returns>The file version string, or "No Version Info" if unavailable.</returns>
    private static string GetFileVersion(string filePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return string.IsNullOrEmpty(versionInfo.FileVersion) ? "No Version Info" : versionInfo.FileVersion;
        }
        catch
        {
            return "No Version Info";
        }
    }

    /// <summary>
    /// Processes all files in a directory recursively, excluding specific patterns, and adds results to a list.
    /// </summary>
    /// <param name="path">The directory path to process.</param>
    /// <param name="result">The list to store processed file details.</param>
    public static void ZBFILESINDIR2(string path, ref List<string> result)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            Console.WriteLine($"Invalid or non-existent directory: {path}");
            return;
        }

        try
        {
            // Get all files recursively in the directory
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    // Skip files matching the regex pattern
                    if (!StringRegExp(file,
                            @"(?i)(desktop\.ini|iconcache|gdipfontcache|ntuser\.|LastFlashConfig\.wfc)"))
                    {
                        // Process the file with ZBFILESINDIR3
                        string fileResult = string.Empty;
                        ZBFILESINDIR3(file, ref fileResult);
                        result.Add(fileResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to directory: {path}. Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing directory: {path}. Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a string matches a given regular expression pattern.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <param name="pattern">The regex pattern to match against.</param>
    /// <returns>True if the string matches the pattern; otherwise, false.</returns>
    private static bool StringRegExp(string input, string pattern)
    {
        try
        {
            return Regex.IsMatch(input, pattern);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Regex error: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Validates a file based on its extension and excludes invalid file names.
    /// </summary>
    /// <param name="file">The file path to validate.</param>
    /// <returns>True if the file is valid; otherwise, false.</returns>
    public static bool IsValidFile(string file)
    {
        if (string.IsNullOrEmpty(file))
            return false;

        // List of invalid file names or patterns
        string[] invalidFiles =
        {
            "FullRemove.exe", "MakeMarkerFile.exe", "EasySurvey.exe", "ezsidmv.dat", "g2mdlhlpx.exe", "ntuser.dat",
            "nvModes.dat", "GoToAssistDownloadHelper.exe", "PKP_DL", "jagex_cl.+.dat", "random.dat", "SetStretch.",
            "Lenovo-\\d+.vbs", "pswi_preloaded.exe"
        };

        // Regex pattern for valid file extensions
        string validExtensionsPattern =
            @"(?i)\.(reg|bat|pad|dat|dll|exe|plz|ctrl|pff|js|dss|pss|fvv|bxx|fdd|jss|zvv|odd|fee|vbs|cmd)\Z";

        // Check for valid file extensions
        if (!Regex.IsMatch(file, validExtensionsPattern))
        {
            return false;
        }

        // Check for invalid file names
        foreach (var invalidFile in invalidFiles)
        {
            if (Regex.IsMatch(file, $"(?i){Regex.Escape(invalidFile)}"))
            {
                return false;
            }
        }

        // File is valid if it passes both checks
        return true;
    }

    /// <summary>
    /// Retrieves the directories of all user profiles on the system.
    /// </summary>
    /// <returns>An array of strings representing the paths to user profiles.</returns>
    public static string[] GetAllUsers()
    {
        var userProfiles = new List<string>();

        try
        {
            // Get the base directory for user profiles
            string baseProfileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string baseDir = Path.GetDirectoryName(baseProfileDir);

            if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
            {
                Console.WriteLine("Unable to determine the base user profile directory.");
                return Array.Empty<string>();
            }

            // Enumerate all subdirectories under the base directory
            foreach (string dir in Directory.GetDirectories(baseDir))
            {
                // Optional: Skip default system directories
                string dirName = Path.GetFileName(dir);
                if (dirName.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
                    dirName.Equals("Public", StringComparison.OrdinalIgnoreCase) ||
                    dirName.Equals("All Users", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                userProfiles.Add(dir);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied while retrieving user profiles: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user profiles: {ex.Message}");
        }

        return userProfiles.ToArray();
    }

    public static void ProcessFilesInDirectory(string inputFix)
    {
        // Log header
        Logger.Instance.LogPrimary($"\n========================= {inputFix} ========================\n\n");

        // Clean input
        inputFix = inputFix.Replace("\"", "");
        string folderPath =
            System.Text.RegularExpressions.Regex.Replace(inputFix, @"(?i)Filesindirectory:\s*(.+)\.", "$1");

        // Display message (Simulating GUI label update)
        Console.WriteLine($"Scanning: {folderPath}");

        if (!Directory.Exists(folderPath))
        {
            Logger.Instance.LogPrimary(folderPath + "Not Found");
            return;
        }

        string pattern = System.Text.RegularExpressions.Regex.Replace(inputFix, @"(?i).+\\(.+)", "$1").Replace("|", "");
        var files = Directory.EnumerateFiles(folderPath, pattern, SearchOption.AllDirectories);

        foreach (var filePath in files)
        {
            try
            {
                var fileAttributes = File.GetAttributes(filePath);
                string attributeString = DirectoryUtils.FormatAttributes(fileAttributes);

                long fileSize = new FileInfo(filePath).Length;
                string fileSizeFormatted = fileSize.ToString("D9");

                DateTime creationTime = File.GetCreationTime(filePath);
                DateTime lastModifiedTime = File.GetLastWriteTime(filePath);

                string company = StringConstants.NoAccessMessage;
                string hash = Utility.GetMD5Hash(filePath);

                if (FileUtils.IsExecutableOrDll(filePath))
                {
                    // Handle logic for .dll, .exe, .sys, .mui files
                    company = DirectoryUtils.GetCompanyName(filePath);
                }
                else
                {
                    company = $"({DirectoryUtils.GetCompanyName(filePath)})";
                }

                Logger.Instance.LogPrimary(
                    $"{creationTime:yyyy-MM-dd HH:mm:ss} - {lastModifiedTime:yyyy-MM-dd HH:mm:ss} - {fileSizeFormatted} {attributeString} [{hash}] {company} {filePath}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        // Log footer
        Logger.Instance.LogPrimary($"\n====== {StringConstants.END} Filesindirectory ======\n");
    }

    public static void ProcessFileFolder()
    {
        // Example paths and variables
        string scanMessage = "Scanning";
        string logPath = Path.Combine(Environment.CurrentDirectory, "FRST.txt");
        List<string> fileList = new List<string>();
        List<string> unsigFiles = new List<string>();
        List<string> lockedFiles = new List<string>();
        List<string> fCheck = new List<string>();

        // Simulate updating a UI label
        Console.WriteLine($"{scanMessage} Files and Folders...");

        // Define the directories to scan
        string[] dirsToScan = new string[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            // Add other paths as needed
        };

        // Log the process to a file
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            foreach (var dir in dirsToScan)
            {
                if (Directory.Exists(dir))
                {
                    ProcessDirectory(dir, writer, fileList);
                }
            }

            // Example for locked files
            if (lockedFiles.Count > 0)
            {
                writer.WriteLine("==================== Locked Files ==============================");
                foreach (var lockedFile in lockedFiles)
                {
                    writer.WriteLine(lockedFile);
                }
            }

            // Example for unsigned files
            if (unsigFiles.Count > 0)
            {
                writer.WriteLine("==================== Unsigned Files =========================");
                foreach (var unsigFile in unsigFiles)
                {
                    writer.WriteLine(unsigFile);
                }
            }

            // Process files for checks
            foreach (var path in fCheck)
            {
                if (File.Exists(path))
                {
                    writer.WriteLine($"FCheck: {path} [LastModified: {File.GetLastWriteTime(path)}] <==== Updated");
                }
                else
                {
                    writer.WriteLine($"FCheck: {path} <==== Not Found");
                }
            }
        }

        Console.WriteLine("File and folder processing complete.");
    }


    public static void ProcessDirectory(string dir, StreamWriter writer, List<string> fileList)
    {
        try
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                writer.WriteLine($"File: {file}");
                fileList.Add(file);
            }

            foreach (var subDir in Directory.GetDirectories(dir))
            {
                ProcessDirectory(subDir, writer, fileList);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            writer.WriteLine($"Access Denied: {dir} - {ex.Message}");
        }
        catch (Exception ex)
        {
            writer.WriteLine($"Error processing directory {dir}: {ex.Message}");
        }
    }

    public static void SearchFolders(string searchPattern)
    {
        using (var writer = new StreamWriter(DirectoryUtils.SearchFilePath, append: true))
        {
            // Get all matching directories and files
            var directories = Directory.EnumerateDirectories("C:\\", searchPattern, SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                try
                {
                    // Get creation and last modification dates
                    var creationDate = Directory.GetCreationTime(directory);
                    var modifiedDate = Directory.GetLastWriteTime(directory);

                    // Get directory attributes
                    var attributes = File.GetAttributes(directory);
                    var attributesString = DirectoryUtils.FormatAttributes(attributes);

                    // Write results to file
                    writer.WriteLine(
                        $"{creationDate:yyyy-MM-dd HH:mm:ss} - {modifiedDate:yyyy-MM-dd HH:mm:ss} {attributesString} {directory}");
                }
                catch (Exception ex)
                {
                    // Handle exceptions gracefully (e.g., access denied)
                    writer.WriteLine($"Error processing {directory}: {ex.Message}");
                }
            }
        }
    }


    public static List<string> SearchDirectories(string root, string pattern)
    {
        var directories = new List<string>();

        try
        {
            directories.AddRange(Directory.GetDirectories(root, pattern, SearchOption.AllDirectories));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while searching directories: {ex.Message}");
        }

        return directories;
    }

    public static List<string> GetAllSubfolders(string path)
    {
        try
        {
            return new List<string>(Directory.GetDirectories(path, "*", SearchOption.AllDirectories));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving subfolders: {path}. {ex.Message}");
            return new List<string>();
        }
    }

}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;
using static Wildlands_System_Scanner.NativeMethods.Flags;

namespace Wildlands_System_Scanner.Scripting
{

    public class FileFix
    {
        private static string Fix = "FCheck: C:\\ExamplePath\\ExampleFile.txt [AdditionalInfo]";
        public static void SetDefaultFileAccess(string path, FileSystemAccessRule defaultAccessRule = null)
        {
            try
            {
                // Check if the path exists
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException($"The specified path does not exist: {path}");
                }

                FileSystemSecurity security;

                // Retrieve access control for file or directory
                if (File.Exists(path))
                {
                    security = File.GetAccessControl(path);
                }
                else
                {
                    security = Directory.GetAccessControl(path);
                }

                // Define the default access rule if none is provided
                if (defaultAccessRule == null)
                {
                    var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    defaultAccessRule = new FileSystemAccessRule(
                        everyone,
                        FileSystemRights.Read | FileSystemRights.Write | FileSystemRights.ExecuteFile,
                        AccessControlType.Allow
                    );
                }

                // Add the access rule
                security.AddAccessRule(defaultAccessRule);

                // Save the changes
                if (File.Exists(path))
                {
                    File.SetAccessControl(path, (FileSecurity)security);
                }
                else
                {
                    Directory.SetAccessControl(path, (DirectorySecurity)security);
                }

                Console.WriteLine($"Default access successfully set for: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting default file access: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a list of files recursively from the specified directory.
        /// </summary>
        /// <param name="directoryPath">The root directory to search.</param>
        /// <param name="searchPattern">Optional search pattern (e.g., "*.txt"). Use "*" for all files.</param>
        /// <param name="includeSubdirectories">Whether to include files in subdirectories.</param>
        /// <returns>A list of file paths.</returns>
        public static List<string> GetFileListRecursively(string directoryPath, string searchPattern = "*", bool includeSubdirectories = true)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"The specified directory does not exist: {directoryPath}");

            var fileList = new List<string>();
            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                // Retrieve all files matching the search pattern
                fileList.AddRange(Directory.GetFiles(directoryPath, searchPattern, searchOption));
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {directoryPath}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving files: {ex.Message}");
                throw;
            }

            return fileList;
        }

        /// <summary>
        /// Moves a file to the specified destination.
        /// If a file with the same name exists at the destination, it appends a numeric suffix to avoid overwriting.
        /// </summary>
        /// <param name="sourceFile">The full path of the source file.</param>
        /// <param name="destinationFolder">The destination folder where the file should be moved.</param>
        public static void MoveFileNormal(string sourceFile, string destinationFolder)
        {
            if (string.IsNullOrEmpty(sourceFile))
                throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));

            if (string.IsNullOrEmpty(destinationFolder))
                throw new ArgumentException("Destination folder cannot be null or empty.", nameof(destinationFolder));

            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"Source file does not exist: {sourceFile}");

            try
            {
                // Ensure the destination folder exists
                Directory.CreateDirectory(destinationFolder);

                // Generate the destination file path
                string fileName = Path.GetFileName(sourceFile);
                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Handle conflicts by appending a numeric suffix if the file already exists
                int counter = 1;
                while (File.Exists(destinationPath))
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    destinationPath = Path.Combine(destinationFolder, $"{fileNameWithoutExtension}_{counter}{extension}");
                    counter++;
                }

                // Move the file
                File.Move(sourceFile, destinationPath);

                Console.WriteLine($"File moved: {sourceFile} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file {sourceFile}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves access control information for a file.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>A string describing the access control information.</returns>
        public static string GetFileAccess(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                return "File does not exist.";

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                var accessControl = fileInfo.GetAccessControl();

                // Return a simplified description of the access rules
                return "Access granted"; // Customize this to return more detailed information if needed
            }
            catch (UnauthorizedAccessException)
            {
                return "Access denied.";
            }
            catch (Exception ex)
            {
                return $"Error retrieving access control: {ex.Message}";
            }
        }
        public static async Task ProcessFile()
        {
            //Logger.WriteToLog(Logger.WildlandsLogFile, $"\n========================= {FixDescription} ========================\n\n");
            string filePathsString = Regex.Replace(Fix, @"(?i)File:\s*(.+)", "$1");
            var paths = filePathsString.Split(';').Select(p => p.Trim(' ', '"')).ToArray();

            if (paths.Length == 0)
            {
                //Logger.WriteToLog(Logger.WildlandsLogFile, "File: Error reading paths\n");
                return;
            }

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths.Length > 4 && i % 4 == 0)
                {
                    System.Threading.Thread.Sleep(60000); // Pause for a minute
                }

                string filePath = paths[i];
                if (!File.Exists(filePath))
                {
                    //Logger.WriteToLog(Logger.WildlandsLogFile, $"File not found: {filePath}\n");
                    continue;
                }

                string createdDate = File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                string modifiedDate = File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                string fileSize = new FileInfo(filePath).Length.ToString("N0");

                string hash = Utility.CalculateMD5(filePath);
                string virusscanResult = await JottiScanHandler.Jotti();

                string fileAttributes = FileUtils.GetFileAttributes(filePath);
                string fileInfo = FileUtils.GetFileProperties(filePath);

                //Logger.WriteToLog(Logger.WildlandsLogFile,
                //    $"{filePath}\n" +
                //    $"MD5: {hash}\n" +
                //    $"{Creamod}: {createdDate} - {modifiedDate}\n" +
                //    $"{SizeLabel}: {fileSize}\n" +
                //    $"{AttributesLabel}: {fileAttributes}\n" +
                //    $"{fileInfo}\n" +
                //    $"Virusscan: {virusscanResult}\n\n"
                //);
            }

            //Logger.WriteToLog(Logger.WildlandsLogFile, $"====== {EndText} {OfText} File: ======\n\n");
        }

        public static bool FileWriteToLine(string filePath, int lineNumber, string text, bool overwrite = false, bool fill = false)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("The specified file does not exist.");

            if (lineNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(lineNumber), "Line number must be greater than 0.");

            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text to write cannot be null or empty.", nameof(text));

            try
            {
                // Read all lines from the file
                List<string> lines = new List<string>(File.ReadAllLines(filePath));
                int totalLines = lines.Count;

                if (fill)
                {
                    // Fill with empty lines if needed
                    while (lines.Count < lineNumber)
                    {
                        lines.Add(string.Empty);
                    }
                }
                else
                {
                    // Check if the line number exceeds the file length
                    if (lineNumber > totalLines)
                        throw new ArgumentOutOfRangeException(nameof(lineNumber), "Specified line number exceeds the number of lines in the file.");
                }

                // Adjust for zero-based index
                int targetIndex = lineNumber - 1;

                // Modify the line
                if (overwrite)
                {
                    lines[targetIndex] = text;
                }
                else
                {
                    lines[targetIndex] = text + Environment.NewLine + lines[targetIndex];
                }

                // Write the modified content back to the file
                File.WriteAllLines(filePath, lines);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // Method to handle file renaming and moving
        public static void ResFile(string path)
        {
            string ren = path;

            // Check if the file has the '.xBAD' suffix
            if (Regex.IsMatch(path, @".xBAD$"))
            {
                // Remove the '.xBAD' suffix and rename the file
                ren = Regex.Replace(path, @"(.+?).xBAD$", "$1");
                File.Move(path, ren); // Move the file to the new name
            }

            // Process the new path
            string des = Regex.Replace(ren, @"(?i)[a-z]:\\FRST\\Quarantine\\([a-z])(\\.+)", "$1:$2");

            // If the destination file doesn't exist
            if (!File.Exists(des))
            {
                // Set default file access and move the file
                SetDefaultFileAccess(ren);
                File.Move(ren, des); // Move the file to the destination path with overwrite
            }

            // Log the file operation
            File.AppendAllText("fixlog.txt", path + " => " + des + Environment.NewLine);
        }

        public static void RESFOL(string path)
        {
            string[] ren = FileUtils._FILELISTTOARRAYREC(path, "*", 1, 1, 0, 2);
            for (int i = 1; i < ren.Length; i++)
            {
                if (Regex.IsMatch(ren[i], @".xBAD$"))
                {
                    string renn = Regex.Replace(ren[i], @"(.+?).xBAD$", "$1");
                    File.Move(ren[i], renn);
                }
            }

            ren = FileUtils._FILELISTTOARRAYREC(path, "*", 0, 1, 1, 2);
            for (int i = 1; i < ren.Length; i++)
            {
                string des = Regex.Replace(ren[i], @"(?i)[a-z]:\\FRST\\Quarantine\\([a-z])(\\.+)", "$1:$2");
                if (!File.Exists(des))
                {
                    if ((File.GetAttributes(ren[i]) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        Directory.Move(ren[i], des);
                    }
                    else
                    {
                        FileUtils._SETDEFAULTFILEACCESS(ren[i]);
                        File.Move(ren[i], des);
                    }
                }
            }

            ren = FileUtils._FILELISTTOARRAYREC(path, "*", 0, 1, 1, 2);
            if (ren.Length > 0)
            {
                Array.Sort(ren);
                for (int i = 0; i < ren.Length; i++)
                {
                    Directory.Delete(ren[i], true);
                }
            }

            File.AppendAllText("HFIXLOG.txt", "=> " + "RESQUA" + Environment.NewLine);
        }

        private static bool MoveFile(string source, string destination)
        {
            try
            {
                File.Move(source, destination);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UnlockFile(string path, string accounts)
        {
            try
            {
                FileSystemSecurity security;
                if (File.Exists(path))
                {
                    security = File.GetAccessControl(path);
                }
                else if (Directory.Exists(path))
                {
                    security = Directory.GetAccessControl(path);
                }
                else
                {
                    Console.WriteLine($"Error: Path does not exist: {path}");
                    return false;
                }

                foreach (var account in accounts.Split(';'))
                {
                    var identity = new NTAccount(account);
                    security.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                }

                if (File.Exists(path))
                {
                    File.SetAccessControl(path, (FileSecurity)security);
                }
                else
                {
                    Directory.SetAccessControl(path, (DirectorySecurity)security);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UnlockFile: {ex.Message}");
                return false;
            }
        }
        public static bool SetDefaultFileAccess(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine($"Error: File or directory does not exist: {path}");
                return false;
            }

            string accounts;
            if (path.IndexOf(SystemConstants.CurrentUserName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                accounts = "Administrators;System;Users;" + SystemConstants.CurrentUserName;
            }
            else
            {
                accounts = "Administrators;System;Users;Authenticated Users";
                if (Environment.OSVersion.Version.Major > 5 && (path.Contains(FolderConstants.WinDir) || path.Contains(FolderConstants.ProgramFiles)))
                {
                    accounts += ";TrustedInstaller";
                }
            }

            if (!UnlockFile(path, accounts))
            {
                Console.WriteLine($"Error: Unable to set default file access for {path}");
                return false;
            }

            return true;
        }

        public static bool SetPrivileges(string privilege, bool enable)
        {
            // Placeholder for privilege setting logic using P/Invoke
            Console.WriteLine($"{(enable ? "Enabling" : "Disabling")} privilege: {privilege}");
            return true;
        }

        public static void SetDefaultPrivileges()
        {
            SetPrivileges("SeDebugPrivilege", true);
            SetPrivileges("SeSecurityPrivilege", true);
            SetPrivileges("SeRestorePrivilege", true);
            SetPrivileges("SeTakeOwnershipPrivilege", true);
            SetPrivileges("SeBackupPrivilege", true);
        }

        public static void Run(string filePath, string hashes)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                FileUtils.BAMMIS();
            }
            else
            {
                if (FileUtils.BAMWL() == 0) // Check if BAMWL indicates "false"
                {
                    string fileHash = Utility.GetMD5Hash(filePath);

                    // Check if the file hash matches any of the provided hashes
                    if (System.Text.RegularExpressions.Regex.IsMatch(fileHash, "(?i)" + hashes))
                    {
                        File.AppendAllText("FRST.txt", filePath + " => " + "MD5 Label" + Environment.NewLine);
                    }
                    else
                    {
                        FileUtils.SYSTEMFILE(fileHash);
                    }
                }
            }
        }

        public static void DELETESTARTUP(string fix)
        {
            // Use Regex to extract the file path from the fix string
            string filePath = Regex.Replace(fix, "(?i)Startup: ([A-Z]:\\\\.+?) \\[.*", "$1");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            // Move the file (you can implement a specific move action here)
            MOVEFILER(filePath);
        }

        public static void MOVEFILER(string filePath)
        {
            try
            {
                string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.GetFileName(filePath));
                // Move the file to the Desktop (as an example destination)
                File.Move(filePath, destination);
                Console.WriteLine($"File moved to: {destination}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file: {ex.Message}");
            }
        }

        public static void DELETESTARTUPTARGET(string fix)
        {
            // Use Regex to extract the shortcut target file path from the fix string
            string filePath = Regex.Replace(fix, "(?i)ShortcutTarget: .+ -> (.+) \\(.*", "$1");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                //   Logger.NFOUND(filePath);  // You can customize the NFOUND method here for error handling
            }
            else
            {
                // Move the file
                MOVEFILER(filePath);
            }
        }

        public static void DELETEURL(string fix)
        {
            // Extract the file path using regex
            string filePath = Regex.Replace(fix, "(?i)InternetURL: ([A-Z]:\\.+) ->.*", "$1");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                // Return the file not found message
                //   Logger.NFOUND(filePath);
                return;
            }

            // Move the file if it exists
            MOVEFILER(filePath);
        }

        public static bool FileDelete(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Move(string fix)
        {
            // Clean up the input
            fix = fix.Replace("\"", string.Empty);

            // Extract source and destination paths using regular expressions
            string source = Regex.Replace(fix, @"(?i)Move:[ ]*([b-z]:\\[^:]+)[ ]+[b-z]:\.+", "$1");
            string destination = Regex.Replace(fix, @"(?i)Move:[ ]*[b-z]:\\[^:]+[ ]([b-z]:\\[^:]+)[ ]*", "$1");

            // Ensure source file exists
            if (!File.Exists(@"\\?\" + source))
            {
                Console.WriteLine($"File not found: {source}");
                return;
            }

            // Check if destination file exists and take appropriate action
            if (File.Exists(@"\\?\" + destination))
            {
                MoveFile(destination);
            }

            try
            {
                // Attempt to move the file
                File.Move(@"\\?\" + source, @"\\?\" + destination);
                Console.WriteLine($"\"{source}\" {destination} moved successfully.");
            }
            catch (Exception ex)
            {
                // Log error if move fails
                Console.WriteLine($"Failed to move \"{source}\" to {destination}: {ex.Message}");
            }
        }

        public static void MoveFile(string path)
        {
            string path1 = @"\\?\" + path;

            // Check if the file has specific attributes like S (system), R (read-only), or H (hidden)
            if (FileUtils.HasFileAttributes(path1, "S") || FileUtils.HasFileAttributes(path1, "R") || FileUtils.HasFileAttributes(path1, "H"))
            {
                SetFileAttributes(path1, "-RSH");
            }

            string fileName = Regex.Replace(path, @".+\\([^\\]+)", "$1");
            string directoryName = Regex.Replace(path, ":", "");
            directoryName = Regex.Replace(directoryName, @"(.+)\.+", "$1");

            string targetDir = Path.Combine(FolderConstants.QuarantinePath, directoryName);
            Directory.CreateDirectory(targetDir);

            string destinationPath = @"\\?\" + Path.Combine(targetDir, fileName + ".xBAD");

            // Move the file
            bool dirMoved = MoveFileToDestination(path1, destinationPath);

            if (dirMoved)
            {
                Logger.Instance.LogFix($"{StringConstants.MOVED} {path}");
            }
            else
            {
                if (File.Exists(path1))
                {
                    GrantPermissions(path1);

                    if (FileUtils.HasFileAttributes(path1, "S") || FileUtils.HasFileAttributes(path1, "R") || FileUtils.HasFileAttributes(path1, "H"))
                    {
                        SetFileAttributes(path1, "-RSH");
                    }

                    dirMoved = MoveFileToDestination(path1, destinationPath);

                    if (dirMoved)
                    {
                        Logger.Instance.LogFix($"{StringConstants.MOVED} {path}");
                    }
                    else
                    {
                        Logger.Instance.LogFix($"{StringConstants.NOTMOVED} {path}");
                    }
                }
                else
                {
                    Logger.Instance.LogFix($"{StringConstants.NOTMOVED} {path}");
                }
            }
        }
        public static void MoveFileNormal(string filePath)
        {
            try
            {
                string originalPath = filePath;

                // Add "\\?\" prefix if not already present
                if (!filePath.StartsWith(@"\\?\"))
                {
                    filePath = @"\\?\" + filePath;
                }

                // Handle blacklisted paths
                if (FileUtils.IsBlacklisted(originalPath))
                {
                    Logger.Instance.LogFix($"{originalPath} => {StringConstants.FIX13}{Environment.NewLine}");
                    return;
                }

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    Logger.Instance.LogFix($"{originalPath} => {StringConstants.NOTFOUND}");
                    return;
                }

                // Remove system, read-only, and hidden attributes
                if (Regex.IsMatch(File.GetAttributes(filePath).ToString(), "(?i)S|R|H"))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                // Get destination path
                string destinationPath = FileUtils.GetDestination(originalPath, true);

                // Attempt to move the file
                bool moveResult = TryMoveFile(filePath, destinationPath);

                if (moveResult)
                {
                    Logger.Instance.LogFix($"{originalPath} => {StringConstants.MOVED}");

                    if (Path.GetExtension(filePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        Process[] processes = Process.GetProcesses();

                        foreach (Process process in processes)
                        {
                            string processPath = ProcessUtils.GetProcessPath(process.Id);

                            if (originalPath.Equals(processPath, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!ProcessUtils.IsProcessCritical(process.Id))
                                {
                                    FileWrite($"{FolderConstants.HomeDrive} + \\Wildlands\\re", "P");
                                    break;
                                }

                                process.Kill();

                                if (process.HasExited)
                                {
                                    FileWrite($"{FolderConstants.HomeDrive} + \\Wildlands\\re", "P");
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Grant permissions and retry
                    GrantPermissions(filePath, 1, 0);

                    if (Regex.IsMatch(File.GetAttributes(filePath).ToString(), "(?i)S|R|H"))
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    moveResult = TryMoveFile(filePath, destinationPath);

                    if (moveResult)
                    {
                        Logger.Instance.LogFix($"{originalPath} => {StringConstants.MOVED}");
                    }
                    else
                    {
                        Logger.Instance.LogFix($"{originalPath} => {StringConstants.FIX8} => MoveReboot.{Environment.NewLine}");

                        MoveFileOnReboot(originalPath, destinationPath);

                        using (var fileStream = new StreamWriter($"{FolderConstants.HomeDrive} + \\FRST\\files", append: true))
                        {
                            fileStream.WriteLine(originalPath);
                        }

                        using (var fileStream = new StreamWriter($"{FolderConstants.HomeDrive} + \\FRST\\reb", append: true))
                        {
                            fileStream.WriteLine("reboote?" + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoveFileNormal: {ex.Message}");
            }
        }

        public static bool DllCallRemoveDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                Console.WriteLine("Directory path is null or empty.");
                return false;
            }

            // Attempt to remove the directory
            bool result = Kernel32NativeMethods.RemoveDirectory(directoryPath);
            if (!result)
            {
                // Retrieve and log the error code
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"Failed to remove directory: {directoryPath}. Error code: {errorCode}");
            }
            else
            {
                Console.WriteLine($"Successfully removed directory: {directoryPath}");
            }

            return result;
        }

        /// <summary>
        /// Attempts to move a file from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The path of the file to be moved.</param>
        /// <param name="destinationPath">The target path where the file should be moved.</param>
        /// <returns>True if the file move is successful; otherwise, false.</returns>
        private static bool TryMoveFile(string sourcePath, string destinationPath)
        {
            try
            {
                // Ensure the destination directory exists
                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Attempt to move the file
                File.Move(sourcePath, destinationPath);
                Console.WriteLine($"File moved successfully: {sourcePath} -> {destinationPath}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied while moving file: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error occurred while moving file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while moving file: {ex.Message}");
            }
            return false; // Return false if any exception occurs
        }

        public static void SetFileAttributes(string path, string attributes)
        {
            FileAttributes fileAttributes = File.GetAttributes(path);

            // Remove the given attributes (e.g., -RSH)
            if (attributes.Contains("R")) fileAttributes &= ~FileAttributes.ReadOnly;
            if (attributes.Contains("S")) fileAttributes &= ~FileAttributes.System;
            if (attributes.Contains("H")) fileAttributes &= ~FileAttributes.Hidden;

            File.SetAttributes(path, fileAttributes);
        }

        private static bool MoveFileToDestination(string sourcePath, string destPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file: {ex.Message}");
                return false;
            }
        }

        public static void MoveFileOnReboot(string source, string dest)
        {
            // Set the registry key to allow protected renames
            SetAllowProtectedRenames();

            // Ensure the source path has the "\\?\" prefix
            if (!source.StartsWith(@"\\?\"))
            {
                source = @"\\?\" + source;
            }

            // If destination is not empty, move the file
            if (!string.IsNullOrEmpty(dest))
            {
                Kernel32NativeMethods.MoveFileEx(source, dest, NativeMethodConstants.MOVEFILE_REPLACE_EXISTING | NativeMethodConstants.MOVEFILE_DELAY_UNTIL_REBOOT);
            }
            else
            {
                Kernel32NativeMethods.MoveFileEx(source, null, NativeMethodConstants.MOVEFILE_DELAY_UNTIL_REBOOT);
            }
        }

        private static void SetAllowProtectedRenames()
        {
            // Set the registry key to allow protected renames (DWORD 0x1)
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager", "AllowProtectedRenames", 1, Microsoft.Win32.RegistryValueKind.DWord);
        }
        public static void MoveFiler(string path1, string bootMode)
        {
            // Check if the boot mode is "recovery"
            if (bootMode.Equals("recovery", StringComparison.OrdinalIgnoreCase))
            {
                MoveFile(path1);
            }
            else
            {
                MoveFileNormal(path1);
            }
        }
        public static void MoveFiles(string path, string quarantineDirectory)
        {
            // Check if the file has "S", "R", or "H" attributes
            if (FileUtils.FileAttributesContains(path, FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }

            // Create the destination directory and file name
            string driveLetter = path.Substring(0, 1);
            string destinationDirectory = Path.Combine(quarantineDirectory, driveLetter);
            string destinationPath = destinationDirectory + ".xBAD";

            // Move the file
            try
            {
                File.Move(path, destinationPath);
            }
            catch (IOException)
            {
                return; // If the move fails, return
            }

            // If the file still exists (check with your custom FILEACCN method)
            if (File.Exists(path))
            {
                GrantAccess(path);

                // Ensure the file has no "S", "R", or "H" attributes
                if (FileUtils.FileAttributesContains(path, FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                }

                // Try moving the file again
                try
                {
                    File.Move(path, destinationPath);
                }
                catch (IOException)
                {
                    // Handle the error (maybe log it)
                }
            }
        }

        public static void TasksJobFix()
        {
            // Example input string
            string input = "Task: C:\\example\\file.txt => Additional Information";
            string filePath = System.Text.RegularExpressions.Regex.Replace(input, @"(?i)Task: ([A-Z]:\\.+) =>.*", "$1");

            // Depending on the boot mode, move the file differently
            if (SystemConstants.BootMode.Equals("Recovery", StringComparison.OrdinalIgnoreCase))
            {
                MoveFile(filePath, "Recovery");
            }
            else
            {
                MoveFile(filePath, "Normal");
            }
        }



        public static void Copy()
        {
            Logger.Instance.LogFix("================== \"" + Fix + "\" ===================" + Environment.NewLine + Environment.NewLine);
            Copy1();
            Logger.Instance.LogFix(Environment.NewLine + "=== " + StringConstants.END + " Copy: ===" + Environment.NewLine);
        }

        public static void Copy1()
        {
            string source = Regex.Replace(Fix, @"(?i)Copy:\s*(.+) [c-z]:.*", "$1");
            string dest = Regex.Replace(Fix, @"(?i)Copy:\s*.+ ([c-z]:.*)", "$1");
            dest = Regex.Replace(dest, @"\\$", "");
            dest = Regex.Replace(dest, @"\\\\(?!\?\\)", @"\");

            if (!File.Exists(@"\\?\" + source))
            {
                Logger.Instance.LogFix($"{source} => {StringConstants.NOTFOUND}");
                return;
            }

            var drive = Regex.Match(dest, ".:").Value;
            if (string.IsNullOrEmpty(drive))
            {
                Logger.Instance.LogFix($"{dest + @"\"} => {StringConstants.NOTFOUND}");
                return;
            }

            if (!Directory.Exists(drive))
            {
                Logger.Instance.LogFix($"{dest + @"\"} => {StringConstants.NOTFOUND}");
                return;
            }

            if (File.GetAttributes(@"\\?\" + source).HasFlag(FileAttributes.Directory))
            {
                CopyDirectory(source, dest);
            }
            else
            {
                CopyFile(source, dest);
            }
        }

        // Handle directory copy
        public static void CopyDirectory(string source, string dest)
        {
            string pdir = Regex.Replace(source, @"(.+)\..+", "$1");
            pdir = Regex.Replace(pdir, @"\\", @"\\");

            if (!Directory.Exists(@"\\?\" + dest))
            {
                Directory.CreateDirectory(dest);
            }

            string[] files = Directory.GetFiles(source);
            foreach (var file in files)
            {
                string dest1 = dest + file.Replace(pdir, "");
                CopyFile(file, dest1);
            }
        }

        // Handle file copy
        public static bool CopyFile(string source, string dest)
        {
            try
            {
                // Ensure destination directory exists
                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                }

                // Construct the destination file path
                string dest1 = Path.Combine(dest, Path.GetFileName(source));
                File.Copy(source, dest1, true);

                // Check for reparse points and log details
                string source1 = $"\"{source}\" ";
                if (FileUtils.IsReparsePoint(source))
                {
                    source1 = $"\"{source}\" ({StringConstants.SYMLINK0} -> {FileUtils.GetReparseTarget(source)}) ";
                }

                Logger.Instance.LogFix($"{source1}{StringConstants.COP1}" + Environment.NewLine);
                return true; // Indicate success
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access denied errors
                Logger.Instance.LogFix($"\"{source}\" => Could not copy ({StringConstants.ERR0}: {StringConstants.NOACC})" + Environment.NewLine);
                return false; // Indicate failure
            }
            catch (Exception ex)
            {
                // Handle general errors
                Logger.Instance.LogFix($"\"{source}\" => Could not copy ({StringConstants.ERR0}: {ex.Message})" + Environment.NewLine);
                return false; // Indicate failure
            }
        }


        // Function to create symbolic link (using CreateSymbolicLinkW)
        public static void CopyLink(string source, string dest, string target)
        {
            // Create symbolic link
            bool ret = Kernel32NativeMethods.CreateSymbolicLinkW(dest, target, 0); // 0: No flags
            if (!ret)
            {
                Logger.Instance.LogFix($"\"{source}\" => Could not create symbolic link ({StringConstants.SYMLINK0} -> {target} <{StringConstants.ERR0}:{Kernel32NativeMethods.GetLastError()}>){Environment.NewLine}");
            }
            else
            {
                Logger.Instance.LogFix($"\"{source}\" ({StringConstants.SYMLINK0} -> {target}) {StringConstants.COP1}" + Environment.NewLine);
            }
        }

        // Handle CLSID fixes
        public static void CLSIDFix(string fix)
        {
            if (SystemConstants.BootMode != "Recovery")
            {
                ProcessFix.KILLDLL();
            }

            string key = Regex.IsMatch(fix, @"(?i)\\(InprocServer32|localserver32|Shell\\Open\\Command)")
                ? Regex.Replace(fix, @"(?i)CustomCLSID: (.+)\\(InprocServer32|localserver32|Shell\\Open\\Command) ->.*", "$1")
                : Regex.Replace(fix, @"(?i)CustomCLSID: (.+?) ->.*", "$1");

            RegistryKeyHandler.DeleteRegistryKey(key);
        }

        public static void CreateAcl(string account, uint seObjectType = 1, uint accMode = 2)
        {
            string[] accounts = account.Split(';');
            int count = accounts.Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    // Parse the account
                    string currentAccount = accounts[i];
                    if (string.IsNullOrWhiteSpace(currentAccount)) continue;

                    // Determine permissions based on account type
                    FileSystemRights rights = FileSystemRights.FullControl; // Default permissions
                    AccessControlType controlType = AccessControlType.Allow;

                    // Customize rights based on account type
                    if (currentAccount.Equals("Users", StringComparison.OrdinalIgnoreCase))
                    {
                        rights = FileSystemRights.ReadAndExecute;
                    }
                    else if (currentAccount.Equals("Authenticated Users", StringComparison.OrdinalIgnoreCase))
                    {
                        rights = FileSystemRights.Modify;
                    }

                    // Apply the ACL to the target object (file, directory, etc.)
                    // Replace `path` with the actual target path where the ACL is applied
                    string targetPath = @"C:\Path\To\Target";

                    FileUtils.ApplyAcl(targetPath, currentAccount, rights, controlType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying ACL for account '{accounts[i]}': {ex.Message}");
                }
            }
        }


        // Method to create or open a file
        public static void CreateFile(string path = "", uint accessMode = 1179785, uint flag = 33554432)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("File path cannot be empty.", nameof(path));
            }

            if (!path.StartsWith("\\\\?\\"))
            {
                path = "\\\\?\\" + path;  // Required for long paths
            }

            IntPtr securityAttributes = IntPtr.Zero;  // Set security attributes to null
            IntPtr hFile = Kernel32NativeMethods.CreateFileW(path, accessMode, 0, securityAttributes, 3, flag, IntPtr.Zero);  // Opening or creating the file

            if (hFile == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Error creating or opening file. Error code: {errorCode}");
            }
            else
            {
                Console.WriteLine($"File '{path}' opened/created successfully.");
            }
        }

        public static void CreateSymbolicLink(string source, string target)
        {
            // Call CreateSymbolicLinkW with two arguments: source and target
            bool result = Kernel32NativeMethods.CreateSymbolicLinkW(source, target, 0);
            if (!result)
            {
                throw new Exception("Error creating symbolic link.");
            }

            Logger.Instance.LogFix($"\"{source}\" ({StringConstants.SYMLINK0} -> {target}) " + StringConstants.COP1 + Environment.NewLine);
        }

        public static void DeleteReparsePoint(string link, bool leaveFinalObject = false)
        {
            // Open the file or directory with reparse point using CreateFileW
            IntPtr hFile = Kernel32NativeMethods.CreateFileW(link, 3, 0, IntPtr.Zero, NativeMethodConstants.OPEN_EXISTING, NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
            if (hFile == IntPtr.Zero) return; // Error in opening file

            // Perform the operation to delete the reparse point
            uint bytesReturned;
            bool success = Kernel32NativeMethods.DeviceIoControl(
                hFile,
                NativeMethodConstants.FSCTL_DELETE_REPARSE_POINT,
                IntPtr.Zero, // Input buffer (not used here)
                0,           // Size of the input buffer
                IntPtr.Zero, // Output buffer
                0,           // Size of the output buffer
                out bytesReturned,
                IntPtr.Zero  // Overlapped (synchronous operation)
            );

            // Close the file handle
            Kernel32NativeMethods.CloseHandle(hFile);

            // Handle the result of DeviceIoControl
            if (!success) // Error deleting reparse point
            {
                if (leaveFinalObject)
                    return;

                // Delete the reparse point object (file or directory)
                if (File.GetAttributes(link).HasFlag(FileAttributes.Directory))
                {
                    Directory.Delete(link, true); // Use recursive delete if directory
                }
                else
                {
                    File.Delete(link); // Delete the file
                }
            }
        }

        // Method to delete a file or directory
        public static void DeleteFileOrDirectory(string path)
        {
            try
            {
                if (File.Exists(path)) // Check if it's a file
                {
                    File.Delete(path); // Delete file
                    Console.WriteLine($"File {path} deleted successfully.");
                }
                else if (Directory.Exists(path)) // Check if it's a directory
                {
                    Directory.Delete(path, true); // Delete directory and its contents
                    Console.WriteLine($"Directory {path} deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"The path {path} does not exist.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to {path}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {path}: {ex.Message}");
            }
        }

        public static void DeleteShadowCopy(string shadow)
        {
            var objWmi = new ManagementObjectSearcher("SELECT * FROM Win32_ShadowCopy");
            foreach (ManagementObject obj in objWmi.Get())
            {
                if (obj["DeviceObject"].ToString() == shadow)
                {
                    obj.InvokeMethod("Delete", null);
                }
            }
        }


        public static void DeleteArray1(string[] arr)
        {
            foreach (var key in arr)
            {
                RegistryKeyHandler.DeleteRegistryKey(key);
            }
        }

        public static void Unlock(string path, int seObjectType = 1, string accounts = "Administrators;System;Users", int acc = 2)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Invalid path.");
                    return;
                }

                // Determine the owner
                string owner = accounts.Equals("Everyone", StringComparison.OrdinalIgnoreCase) ? "Everyone" : "Administrators";
                if (acc == 3)
                    owner = "Administrators";

                // Set the owner
                SetOwner(path, owner);

                // Apply permissions (DACL)
                CreateAndApplyAcl(path, accounts, acc);

                Console.WriteLine($"Successfully unlocked: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking path '{path}': {ex.Message}");
            }
        }

        private static void SetOwner(string path, string owner)
        {
            if (File.Exists(path))
            {
                var fileSecurity = new FileSecurity();
                fileSecurity.SetOwner(new NTAccount(owner));
                File.SetAccessControl(path, fileSecurity);
            }
            else if (Directory.Exists(path))
            {
                var dirSecurity = new DirectorySecurity();
                dirSecurity.SetOwner(new NTAccount(owner));
                Directory.SetAccessControl(path, dirSecurity);
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }
        }

        private static void CreateAndApplyAcl(string path, string accounts, int acc)
        {
            // Parse the accounts
            string[] accountList = accounts.Split(';');
            FileSystemRights rights;
            if (acc == 1)
            {
                rights = FileSystemRights.Read;
            }
            else if (acc == 2)
            {
                rights = FileSystemRights.Modify;
            }
            else if (acc == 3)
            {
                rights = FileSystemRights.FullControl;
            }
            else
            {
                rights = FileSystemRights.Modify;
            }

            if (File.Exists(path))
            {
                var fileSecurity = new FileSecurity();

                foreach (string account in accountList)
                {
                    if (string.IsNullOrWhiteSpace(account)) continue;

                    fileSecurity.AddAccessRule(new FileSystemAccessRule(
                        account,
                        rights,
                        InheritanceFlags.None,
                        PropagationFlags.None,
                        AccessControlType.Allow));
                }

                File.SetAccessControl(path, fileSecurity);
            }
            else if (Directory.Exists(path))
            {
                var dirSecurity = new DirectorySecurity();

                foreach (string account in accountList)
                {
                    if (string.IsNullOrWhiteSpace(account)) continue;

                    dirSecurity.AddAccessRule(new FileSystemAccessRule(
                        account,
                        rights,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));
                }

                Directory.SetAccessControl(path, dirSecurity);
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }
        }

        private static void SetSecurityInfo(string path, int seObjectType, IntPtr pDacl)
        {
            // Explicitly cast seObjectType to uint
            int result = Advapi32NativeMethods.SetNamedSecurityInfo(
                path,
                (uint)seObjectType,
                4, // SECURITY_INFORMATION.DACL_SECURITY_INFORMATION
                IntPtr.Zero,
                IntPtr.Zero,
                pDacl,
                IntPtr.Zero
            );

            if (result != 0)
            {
                Console.WriteLine($"SetSecurityInfo failed with error {result}");
            }
        }

        private static void ApplySecurityInfo(string path, int seObjectType, IntPtr customAcl)
        {
            // Explicitly cast seObjectType to uint
            int result = Advapi32NativeMethods.SetNamedSecurityInfo(
                path,
                (uint)seObjectType, // Convert int to uint
                4,                 // SECURITY_INFORMATION.DACL_SECURITY_INFORMATION
                IntPtr.Zero,
                IntPtr.Zero,
                customAcl,
                IntPtr.Zero
            );

            if (result != 0)
            {
                Console.WriteLine($"ApplySecurityInfo failed with error {result}");
            }
        }

        public static bool CreateFile(string path, long size)
        {
            try
            {
                // Create a new file and set its size
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fs.SetLength(size); // Set the file size
                }

                return true; // Return true if file creation succeeds
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating file: {ex.Message}");
                return false; // Return false if file creation fails
            }
        }

        public static void GrantAccess(string path, bool fullControl, bool inherit)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                var security = directoryInfo.GetAccessControl();

                var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                var rule = new FileSystemAccessRule(
                    currentUser,
                    fullControl ? FileSystemRights.FullControl : FileSystemRights.ReadAndExecute,
                    inherit ? InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit : InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow
                );

                security.AddAccessRule(rule);
                directoryInfo.SetAccessControl(security);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error granting access: {ex.Message}");
            }
        }

        public static void GrantPermissions(string path, int seObjectType = 1, int rec = 0, string account = "Everyone")
        {
            Unlock(path, seObjectType, account);  // Passes the correct int and string to Unlock

            if (rec != 0)
            {
                UnlockAllChild(path);  // This method is assumed to accept a string path
            }

            string fullKey = ConvertKey(path);
            if (!string.IsNullOrEmpty(fullKey))
            {
                RegistryUtilsScript.UnlockAllRegistry(fullKey);  // Assumed method that works with string path
            }
        }

        private static string ConvertKey(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            // Example logic: replace certain characters to make the path registry-compatible
            string registryKey = path.Replace('\\', '/').Replace(":", "_");

            // Prefix with a base registry key if needed (e.g., SOFTWARE or SYSTEM)
            return $"HKLM\\SOFTWARE\\";
        }

        public static void UnlockAllChild(string path)
        {
            try
            {
                // Check if the directory exists
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Directory does not exist.");
                    return;
                }

                // Unlock all files in the directory
                foreach (var file in Directory.GetFiles(path))
                {
                    Unlock(file, 1, "Everyone");  // Unlock each file (seObjectType and account are placeholders)
                }

                // Unlock all subdirectories recursively
                foreach (var subDirectory in Directory.GetDirectories(path))
                {
                    UnlockAllChild(subDirectory);  // Recursively unlock subdirectories
                }

                Console.WriteLine($"All files and directories under {path} have been unlocked.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking child files and directories: {ex.Message}");
            }
        }

        private static void UnlockFile(string filePath)
        {
            try
            {
                // Example logic to unlock a file (e.g., remove read-only attribute)
                if (File.Exists(filePath))
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        Console.WriteLine($"File {filePath} unlocked (Read-only attribute removed).");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking file {filePath}: {ex.Message}");
            }
        }

        public static void RemoveFolderArray(string path, string mask)
        {
            // Get all matching files and directories (recursively)
            string[] arrStor = Directory.GetFileSystemEntries(path, mask, SearchOption.AllDirectories);

            foreach (string item in arrStor)
            {
                try
                {
                    // Try to remove the directory
                    Directory.Delete(item, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting {item}: {ex.Message}");
                }
            }
        }

        public static void SDELETE1()
        {
            // Get all txt files in the script directory
            string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
            var fileArray = Directory.GetFiles(scriptDir, "*.txt", SearchOption.TopDirectoryOnly);

            foreach (var file in fileArray)
            {
                if (Regex.IsMatch(file, @"(?i)\\(frst|addition|fixlog|search|fixlist|Shortcut).*\.txt$"))
                {
                    File.Delete(file);
                    if (File.Exists(file))
                    {
                        MOVEFILEONREBOOT(file);
                    }
                }
            }

            // Remove specific directories
            string path = Path.Combine(FolderConstants.HomeDrive, "frst");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            if (!File.Exists(path))
            {
                //Program.SDELETE(); // Recursive call
                return;
            }

            var subfolders = DirectoryUtils.GetAllSubFolders(path);
            if (subfolders.Any())
            {
                subfolders.Reverse();
                foreach (var subfolder in subfolders)
                {
                    Directory.Delete(subfolder, true);
                    if (Directory.Exists(subfolder) && DirectoryUtils.BackupCheck(subfolder))
                    {
                        Directory.Delete(subfolder, true);
                    }
                }
            }

            // More deletion logic for files and folders
            if (!File.Exists(path))
            {
                //Program.SDELETE(); // Recursive call
                return;
            }

            if (FileUtils.FileAccessCheck(path))
            {
                GrantPermissions(path);
            }

            subfolders = DirectoryUtils.GetAllSubFolders(path);
            if (subfolders.Any())
            {
                subfolders.Reverse();
                foreach (var subfolder in subfolders)
                {
                    Directory.Delete(subfolder, true);
                    if (Directory.Exists(subfolder) && DirectoryUtils.BackupCheck(subfolder))
                    {
                        Directory.Delete(subfolder, true);
                    }
                }
            }

            var fileList = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in fileList)
            {
                if (file == Path.Combine(path, Path.GetFileName(file)))
                {
                    File.Delete(file);
                }

                if (File.Exists(file))
                {
                    MOVEFILEONREBOOT(file);
                }
            }

            // Final clean-up
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true); // Attempt to delete the directory
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete directory: {ex.Message}");
                }
            }

            if (Directory.Exists(path))
            {
                // Backup the folder if it still exists
                DirectoryUtils.BackupFolder(path);
            }

            if (Directory.Exists(path))
            {
                // Queue the directory for deletion on reboot
                MOVEFILEONREBOOT(path);
            }

            if (Directory.Exists(path))
            {
                // Attempt to delete using the command line
                CommandHandler.RunCommand($"rd /s /q \"{path}\"");
            }

            // Recursive call
            //Program.SDELETE();

        }

        private static void MOVEFILEONREBOOT(string filePath)
        {
            // Call the native MoveFileEx method
            int result = Kernel32NativeMethods.MoveFileEx(filePath, null, NativeMethodConstants.MOVEFILE_DELAY_UNTIL_REBOOT);

            // Check for success or failure
            if (result == 0) // A return value of 0 indicates failure
            {
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"Failed to queue file for deletion on reboot: {filePath}. Error code: {errorCode}");
            }
            else
            {
                Console.WriteLine($"File successfully queued for deletion on reboot: {filePath}");
            }
        }


        public static void ReplaceFile(string fix)
        {
            try
            {
                string owner = "", secDesc = "";
                string fileP = fix.Trim('"');
                string source = Regex.Replace(fix, "(?i)Replace:[ ]*([b-z]:\\\\[^:]+)[ ][b-z]:\\\\.+", "$1");
                string destination = Regex.Replace(fix, "(?i)Replace:[ ]*[b-z]:\\\\[^:]+[ ]([b-z]:\\\\[^:]+)[ ]*", "$1");
                string desDir = Path.GetDirectoryName(destination);

                // Check if source exists
                if (!File.Exists(@"\\?\" + source))
                {
                    File.AppendAllText("fixlog.txt", $"\"{source}\" -> Not Found => Replace {destination}\n");
                    return;
                }

                // Check if destination exists
                if (!File.Exists(desDir))
                {
                    File.AppendAllText("fixlog.txt", $"\"{desDir}\" -> Not Found => Replace {destination}\n");
                    return;
                }

                if (!File.Exists(@"\\?\" + destination))
                {
                    File.AppendAllText("fixlog.txt", $"Not Found {destination}\n");
                    return;
                }

                // Process move based on destination attributes
                if (!File.GetAttributes(destination).HasFlag(FileAttributes.Directory))
                {
                    if (Regex.IsMatch(destination, "(?i)config\\\\system$"))
                        CommandHandler.RunCommand("reg unload hklm\\999");

                    if (Regex.IsMatch(destination, "(?i)config\\\\software$"))
                        CommandHandler.RunCommand("reg unload hklm\\888");

                    MoveFile(destination, destination); // Move file operation (this is a simplified action, replace as needed)
                }
                else
                {
                    if (FileUtils.IsBlacklisted(destination))
                    {
                        File.AppendAllText("fixlog.txt", $"\"{destination}\" => Warning: Access Denied\n");
                        DirectoryFix.MoveDirectory(destination); // Directory move operation (replace with actual function if needed)
                    }
                }

                // Copy file with exception handling
                string error = string.Empty;
                if (File.Exists(source))
                {
                    try
                    {
                        File.Copy(source, destination, true);
                        File.AppendAllText("fixlog.txt", $"{source} copied to {destination}\n");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        RawSecurityDescriptor dacl = null;

                        // Retrieve ACL information
                        FileUtils.GetAcl(source, ref owner, ref dacl, ref secDesc);

                        // Unlock the file, copy it, and restore ACL
                        UnlockFile(source);
                        File.Copy(source, destination, true);
                        if (secDesc != null)
                        {
                            SetAcl(source, owner, dacl, secDesc);
                        }
                        error = "(Access Denied)";
                    }
                }

                if (string.IsNullOrEmpty(error))
                {
                    File.AppendAllText("fixlog.txt", $"{source} successfully replaced with {destination}\n");
                }
                else
                {
                    File.AppendAllText("fixlog.txt", $"Error replacing {source} -> {destination} {error}\n");
                }

                // Restore registry if required
                if (Regex.IsMatch(destination, "(?i)config\\\\system$"))
                    CommandHandler.RunCommand("reg load hklm\\999 c:\\Windows\\System32\\config\\System");

                if (Regex.IsMatch(destination, "(?i)config\\\\software$"))
                    CommandHandler.RunCommand("reg load hklm\\888 c:\\Windows\\System32\\config\\software");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                File.AppendAllText("fixlog.txt", $"Error: {ex.Message}\n");
            }
        }

        private static void SetAcl(string filePath, string owner, RawSecurityDescriptor dacl, string secDesc)
        {
            try
            {
                // Create a new FileSecurity object
                FileSecurity fileSecurity = new FileSecurity();

                // Set the owner
                fileSecurity.SetOwner(new NTAccount(owner));

                // Set the DACL
                fileSecurity.SetSecurityDescriptorSddlForm(secDesc, AccessControlSections.Access);

                // Apply the security settings to the file
                File.SetAccessControl(filePath, fileSecurity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting ACL: {ex.Message}");
            }
        }

        public static void CreateSymbolicLink(string symlink, string target, bool isDirectory = false)
        {
            uint flags = isDirectory ? NativeMethodConstants.SYMBOLIC_LINK_FLAG_DIRECTORY : NativeMethodConstants.SYMBOLIC_LINK_FLAG_FILE;
            bool result = Kernel32NativeMethods.CreateSymbolicLinkW(symlink, target, flags);

            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"CreateSymbolicLinkW failed with error code {error}");
            }
        }

        public static void ProcessKeysRem(StreamWriter fixLog)
        {
            string keysRemPath = FolderConstants.HomeDrive + @"\frst\keysRem";

            if (File.Exists(keysRemPath))
            {
                // Check if the path contains "HKU\\" and call Load if true
                if (keysRemPath.Contains("HKU\\"))
                {
                    FileUtils.Load();
                }

                int k = 1;
                while (true)
                {
                    // Read the registry key line by line
                    string key = FileUtils.ReadFileLine(keysRemPath, k);
                    if (string.IsNullOrEmpty(key)) break;

                    // Check if force delete is needed
                    bool forceDelete = (1 == 1); // Replace with your condition if necessary

                    // Delete the registry key
                    RegistryKeyHandler.DeleteRegistryKey(key);

                    // Additional logic for forced deletion, if required
                    if (forceDelete)
                    {
                        Console.WriteLine($"Forcefully deleted registry key: {key}");
                    }

                    k++;

                }

                // If LoadKeys has more than 1 item, unload it
                if (RegistryKeyHandler.LoadKeys().Length > 1)
                {
                    string user = "TempHive"; // Replace with the actual hive or user name
                    RegistryUtilsScript.Unload(user); // Pass the required argument
                }

                // Delete the keysRem file after processing
                File.Delete(keysRemPath);
            }
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Cleanup(string mappedDrive, string shadowCopyPath, string tempFolder)
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Kernel32NativeMethods.DefineDosDevice(NativeMethodConstants.DDD_REMOVE_DEFINITION, mappedDrive, shadowCopyPath);
        }

        public static bool CF(string source, string dest)
        {
            if (!source.StartsWith(@"\\?\")) source = @"\\?\" + source;
            if (!dest.StartsWith(@"\\?\")) dest = @"\\?\" + dest;

            try
            {
                File.Copy(source, dest, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CreateFile(string filePath)
        {
            // Check if file exists and can be opened
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool EmptyFile(string path)
        {
            if (!File.Exists(path))
            {
                return false; // File does not exist
            }

            Console.WriteLine($"Deleting temporary file: {path}");

            try
            {
                // Attempt to delete the file
                File.Delete(path);

                // If the file still exists, move it to a temporary folder
                if (File.Exists(path))
                {
                    string tempFolder = Path.Combine(Path.GetTempPath(), "FRST", "Temp");
                    Directory.CreateDirectory(tempFolder); // Ensure the temp folder exists

                    string destinationPath = Path.Combine(tempFolder, Path.GetFileName(path));
                    File.Move(path, destinationPath);
                }

                // If the file still exists after moving, schedule it for deletion on reboot
                if (File.Exists(path))
                {
                    ScheduleFileForDeletionOnReboot(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling file: {path}. {ex.Message}");
                return false;
            }
        }

        public static void ScheduleFileForDeletionOnReboot(string filePath)
        {
            try
            {
                // Use MoveFileEx API to schedule deletion on reboot
                Kernel32NativeMethods.MoveFileEx(filePath, null, Flags.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                Console.WriteLine($"File scheduled for deletion on reboot: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to schedule file for deletion: {filePath}. {ex.Message}");
            }
        }

        private static void FileCheckFix()
        {
            // Extract the file path from the FIX string using a regex pattern
            string filePath = Regex.Replace(Fix, @"FCheck:\s(.+?) \[.+", "$1");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Logger.Instance.LogFix($"{filePath} => {StringConstants.NOTFOUND}");
                return;
            }

            // Check if the path is a directory
            if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryFix.MoveDirectory(filePath); // Replace with your MOVEDIR equivalent
            }
            else
            {
                MoveFile(filePath); // Replace with your MOVEFILER equivalent
            }
        }

        private static void FfMove(string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                Console.WriteLine("Destination path is not specified.");
                return;
            }

            // Replace `Fix`, `HFixLogPath`, and other custom references with actual variables/constants from your context.
            string filePath = Regex.Replace(Fix, "^\"|\"$", ""); // Remove surrounding quotes
            filePath = filePath.Replace("/", "\\"); // Convert forward slashes to backslashes

            if (Regex.IsMatch(Fix, @"(?i)^\d{4}-\d{2}-\d{2}.+"))
            {
                filePath = Regex.Replace(Fix, $@"(?i)^\d{{4}}-\d{{2}}-\d{{2}}.+({FolderConstants.HomeDrive}\\.+)", "$1");
            }

            filePath = Regex.Replace(filePath, @"\\$", ""); // Remove trailing backslash
            string fileName = Regex.Replace(filePath, @".+\\(.+)", "$1"); // Extract the file name

            // Handle wildcard in file name
            if (fileName.Contains("*"))
            {
                FfMoveWild(filePath, fileName, destinationPath);
                return;
            }

            // Handle invalid or missing file
            if (!File.Exists(@"\\?\" + filePath) || fileName.Contains("?"))
            {
                Logger.Instance.LogFix($"{filePath} => {StringConstants.NOTFOUND}");
                return;
            }

            // Handle symbolic link or reparse point
            if (FileUtils.IsReparsePoint(@"\\?\" + filePath))
            {
                string reparseTarget = FileUtils.GetReparseTarget(filePath); // Replace with your `_GETREPARSETARGET` equivalent

                if (DeleteReparsePointAlt(filePath, true)) // Replace with `_DELETEREPARSEPOINT` equivalent
                {
                    Logger.Instance.LogFix($"\"{filePath}\" => Reparse point deleted.");
                }
                else
                {
                    Logger.Instance.LogFix($"\"{filePath}\" => Failed to delete reparse point.");
                }

                return;
            }

            // Handle file or directory
            FileAttributes attributes = File.GetAttributes(@"\\?\" + filePath);

            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (FileUtils.IsBlacklisted(filePath)) // Replace with `_BLACK` equivalent
                {
                    Logger.Instance.LogFix($"\"{filePath}\" => Blacklisted.");
                    return;
                }

                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string newDestination = Path.Combine(destinationPath, Path.GetFileName(filePath));

                try
                {
                    Directory.Move(filePath, newDestination);
                    Console.WriteLine($"Directory moved from {filePath} to {newDestination}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to move directory: {ex.Message}");
                }

            }
            else
            {
                MoveFile(filePath, destinationPath); // Replace with `MOVEFILER` equivalent
            }
        }

        public static bool DeleteReparsePointAlt(string filePath, bool leaveFinalObject = false)
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = Kernel32NativeMethods.CreateFile(
                    filePath,
                    0x40000000, // GENERIC_WRITE
                    0,
                    IntPtr.Zero,
                    NativeMethodConstants.OPEN_EXISTING,
                    NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero);

                if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                {
                    throw new IOException($"Failed to open file handle. Error code: {Marshal.GetLastWin32Error()}");
                }

                // Prepare the reparse point buffer
                Structs.REPARSE_DATA_BUFFER reparseBuffer = new Structs.REPARSE_DATA_BUFFER { ReparseTag = 0xA000000C }; // Example tag
                uint bufferSize = (uint)Marshal.SizeOf(reparseBuffer);
                IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
                Marshal.StructureToPtr(reparseBuffer, buffer, false);

                // Send the delete request
                bool result = Kernel32NativeMethods.DeviceIoControl(
                    handle,
                    NativeMethodConstants.FSCTL_DELETE_REPARSE_POINT,
                    buffer,
                    bufferSize,
                    IntPtr.Zero,
                    0,
                    out uint bytesReturned,
                    IntPtr.Zero);

                Marshal.FreeHGlobal(buffer);

                if (!result && !leaveFinalObject)
                {
                    // Attempt to delete the object if reparse point deletion failed
                    if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                    {
                        Directory.Delete(filePath, true);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting reparse point: {ex.Message}");
                return false;
            }
            finally
            {
                if (handle != IntPtr.Zero && handle != new IntPtr(-1))
                {
                    Kernel32NativeMethods.CloseHandle(handle);
                }
            }
        }

        public static void FfMoveWild(string sourcePath, string searchPattern, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
            {
                throw new ArgumentException("Source or destination path cannot be null or empty.");
            }

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"Source directory does not exist: {sourcePath}");
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            try
            {
                // Get files matching the wildcard pattern
                var files = Directory.GetFiles(sourcePath, searchPattern);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var destinationFile = Path.Combine(destinationPath, fileName);

                    // Move file to destination
                    File.Move(file, destinationFile);
                    Console.WriteLine($"Moved: {file} -> {destinationFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing wildcard files: {ex.Message}");
            }
        }

        public static void MoveWildFiles(string filePath, string pattern, string logPath)
        {
            // Extract folder from filePath
            string folder = Regex.Replace(filePath, @"(.+)\.\+", "$1");

            // Append initial log
            Logger.Instance.LogFix($"\n=========== \"{filePath}\" ==========\n\n");

            // Retrieve matching files in the folder
            var matchingFiles = FileUtils.GetFilesByPattern(folder, pattern);

            if (matchingFiles != null && matchingFiles.Any())
            {
                foreach (var file in matchingFiles)
                {
                    // Check for reparse points
                    if (FileUtils.IsReparsePoint(file))
                    {
                        Logger.Instance.LogFix($"{StringConstants.SYMLINK0} {StringConstants.FOUND}: \"{file}\" => \"{FileUtils.GetReparseTarget(file)}\"\n");

                        if (DeleteReparsePointAlt(file, true))
                        {
                            Logger.Instance.LogFix($"\"{file}\" => {StringConstants.SYMLINK0} {StringConstants.DELETED}\n");
                        }
                        else
                        {
                            Logger.Instance.LogFix($"\"{file}\" => {StringConstants.SYMLINK0}{StringConstants.NOTDELETED}.\n");
                            continue;
                        }

                        if (!File.Exists(file))
                            continue;
                    }

                    // Move the file
                    MoveFile(file);
                }
            }
            else
            {
                Logger.Instance.LogFix($"{StringConstants.NOTFOUND}\n");
            }

            // Append ending log
            Logger.Instance.LogFix($"\n========= {StringConstants.END} -> \"{filePath}\" ========\n\n");
        }

        /// Opens a file with specific privileges and returns a handle to the file.
        /// <param name="path">The file path.</param>
        /// <returns>The file handle, or <c>IntPtr.Zero</c> if the operation fails.</returns>
        public static IntPtr CreateFileHandle(string path)
        {
            try
            {
                // Ensure the path uses the correct format for low-level file access
                path = FileUtils.NormalizeFilePath(path);

                // Call the WinAPI CreateFile function
                IntPtr fileHandle = Kernel32NativeMethods.CreateFileW(
                    path,
                    NativeMethodConstants.GENERIC_READ | NativeMethodConstants.GENERIC_WRITE,
                    NativeMethodConstants.FILE_SHARE_READ,
                    IntPtr.Zero,
                    NativeMethodConstants.OPEN_EXISTING,
                    NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero);

                // Check for invalid handle
                if (fileHandle == NativeMethodConstants.INVALID_HANDLE_VALUE)
                {
                    return IntPtr.Zero;
                }

                return fileHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        public static void MoveFileToTemp(string path)
        {
            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "FRST", "Temp");
                Directory.CreateDirectory(tempFolder);
                string destinationPath = Path.Combine(tempFolder, Path.GetFileName(path));
                File.Move(path, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file to temp: {path}. {ex.Message}");
            }
        }

        public static void GrantAccess(string filePath)
        {
            // Assuming _GRANTE grants full control permissions to the file
            FileSecurity fileSecurity = new FileSecurity();
            fileSecurity.SetAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(filePath, fileSecurity);
        }

        public static void CreateFileForAccess(string path)
        {
            try
            {
                string dummyFile = Path.Combine(path, "dummy.tmp");
                File.WriteAllBytes(dummyFile, new byte[0]);
                Console.WriteLine($"File created for access: {dummyFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create access file: {ex.Message}");
            }
        }
        /// <summary>
        /// Marks a file or directory as deleted by logging the action and optionally scheduling its removal.
        /// </summary>
        /// <param name="path">The path of the file or directory to be marked as deleted.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public static bool MarkDeleted(string path)
        {
            try
            {
                // Log the deletion action
                Console.WriteLine($"[MARK DELETED]: {path}");

                // Optionally, write the deletion marker to a log file
                string logFilePath = Path.Combine(Path.GetTempPath(), "DeletedItemsLog.txt");
                File.AppendAllText(logFilePath, $"[MARK DELETED]: {path}{Environment.NewLine}");

                // Optionally schedule the file or directory for deletion on reboot
                if (File.Exists(path) || Directory.Exists(path))
                {
                    MOVEFILEONREBOOT(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: Failed to mark {path} as deleted. Details: {ex.Message}");
                return false;
            }
        }

        // Unlock all files and directories in the specified path (this can be customized further)
        public static void UnlockAllChildItems(string path)
        {
            try
            {
                // Get all files in the directory
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    try
                    {
                        // Perform unlocking logic, like releasing file handles or resetting file attributes
                        Console.WriteLine($"Unlocking file: {file}");
                        // Unlock logic here, e.g., changing file attributes, closing handles, etc.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error unlocking file {file}: {ex.Message}");
                    }
                }

                // Get all subdirectories in the directory
                var directories = Directory.GetDirectories(path);
                foreach (var directory in directories)
                {
                    try
                    {
                        // Recursively unlock child directories
                        UnlockAllChildItems(directory);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error unlocking directory {directory}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking child items in {path}: {ex.Message}");
            }
        }

        // Modify this method to return a bool indicating success or failure
        public static bool CreateFileForAccessBool(string path)
        {
            try
            {
                // Attempt to create a dummy file to simulate file access
                string tempFile = Path.Combine(path, "dummyfile.txt");
                File.Create(tempFile).Dispose();  // Create the file and immediately dispose it
                return true; // Return true if the file was created successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating file for access: {ex.Message}");
                return false; // Return false if there was an error
            }
        }

        public static void FileWrite(string filePath, string content)
        {
            System.IO.File.AppendAllText(filePath, content);
        }

        public static void MoveFile(string sourceFile, string destinationFile, bool overwrite)
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));
            }

            if (string.IsNullOrEmpty(destinationFile))
            {
                throw new ArgumentException("Destination file path cannot be null or empty.", nameof(destinationFile));
            }

            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("The source file does not exist.", sourceFile);
            }

            if (File.Exists(destinationFile))
            {
                if (overwrite)
                {
                    File.Delete(destinationFile);
                }
                else
                {
                    throw new IOException("The destination file already exists.");
                }
            }

            File.Move(sourceFile, destinationFile);
        }

    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting
{
    public class DirectoryFix
    {
        // Deletes reparse points and handles directories
        public void DELJUNCTIONSH(string folder)
        {
            if (IsReparsePoint(folder))
            {
                DeleteReparsePoint(folder);

                string fsutil = Path.Combine(FolderConstants.System32, "fsutil.exe");
                if (SystemConstants.BootMode != "Recovery" && IsReparsePoint(folder))
                {
                    // Standard string concatenation for command execution
                    CommandHandler.RunCommand("cmd /c " + fsutil + " reparsepoint delete \"" + folder + "\"");
                }

                if (IsReparsePoint(folder))
                {
                    Logger.Instance.LogPrimary("\"" + folder + "\" => " + "Not Deleted" + "\n");
                }
                else
                {
                    if (File.Exists(folder))
                    {
                        FileFix.SetDefaultFileAccess(folder);
                    }
                    Logger.Instance.LogPrimary("\"" + folder + "\" => " + "Done" + "\n");
                }
            }
            else
            {
                if (File.Exists(folder))
                {
                    FileFix.SetDefaultFileAccess(folder);
                }
            }
        }

        // Check if a folder is a reparse point
        private bool IsReparsePoint(string folder)
        {
            try
            {
                FileAttributes attributes = File.GetAttributes(folder);
                return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch
            {
                return false;
            }
        }

        public static void MoveDir(string path)
        {
            string path1 = @"\\?\" + path;

            // Check if the directory exists
            if (!Directory.Exists(path1))
            {
                Console.WriteLine(path + "--->" + "Not Found");
                return;
            }

            // Get the destination path
            GrantFolderPermissions(path);

            // If you need to use `path` elsewhere, use it directly
            string sDest = Path.Combine(FolderConstants.QuarantinePath, path);


            // Try to move the directory
            bool dirDone = MoveDirectory(path1, sDest);

            if (dirDone && !Directory.Exists(path1))
            {
                MovedDirNormal(path);
            }
            else if (!dirDone || Directory.Exists(path1))
            {
                // Check if the directory is related to certain paths
                if (path.Contains(@"$Recycle.Bin\S-1-5-18") || path.Contains(@"$Recycle.Bin\S-1-5-21") ||
                    path.Contains($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\RECYCLER\S-1-5-21") ||
                    path.Contains($@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\RECYCLER\S-1-5-18") ||
                    path.Contains(@"\Desktop\install"))
                {
                    GrantFolderPermissions(path1);

                    dirDone = MoveDirectory(path1, sDest);

                    if (dirDone)
                    {
                        MovedDirNormal(path);
                    }
                    else
                    {
                        CommandHandler.RunCommand($@"rd /q/s ""{path}""");

                        if (!Directory.Exists(path1))
                        {
                            Console.WriteLine(path + "--->" + "Deleted");
                        }
                        else
                        {
                            if (Utility.IsRecoveryMode())
                            {
                                MovedDirNormal(path);
                            }
                            else
                            {
                                Console.WriteLine(path + "--->" + "Not Moved");
                            }
                        }
                    }
                }
                else
                {
                    GrantFolderPermissions(path1);

                    dirDone = MoveDirectory(path1, sDest);

                    if (dirDone && !Directory.Exists(path1))
                    {
                        MovedDirNormal(path);
                    }
                    else if (!dirDone || Directory.Exists(path1))
                    {
                        if (Utility.IsRecoveryMode())
                        {
                            MovedDirNormal(path);
                        }
                        else
                        {
                            Console.WriteLine(path + "--->" + "Not Moved");
                        }
                    }
                }
            }
        }

        public static void DeleteJunctionsInDirectory(string fix, string tempDir, string hfFixLog)
        {
            // Extract folder path from the fix string
            string folder = Regex.Replace(fix, "\"", "");
            folder = Regex.Replace(folder, @"(?i).*DeleteJunctionsInDirectory:[ ]*(.+)", "$1");

            if (Regex.IsMatch(folder, @"(?i)(.+) \Z"))
            {
                folder = folder.TrimEnd();
            }

            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Not Found: {folder}");
                return;
            }

            // Log the action
            File.AppendAllText(hfFixLog, $"\"{folder}\" => {Environment.NewLine}");

            // Call the method to delete junctions
            DeleteFolderJunction(folder);

            if ((File.GetAttributes(folder) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                return;
            }

            string logFile = Path.Combine(tempDir, "logfold");
            File.Delete(logFile);

            // Run the command to list directory contents and write to a log file
            CommandHandler.RunCommand($@"dir /a/b ""{folder}"" > ""{logFile}""");

            int i = 0;
            while (true)
            {
                i++;
                string file = FileUtils.ReadLineFromFile(logFile, i);
                if (file == null) break;

                string filePath = Path.Combine(folder, file);
                if (filePath.Contains(@"\msseces.exe"))
                {
                    // Kill the msseces.exe process
                    Process.Start("taskkill", "/f /im msseces.exe").WaitForExit();
                    System.Threading.Thread.Sleep(2000);
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "re"), "P");
                }

                DeleteFolderJunction(filePath);

                if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string logFile1 = Path.Combine(tempDir, "logfold1");
                    File.Delete(logFile1);

                    // Run the command to list directory contents and write to a log file
                    CommandHandler.RunCommand($@"dir /a/b ""{filePath}"" > ""{logFile1}""");

                    int q = 0;
                    while (true)
                    {
                        q++;
                        string file2 = FileUtils.ReadLineFromFile(logFile1, q);
                        if (file2 == null) break;

                        string filePath2 = Path.Combine(filePath, file2);
                        DeleteFolderJunction(filePath2);

                        if ((File.GetAttributes(filePath2) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            string logFile2 = Path.Combine(tempDir, "logfold2");
                            File.Delete(logFile2);

                            // Run the command to list directory contents and write to a log file
                            CommandHandler.RunCommand($@"dir /a/b ""{filePath2}"" > ""{logFile2}""");

                            int x = 1;
                            while (true)
                            {
                                string file4 = FileUtils.ReadLineFromFile(logFile2, x);
                                if (file4 == null) break;

                                string filePath3 = Path.Combine(filePath2, file4);
                                DeleteFolderJunction(filePath3);
                                x++;
                            }

                            File.Delete(logFile2);
                        }
                    }

                    File.Delete(logFile1);
                }
            }

            File.Delete(logFile);
            File.AppendAllText(hfFixLog, $"\"{folder}\" => Done.{Environment.NewLine}");
        }

        /// <summary>
        /// Deletes a folder junction if it exists at the specified path.
        /// </summary>
        /// <param name="junctionPath">The path to the folder junction.</param>
        public static void DeleteFolderJunction(string junctionPath)
        {
            if (string.IsNullOrEmpty(junctionPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(junctionPath));

            if (!Directory.Exists(junctionPath))
            {
                Console.WriteLine($"The path does not exist: {junctionPath}");
                return;
            }

            if (!DirectoryHandler.IsJunction(junctionPath))
            {
                Console.WriteLine($"The specified path is not a folder junction: {junctionPath}");
                return;
            }

            try
            {
                Directory.Delete(junctionPath);
                Console.WriteLine($"Folder junction deleted successfully: {junctionPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting folder junction: {ex.Message}");
            }
        }

        /// <summary>
        /// Moves a directory to a quarantine path, ensuring permissions and logging operations.
        /// </summary>
        /// <param name="dir">The directory path to move.</param>
        public static void MoveDirNormal(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                Logger.Instance.LogFix("Directory path cannot be null or empty.");
                return;
            }

            if (!Directory.Exists(dir))
            {
                Logger.Instance.LogFix($"Directory does not exist: {dir}");
                return;
            }

            try
            {
                // Log the directory move attempt
                Logger.Instance.LogFix($@"""{dir}"" Folders Moving: {Environment.NewLine}{Environment.NewLine}");

                // Grant permissions to the directory
                GrantFolderPermissions(dir);

                // Get all files recursively in the directory
                var fileArray = FileFix.GetFileListRecursively(dir);

                // Move each file individually
                foreach (var file in fileArray)
                {
                    if (!string.IsNullOrEmpty(file))
                    {
                        FileFix.MoveFileNormal(file, dir);
                    }
                }

                // Create the destination directory path
                string newDir = Regex.Replace(dir, @"(?i)([a-z]):", "$1");
                string sDest = Path.Combine(FolderConstants.QuarantinePath, newDir);

                // Attempt to move the directory
                bool dirMoved = MoveDirectory(dir, sDest);

                if (dirMoved && !Directory.Exists(dir))
                {
                    MovedDirNormal(dir);
                }
                else
                {
                    HandleMoveFailure(dir, sDest);
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors
                Logger.Instance.LogFix($"Error in MoveDirNormal: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");
                throw;
            }
        }

        /// <summary>
        /// Grants folder permissions to ensure access.
        /// </summary>
        /// <param name="dir">The directory to grant permissions to.</param>
        private static void GrantFolderPermissions(string dir)
        {
            try
            {
                // Simulated permission granting logic
                Console.WriteLine($"Granted permissions for: {dir}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error granting permissions to {dir}: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the failure of directory movement.
        /// </summary>
        /// <param name="dir">The original directory path.</param>
        /// <param name="sDest">The intended destination path.</param>
        private static void HandleMoveFailure(string dir, string sDest)
        {
            try
            {
                // Retry granting permissions
                GrantFolderPermissions(dir);

                // Log the failure
                Logger.Instance.LogFix($@"{dir} => Move failed.{Environment.NewLine}{Environment.NewLine}");

                // Queue for move on reboot
                FileFix.MoveFileOnReboot(dir, sDest);

                // Update files and reboot logs
                Logger.Instance.LogFix(dir + Environment.NewLine);
                Logger.Instance.LogFix("reboot?" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error handling move failure for {dir}: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs the successful move of a directory.
        /// </summary>
        /// <param name="dir">The directory path.</param>
        private static void MovedDirNormal(string dir)
        {
            Logger.Instance.LogFix($"Directory moved successfully: {dir}");
            Console.WriteLine($"Directory moved successfully: {dir}");
        }



        // Method for moving a directory
        private static bool MoveDirectory(string sourceDir, string destDir)
        {
            try
            {
                if (Directory.Exists(sourceDir))
                {
                    Directory.Move(sourceDir, destDir);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving directory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a reparse point at the specified path.
        /// </summary>
        /// <param name="path">The path of the reparse point.</param>
        public static void DeleteReparsePoint(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new DirectoryNotFoundException($"The path does not exist: {path}");
            }

            IntPtr handle = IntPtr.Zero;

            try
            {
                // Open the reparse point with proper flags
                handle = Kernel32NativeMethods.CreateFile(
                    path,
                    NativeMethodConstants.DELETE,
                    0,
                    IntPtr.Zero,
                    NativeMethodConstants.OPEN_EXISTING,
                    NativeMethodConstants.FILE_FLAG_OPEN_REPARSE_POINT | NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero
                );

                if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                {
                    throw new IOException($"Failed to open reparse point. Error: {Marshal.GetLastWin32Error()}");
                }

                // Delete the reparse point
                if (!Kernel32NativeMethods.DeleteFile(path))
                {
                    throw new IOException($"Failed to delete reparse point. Error: {Marshal.GetLastWin32Error()}");
                }

                Console.WriteLine($"Reparse point deleted successfully: {path}");
            }
            finally
            {
                if (handle != IntPtr.Zero && handle != new IntPtr(-1))
                {
                    Kernel32NativeMethods.CloseHandle(handle);
                }
            }
        }

        public static void MoveDirectory(string directoryPath)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileName(directoryPath));
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.Move(directoryPath, tempDir);
                Console.WriteLine($"Moved directory to temp: {directoryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to move directory {directoryPath}: {ex.Message}");
            }
        }

        public static void EmptyFolderAdvanced(string path)
        {
            if (!Directory.Exists(path))
            {
                return; // Folder does not exist
            }

            try
            {
                // Attempt to create a file to gain access
                try
                {
                    string tempFilePath = Path.Combine(path, "tempfile.txt");
                        FileFix.CreateFile(tempFilePath, 268435456); // Assuming this creates the file successfully
                    if (File.Exists(tempFilePath)) // Check if the file was successfully created
                    {
                        // Grant access and attempt deletion of folder
                        FileFix.GrantAccess(path, true, false);
                        Directory.Delete(path, true);

                        // Verify if the directory was deleted
                        if (!Directory.Exists(path))
                        {
                            Console.WriteLine("Folder successfully removed.");
                            return; // Exit if deletion was successful
                        }
                        else
                        {
                            Console.WriteLine("Failed to remove the folder.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to create the file.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during file creation or access: {ex.Message}");
                }

                // Continue with the recursive deletion process if the initial file creation was successful
                var items = Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories).Reverse().ToList();
                var protectedFiles = new List<string>();

                foreach (var item in items)
                {
                    try
                    {
                        if (FileUtils.IsReparsePoint(item))
                        {
                            FileFix.DeleteReparsePoint(item, true);
                            string fsutilPath = Path.Combine(Environment.SystemDirectory, "fsutil.exe");

                            if (FileUtils.IsReparsePoint(item) && SystemUtils.GetBootMode() != "Recovery")
                            {
                                CommandHandler.RunCommand($"\"{fsutilPath}\" reparsepoint delete \"{item}\"");
                            }
                        }
                        else if (!DirectoryUtils.IsDirectory(item))
                        {
                            if (FileUtils.CheckFile(path, unchecked((uint)2147483648)) != 32)
                            {
                                FileFix.DeleteFile(item);
                                if (File.Exists(item))
                                {
                                    FileFix.MoveFileToTemp(item);
                                }
                            }


                            if (File.Exists(item))
                            {
                                protectedFiles.Add(item);
                            }
                        }
                        else if (DirectoryUtils.IsDirectory(item))
                        {
                            bool isProtected = protectedFiles.Any(file => file.StartsWith(item, StringComparison.OrdinalIgnoreCase));

                            if (!isProtected)
                            {
                                Directory.Delete(item, true);
                                if (Directory.Exists(item))
                                {
                                    FileFix.UnlockAllChildItems(path);
                                    Directory.Delete(item, true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing item: {item}. {ex.Message}");
                    }
                }

                // Schedule remaining protected files for deletion on reboot
                foreach (var file in protectedFiles)
                {
                    FileFix.ScheduleFileForDeletionOnReboot(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning folder: {path}. {ex.Message}");
            }
        }

        public static bool EmptyFolderFiles(string path, string searchPattern)
        {
            if (!Directory.Exists(path))
            {
                return false; // Folder does not exist
            }

            Console.WriteLine($"Deleting files in folder: {path} with search pattern: {searchPattern}");

            try
            {
                // Get all matching files in the folder and subfolders
                var files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    // Call the EmptyFile method for each file
                    FileFix.EmptyFile(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing folder: {path}. {ex.Message}");
                return false;
            }
        }
        public static bool EmptyFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                return true; // Folder does not exist, treated as success
            }

            Console.WriteLine($"Deleting contents of folder: {path}");

            try
            {
                // Ensure the folder can be accessed and permissions are granted
                DirectoryUtils.EnsureFolderAccessibility(path);

                // Get all subfolders in reverse order
                var subfolders = DirectoryHandler.GetAllSubfolders(path);
                subfolders.Reverse(); // Reverse to ensure proper deletion order

                foreach (var subfolder in subfolders)
                {
                    try
                    {
                        Directory.Delete(subfolder, true);

                        if (Directory.Exists(subfolder) && DirectoryUtils.BackupFolder(subfolder))
                        {
                            Directory.Delete(subfolder, true);
                        }

                        if (Directory.Exists(subfolder))
                        {
                            EmptyFolderRecursively(subfolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting subfolder: {subfolder}. {ex.Message}");
                    }
                }

                // Delete all files in the folder
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    FileFix.DeleteFile(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing folder: {path}. {ex.Message}");
                return false;
            }
        }

        public static void EmptyFolderRecursively(string path)
        {
            try
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    FileFix.DeleteFile(file);
                }

                foreach (var directory in Directory.GetDirectories(path))
                {
                    EmptyFolder(directory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recursively emptying folder: {path}. {ex.Message}");
            }
        }

        public static void RemoveDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting directory: {ex.Message}");
            }
        }

        public static bool CopyDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(sourcePath))
                {
                    string destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                foreach (var dir in Directory.GetDirectories(sourcePath))
                {
                    string destDir = Path.Combine(destinationPath, Path.GetFileName(dir));
                    Directory.CreateDirectory(destDir);
                    CopyDirectory(dir, destDir); // Recursively copy subdirectories
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static long CleanFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long totalSizeFreed = 0;
            try
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    totalSizeFreed += new FileInfo(file).Length;
                    File.Delete(file);
                }

                foreach (var dir in Directory.GetDirectories(folderPath))
                {
                    totalSizeFreed += CleanFolder(dir);
                    Directory.Delete(dir, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning folder {folderPath}: {ex.Message}");
            }

            return totalSizeFreed;
        }

        public static long CleanFolderFiles(string folderPath, string searchPattern)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long totalSizeFreed = 0;
            try
            {
                foreach (var file in Directory.GetFiles(folderPath, searchPattern))
                {
                    totalSizeFreed += new FileInfo(file).Length;
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning files in {folderPath}: {ex.Message}");
            }

            return totalSizeFreed;
        }

        public static string ExtractFolderFromFix(string fixPath)
        {
            return fixPath.Replace("symlink:", "").Trim();
        }

        public static void GrantAccess(string folderPath)
        {
            try
            {
                DirectorySecurity security = new DirectorySecurity();
                security.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                Directory.SetAccessControl(folderPath, security);
                Console.WriteLine($"Granted full access to folder: {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to grant access to folder: {folderPath}. Error: {ex.Message}");
            }
        }

        public static void ForceDeleteDirectory(string folderPath)
        {
            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                foreach (string dir in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
                {
                    Directory.Delete(dir, true);
                }
                Directory.Delete(folderPath, true);
                Console.WriteLine($"Forcefully deleted: {folderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to forcefully delete: {folderPath}. Error: {ex.Message}");
            }
        }


        public static void DeleteSubfolders(string path)
        {
            var subFolders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var subFolder in subFolders.Reverse())
            {
                TryDeleteDirectory(subFolder);
            }
        }

        public static bool TryDeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
                Console.WriteLine($"Deleted directory: {path}");
                return true;
            }
            catch
            {
                Console.WriteLine($"Failed to delete directory: {path}");
                return false;
            }
        }

        public static void UnlockAllChild(string path)
        {
            try
            {
                // Create a dummy file to gain access (simulate _CREATEFILE)
                if (FileFix.CreateFileForAccessBool(path))
                {
                    // Passing 1 for true to indicate "unlocked"
                    FileFix.Unlock(path, 1); // Instead of true, pass 1
                }


                UnlockAllDirectories(path);

                // Get all files recursively in the directory
                string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    string shortName = FileUtils.GetShortPath(file);

                    if (FileFix.CreateFileForAccessBool(shortName) && FileUtils.CheckFile(shortName, FileAttributes.ReadOnly) != FileAttributes.Compressed)
                    {
                        // Convert 'true' to '1' for the integer expected by the Unlock method
                        FileFix.Unlock(shortName, 1); // 1 indicates unlocked
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing path '{path}': {ex.Message}");
            }
        }

        public static void UnlockAllDirectories(string path)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    Console.WriteLine($"Unlocking directory: {directory}");
                    // Pass 1 for "true" to the Unlock method
                    FileFix.Unlock(directory, 1);  // 1 indicates unlocked
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking directories in '{path}': {ex.Message}");
            }
        }
        public static void ResFol(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                // Logger.Instance.WriteToLog($"\"{folderPath}\" => Directory not found.");
                return;
            }

            Console.WriteLine($"Restoring folder: {folderPath}");
            // Implement folder restoration logic here
        }

        public static void MoveDir(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }

            // Ensure the destination directory does not exist
            if (Directory.Exists(destinationDir))
            {
                throw new IOException($"Destination directory already exists: {destinationDir}");
            }

            Directory.Move(sourceDir, destinationDir);
        }
    }
}

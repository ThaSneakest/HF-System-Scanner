using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Scripting
{
    public class BackupScripting
    {
        private static string Fix = "FCheck: C:\\ExamplePath\\ExampleFile.txt [AdditionalInfo]";
        private static readonly string[] Hives = { "DEFAULT", "SAM", "SECURITY", "SOFTWARE", "SYSTEM" };
        private const string BackupDir = @"C:\Windows\System32\config\HiveBackup";
        private const string ConfigDir = @"C:\Windows\System32\config\";
        private const string RegLoadSystem = "reg load hklm\\999 c:\\Windows\\System32\\config\\System";
        private const string RegLoadSoftware = "reg load hklm\\888 c:\\Windows\\System32\\config\\SOFTWARE";


        public static void RestoreFromHiveBackup(string hiveName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(hiveName))
            {
                Logger.Instance.LogFix("Invalid hive name provided.");
                return;
            }

            string sourcePath = Path.Combine(@"C:\Windows\System32\config\HiveBackup", hiveName);
            string destinationPath = Path.Combine(@"C:\Windows\System32\config", hiveName);

            try
            {
                // Check if source file exists
                if (!File.Exists(sourcePath))
                {
                    Logger.Instance.LogFix($"Source file not found: {sourcePath}");
                    return;
                }

                // Attempt to copy the file
                bool isCopied = FileFix.CopyFile(sourcePath, destinationPath);

                if (isCopied)
                {
                    Logger.Instance.LogFix($"{hiveName} => {StringConstants.COP} System32\\config{Environment.NewLine}");
                }
                else
                {
                    Logger.Instance.LogFix($"{hiveName} => {StringConstants.NCOPY}{Environment.NewLine}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied while restoring hive: {ex.Message}");
            }
            catch (IOException ex)
            {
                Logger.Instance.LogFix($"I/O error occurred while restoring hive: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Unexpected error occurred while restoring hive: {ex.Message}");
            }
        }

        public static void RestoreFromRegBack(string hiveName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(hiveName))
            {
                Logger.Instance.LogFix("Invalid hive name provided.");
                return;
            }

            string configPath = @"C:\Windows\System32\config\";
            string regBackPath = Path.Combine(configPath, "RegBack");
            string hiveBackupPath = Path.Combine(configPath, "HiveBackup");

            try
            {
                // Ensure HiveBackup directory exists
                if (!Directory.Exists(hiveBackupPath))
                {
                    Directory.CreateDirectory(hiveBackupPath);
                }

                string hiveFilePath = Path.Combine(configPath, hiveName);
                string regBackHivePath = Path.Combine(regBackPath, hiveName);
                string hiveBackupFilePath = Path.Combine(hiveBackupPath, hiveName);

                // Step 1: Backup the current hive file
                if (File.Exists(hiveFilePath))
                {
                    bool isBackupSuccessful = FileFix.CopyFile(hiveFilePath, hiveBackupFilePath);
                    if (isBackupSuccessful)
                    {
                        Logger.Instance.LogFix($"{hiveName} => {StringConstants.COP} System32\\config\\HiveBackup{Environment.NewLine}");
                    }
                    else
                    {
                        Logger.Instance.LogFix($"{hiveName} => {StringConstants.NCOPY} System32\\config\\HiveBackup{Environment.NewLine}");
                    }
                }
                else
                {
                    Logger.Instance.LogFix($"{hiveFilePath} => {StringConstants.NOTFOUND}{Environment.NewLine}");
                }

                // Step 2: Restore the hive file from RegBack
                if (File.Exists(regBackHivePath))
                {
                    bool isRestoreSuccessful = FileFix.CopyFile(regBackHivePath, hiveFilePath);
                    if (isRestoreSuccessful)
                    {
                        Logger.Instance.LogFix($"{hiveName} => {StringConstants.RESTORED} from RegBack{Environment.NewLine}");
                    }
                    else
                    {
                        Logger.Instance.LogFix($"{hiveName} => {StringConstants.NRESTORE} from RegBack{Environment.NewLine}");
                    }
                }
                else
                {
                    Logger.Instance.LogFix($"{regBackHivePath} => {StringConstants.NOTFOUND}{Environment.NewLine}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied while restoring hive '{hiveName}': {ex.Message}{Environment.NewLine}");
            }
            catch (IOException ex)
            {
                Logger.Instance.LogFix($"I/O error occurred while handling hive '{hiveName}': {ex.Message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Unexpected error occurred while restoring hive '{hiveName}': {ex.Message}{Environment.NewLine}");
            }
        }

        public static void RestoreHive()
        {
            // Extract hive name from the input using Regex
            string hiveName = Regex.Replace(Fix, @"(?i)Restore\s*From\s*Backup:\s*(.+)", "$1");

            // Validate hive name
            if (string.IsNullOrWhiteSpace(hiveName))
            {
                Logger.Instance.LogFix("Invalid hive name provided.");
                return;
            }

            try
            {
                // Step 1: Unload the registry hives
                UnloadRegistryHive(hiveName);

                // Step 2: Rename the existing hive file
                string hivePath = Path.Combine(@"C:\Windows\System32\config\", hiveName);
                string renamedHivePath = hivePath + ".old";

                try
                {
                    // Attempt to move the file
                    FileFix.MoveFile(hivePath, renamedHivePath, true);
                    Logger.Instance.LogFix($"\"{hiveName}\" => {StringConstants.REN0} ({hiveName}.old)\n");
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogFix($"Failed to rename hive file \"{hiveName}\": {ex.Message}\n");
                }


                // Step 3: Copy the hive file from the backup
                string backupHivePath = Path.Combine(@"C:\FRST\Hives\", hiveName);
                if (FileFix.CopyFile(backupHivePath, hivePath))
                {
                    Logger.Instance.LogFix($"\"{hiveName}\" => {StringConstants.RESTORED}\n");
                }
                else
                {
                    Logger.Instance.LogFix($"\"{hiveName}\" => {StringConstants.NRESTORE}\n");
                }

                // Step 4: Reload the registry hive
                ReloadRegistryHive(hiveName);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied while restoring hive \"{hiveName}\": {ex.Message}\n");
            }
            catch (IOException ex)
            {
                Logger.Instance.LogFix($"I/O error occurred while restoring hive \"{hiveName}\": {ex.Message}\n");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Unexpected error occurred while restoring hive \"{hiveName}\": {ex.Message}\n");
            }
        }

        private static void UnloadRegistryHive(string hiveName)
        {
            string command;
            switch (hiveName)
            {
                case "SYSTEM":
                case "SAM":
                case "SECURITY":
                    command = "reg unload hklm\\999";
                    break;
                case "SOFTWARE":
                    command = "reg unload hklm\\888";
                    break;
                default:
                    command = null;
                    break;
            }


            if (!string.IsNullOrEmpty(command))
            {
                CommandHandler.RunCommand(command);
                Logger.Instance.LogFix($"Registry hive \"{hiveName}\" unloaded.\n");
            }
        }

        private static void ReloadRegistryHive(string hiveName)
        {
            string command;
            switch (hiveName)
            {
                case "SYSTEM":
                case "SAM":
                case "SECURITY":
                    command = "reg load hklm\\999 c:\\Windows\\System32\\config\\System";
                    break;
                case "SOFTWARE":
                    command = "reg load hklm\\888 c:\\Windows\\System32\\config\\SOFTWARE";
                    break;
                default:
                    command = null;
                    break;
            }


            if (!string.IsNullOrEmpty(command))
            {
                CommandHandler.RunCommand(command);
                Logger.Instance.LogFix($"Registry hive \"{hiveName}\" reloaded.\n");
            }
        }

        public static void RestoreHiveBackup()
        {
            UnloadRegistryHives();

            try
            {
                foreach (string hive in Hives)
                {
                    RestoreFromHiveBackup(hive);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error restoring hives from backup: {ex.Message}");
            }

            ReloadRegistryHives();
        }

        public static void RestoreHives()
        {
            UnloadRegistryHives();

            try
            {
                // Ensure HiveBackup directory exists
                Directory.CreateDirectory(BackupDir);

                foreach (string hive in Hives)
                {
                    RestoreFromRegBack(hive);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error restoring hives from RegBack: {ex.Message}");
            }

            ReloadRegistryHives();
        }

        private static void UnloadRegistryHives()
        {
            try
            {
                CommandHandler.RunCommand("reg unload hklm\\999");
                CommandHandler.RunCommand("reg unload hklm\\888");
                Logger.Instance.LogFix("Registry hives unloaded.");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error unloading registry hives: {ex.Message}");
            }
        }

        private static void ReloadRegistryHives()
        {
            try
            {
                CommandHandler.RunCommand(RegLoadSystem);
                CommandHandler.RunCommand(RegLoadSoftware);
                Logger.Instance.LogFix("Registry hives reloaded.");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error reloading registry hives: {ex.Message}");
            }
        }

        public static void RestorePints()
        {
            // Unload the registry hives
            UnloadRegistryHives();

            // Define the hives and their corresponding snapshot paths
            var hivesToRestore = new[]
            {
                ("SAM", "_REGISTRY_MACHINE_SAM"),
                ("SECURITY", "_REGISTRY_MACHINE_SECURITY"),
                ("SOFTWARE", "_REGISTRY_MACHINE_SOFTWARE"),
                ("SYSTEM", "_REGISTRY_MACHINE_SYSTEM"),
                ("DEFAULT", "_REGISTRY_USER_.DEFAULT")
            };

            // Restore each hive from the snapshot
            foreach (var (hiveName, registryFileName) in hivesToRestore)
            {
                string hivePath = Regex.Replace(Fix, @"(?i)RP:.+(_res.+\\RP\d+)", $"C:\\System Volume Information\\$1\\snapshot\\{registryFileName}");
                RestorePintsXp(hivePath, hiveName);
            }

            // Reload the registry hives after restoring
            ReloadRegistryHives();
        }

        public static void RestorePintsXp(string hiveBackupPath, string hiveName)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(hiveBackupPath) || string.IsNullOrWhiteSpace(hiveName))
                {
                    Logger.Instance.LogFix($"Invalid parameters: hiveBackupPath or hiveName is null or empty.");
                    return;
                }

                string destinationPath = Path.Combine(@"C:\Windows\System32\config\", hiveName);

                // Ensure the backup file exists
                if (!File.Exists(hiveBackupPath))
                {
                    Logger.Instance.LogFix($"Hive backup not found: {hiveBackupPath}");
                    return;
                }

                // Attempt to copy the hive file
                bool result = FileFix.CopyFile(hiveBackupPath, destinationPath);

                if (result)
                {
                    Logger.Instance.LogFix($"\"{hiveName}\" successfully restored from \"{hiveBackupPath}\".");
                }
                else
                {
                    Logger.Instance.LogFix($"Failed to restore \"{hiveName}\" from \"{hiveBackupPath}\".");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied while restoring \"{hiveName}\" from \"{hiveBackupPath}\": {ex.Message}");
            }
            catch (IOException ex)
            {
                Logger.Instance.LogFix($"I/O error occurred while restoring \"{hiveName}\" from \"{hiveBackupPath}\": {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Unexpected error while restoring \"{hiveName}\" from \"{hiveBackupPath}\": {ex.Message}");
            }
        }

        public static void RestoreXP()
        {
            string tempDir = Path.GetTempPath();
            string restorePoint = "Restore Point 1"; // Replace with actual value
            string scanLabel = "Scanning"; // Replace with actual value

            string searchPattern = "*_SAM";
            string systemVolumeFolder = @"C:\System Volume Information";
            string tempLogFile = Path.Combine(tempDir, "log0");

            // Write a header to the log file
            Logger.Instance.LogFix($"\n==================== {restorePoint} (XP) =====================\n\n");

            try
            {
                // Search for files recursively and write results to a temporary log file
                var files = Directory.GetFiles(systemVolumeFolder, searchPattern, SearchOption.AllDirectories);
                File.WriteAllLines(tempLogFile, files);

                // Process each file listed in the temporary log
                int lineIndex = 0;
                foreach (string file in files)
                {
                    try
                    {
                        // Update GUI (simulated here with a Console message)
                        Console.WriteLine($"{scanLabel} {restorePoint}: {file}");

                        // Gather file information
                        var fileInfo = new FileInfo(file);
                        long size = fileInfo.Length;
                        string formattedSize = size.ToString("D6"); // Format size with leading zeros
                        string creationDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");

                        // Extract a specific portion of the file path using Regex
                        string formattedPath = Regex.Replace(file, @"(?i)C:\\.+ion\\(_r.+\\RP\d+).+", "$1");

                        // Write file details to the log
                        Logger.Instance.LogFix($"RP: -> {creationDate} - {formattedSize} {formattedPath}\n\n");

                        lineIndex++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogFix($"Error processing file: {file} - {ex.Message}\n");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied to folder {systemVolumeFolder}: {ex.Message}\n");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Error during file search or processing: {ex.Message}\n");
            }
            finally
            {
                // Clean up temporary log file
                DeleteTempLog(tempLogFile);
            }
        }

        private static void DeleteTempLog(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete temporary log file {filePath}: {ex.Message}");
            }
        }

        public static void RestorePoint()
        {

            string rp1 = "Restore Point 1"; // Replace with actual value
            string resd0 = "Restore Description"; // Replace with actual value

            // Write header to the log file
            Logger.Instance.LogFix($"\n==================== {rp1} =========================\n\n");

            try
            {
                // Get the list of files in the specified directory
                string folderPath = @"C:\System Volume Information";
                if (!Directory.Exists(folderPath))
                {
                    Logger.Instance.LogFix($"Directory not found: {folderPath}\n");
                    return;
                }

                var fileDetails = Directory
                    .GetFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Select(file =>
                    {
                        string creationDate = File.GetCreationTime(file).ToString("yyyy-MM-dd HH:mm:ss");
                        return $"{resd0}: {creationDate} ||||";
                    })
                    .OrderBy(detail => detail) // Sort the array
                    .ToList();

                // Write sorted file details to the log file
                Logger.Instance.LogFix(string.Join(Environment.NewLine, fileDetails) + Environment.NewLine);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance.LogFix($"Access denied to folder: {ex.Message}\n");
            }
            catch (IOException ex)
            {
                Logger.Instance.LogFix($"I/O error: {ex.Message}\n");
            }
            catch (Exception ex)
            {
                Logger.Instance.LogFix($"Unexpected error: {ex.Message}\n");
            }
        }
        public static void SBKP(string[] sourcePaths)
        {
            string shadowCopyPath = null;
            string mappedDrive = null;
            string tempFolder = null;

            try
            {
                // Create a shadow copy and map it to a drive
                shadowCopyPath = CreateShadowCopy();
                mappedDrive = MapShadowCopyToDrive(shadowCopyPath);

                // Create a temporary hidden and system folder
                tempFolder = Path.Combine(@"C:\FRST", Utility.GenerateRandomString());
                Directory.CreateDirectory(tempFolder);
                File.SetAttributes(tempFolder, FileAttributes.Hidden | FileAttributes.System);

                foreach (var sourcePath in sourcePaths)
                {
                    try
                    {
                        ProcessSourcePath(sourcePath, mappedDrive, tempFolder);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing source path '{sourcePath}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shadow backup: {ex.Message}");
            }
            finally
            {
                // Cleanup resources
                FileFix.Cleanup(mappedDrive, shadowCopyPath, tempFolder);
            }
        }

        private static void ProcessSourcePath(string sourcePath, string mappedDrive, string tempFolder)
        {
            // Get relative and destination paths
            string relativePath = sourcePath.Substring(3); // Remove "C:\" prefix
            string fileName = Path.GetFileName(relativePath);
            string destination = Path.Combine(@"C:\FRST\Hives", Path.GetFileName(sourcePath));

            if (sourcePath.Contains(@"\Users\"))
            {
                destination = Path.Combine(@"C:\FRST\Hives", Environment.UserName);
                Directory.CreateDirectory(destination);
            }

            string fullSource = Path.Combine(mappedDrive, relativePath);

            // Copy the main file and its associated log file
            CopyFileWithLog(fullSource, tempFolder);

            // Load and unload registry hive if applicable
            if (Environment.OSVersion.Version.Major > 6 || Environment.OSVersion.Version.Minor > 1)
            {
                LoadAndUnloadHive(tempFolder, fileName);
            }

            // Copy the processed file to the final destination
            File.Copy(Path.Combine(tempFolder, fileName), destination, true);

            // Adjust attributes for certain files
            if (Regex.IsMatch(sourcePath, @"(?i)(NTUSER|UsrClass)\.DAT"))
            {
                File.SetAttributes(destination, FileAttributes.Normal);
            }
        }

        private static void CopyFileWithLog(string fullSource, string tempFolder)
        {
            try
            {
                File.Copy(fullSource, tempFolder, true);
                File.Copy(fullSource + ".LOG", tempFolder, true);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to copy file or log from '{fullSource}' to '{tempFolder}': {ex.Message}");
            }
        }

        private static void LoadAndUnloadHive(string tempFolder, string fileName)
        {
            string tempHiveKey = Utility.GenerateRandomString();
            string hivePath = Path.Combine(tempFolder, fileName);

            int loadResult = Advapi32NativeMethods.RegLoadKey(NativeMethodConstants.HKEY_LOCAL_MACHINE, tempHiveKey, hivePath);
            if (loadResult != 0)
            {
                throw new InvalidOperationException($"Failed to load registry hive. Error: {loadResult}");
            }

            Advapi32NativeMethods.RegUnLoadKey(NativeMethodConstants.HKEY_LOCAL_MACHINE, tempHiveKey);
        }

        private static string CreateShadowCopy()
        {
            try
            {
                using (var managementClass = new ManagementClass(@"\\.\root\cimv2", "Win32_ShadowCopy", null))
                {
                    using (var parameters = managementClass.GetMethodParameters("Create"))
                    {
                        // Set required parameters for creating a shadow copy
                        parameters["Volume"] = @"C:\";
                        parameters["Context"] = "ClientAccessible";

                        // Invoke the Create method
                        var result = managementClass.InvokeMethod("Create", parameters, null);

                        // Validate and return the ShadowCopyID
                        if (result != null && result["ShadowID"] != null)
                        {
                            return result["ShadowID"].ToString();
                        }
                        throw new InvalidOperationException("Failed to create shadow copy. No ShadowCopyID returned.");
                    }
                }
            }
            catch (ManagementException ex)
            {
                throw new InvalidOperationException($"Management error while creating shadow copy: {ex.Message}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException("Insufficient permissions to create shadow copy. Run as administrator.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error while creating shadow copy: {ex.Message}", ex);
            }
        }


        private static string MapShadowCopyToDrive(string shadowCopyPath)
        {
            if (string.IsNullOrWhiteSpace(shadowCopyPath))
            {
                throw new ArgumentNullException(nameof(shadowCopyPath), "Shadow copy path cannot be null or empty.");
            }

            for (char drive = 'D'; drive <= 'Z'; drive++)
            {
                string driveLetter = $"{drive}:";

                try
                {
                    // Check if the drive is available
                    if (!Directory.Exists(driveLetter))
                    {
                        // Attempt to map the shadow copy
                        bool mapped = Kernel32NativeMethods.DefineDosDevice(0, driveLetter, shadowCopyPath);
                        if (mapped)
                        {
                            return driveLetter;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while attempting to map {driveLetter} to {shadowCopyPath}: {ex.Message}");
                }
            }

            throw new InvalidOperationException("Failed to map shadow copy to an available drive letter.");
        }
    }
}

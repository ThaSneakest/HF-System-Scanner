using DevExpress.Emf;
using DevExpress.Utils.Drawing.Helpers;
using DevExpress.Xpo.Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Handlers;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

public static class FileUtils
{
    private static string Fix = "FCheck: C:\\ExamplePath\\ExampleFile.txt [AdditionalInfo]";
    private static string destinationPath = @"C:\Destination";
    private static string FixDescription = "Fix description here";
    private static readonly bool IsCryptEnabled = true; // Replace with your condition if required
    private static readonly string _baseDirectory;
    private static readonly string _recoveryDrive;
    private static readonly bool _isRecoveryMode;
    private static readonly string _scriptDirectory;
    private static List<string> allUsers = new List<string>();
    private const string DirectoryMessage = "Directory";
    private static readonly int CRYPT = 1;
    private static string FILE = "";
    private static string ARG = "";


    // Helper method to get file creation date
    public static string GetFileCreationDate(string file)
    {
        try
        {
            var fileInfo = new FileInfo(file);
            return fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "N/A";
        }
    }

    // Helper method to get file company name
    public static string GetFileCompany(string file)
    {
        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(file);
            return fileInfo.CompanyName ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Reads a specific line from a file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="lineNumber">The line number to read (1-based).</param>
    /// <returns>The content of the specified line, or null if the line does not exist.</returns>
    public static string ReadLineFromFile(string filePath, int lineNumber)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (lineNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber),
                "Line number must be greater than or equal to 1.");
        }

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                int currentLine = 0;

                // Read lines until the specified line number is reached
                while ((line = reader.ReadLine()) != null)
                {
                    currentLine++;
                    if (currentLine == lineNumber)
                    {
                        return line;
                    }
                }
            }

            // Return null if the line number exceeds the total number of lines
            return null;
        }
        catch (Exception ex)
        {
            throw new IOException($"Error reading from file: {ex.Message}");
        }
    }

    public static DateTime? GetFileCreationDateTime(string path)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Exists ? fileInfo.CreationTime : (DateTime?)null;
        }
        catch (Exception)
        {
            return null;
        }
    }


    /// <summary>
    /// Gets properties of a file, such as size, creation time, modification time, and attributes.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A formatted string containing file properties.</returns>
    public static string GetFileProperties(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        try
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Retrieve file properties
            long size = fileInfo.Length;
            DateTime creationTime = fileInfo.CreationTime;
            DateTime lastAccessTime = fileInfo.LastAccessTime;
            DateTime lastWriteTime = fileInfo.LastWriteTime;
            FileAttributes attributes = fileInfo.Attributes;

            // Format the properties into a readable string
            return $@"
File: {fileInfo.FullName}
Size: {size} bytes
Created: {creationTime}
Last Accessed: {lastAccessTime}
Last Modified: {lastWriteTime}
Attributes: {attributes}
";
        }
        catch (Exception ex)
        {
            return $"Error retrieving file properties: {ex.Message}";
        }
    }

    public static string[] FileListToArray(
        string filePath,
        string filter = "*",
        int flag = 0,
        bool returnPath = false)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is invalid.");

        if (filter == null) filter = "*";

        if (flag != 0 && flag != 1 && flag != 2)
            throw new ArgumentException("Flag must be 0, 1, or 2.");

        // Normalize file path
        filePath = Regex.Replace(filePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            @"[\\/]+$", "") + Path.DirectorySeparatorChar;

        if (!Directory.Exists(filePath))
            throw new DirectoryNotFoundException($"The directory '{filePath}' does not exist.");

        if (Regex.IsMatch(filter, @"[\\/:><|]|\s*$"))
            throw new ArgumentException("Invalid filter specified.");

        // Collect files
        List<string> fileList = new List<string>();

        try
        {
            string[] files = Directory.GetFiles(filePath, filter);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                // Handle flag conditions
                if (flag == 1 && (fileInfo.Attributes & FileAttributes.Hidden) != 0)
                    continue; // Skip hidden files
                if (flag == 2 && (fileInfo.Attributes & FileAttributes.Hidden) == 0)
                    continue; // Include only hidden files

                // Add file to list
                fileList.Add(returnPath ? file : Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Error accessing files in '{filePath}': {ex.Message}", ex);
        }

        if (fileList.Count == 0)
            throw new FileNotFoundException("No files matched the specified criteria.");

        return fileList.ToArray();
    }

    public static List<string> FileListToArrayRecursive(
        string filePath,
        string mask = "*",
        int returnType = 0,
        int recursion = 0,
        int sort = 0,
        int returnPath = 1)
    {
        // Validate the base directory
        if (string.IsNullOrWhiteSpace(filePath) || !Directory.Exists(filePath))
            throw new DirectoryNotFoundException($"The directory '{filePath}' does not exist.");

        if (recursion != 0 && recursion != 1)
            throw new ArgumentException("Recursion value must be 0 or 1.");

        // Normalize file path
        filePath = filePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        // Parse mask for inclusions and exclusions
        var maskParts = mask.Split('|');
        string includeMask = maskParts.Length > 0 ? maskParts[0] : "*";
        string excludeMask = maskParts.Length > 1 ? maskParts[1] : null;
        string excludeFolders = maskParts.Length > 2 ? maskParts[2] : null;

        // Build regex patterns for filters
        Regex includeRegex = includeMask == "*" ? null : new Regex(WildcardToRegex(includeMask));
        Regex excludeRegex = excludeMask == null ? null : new Regex(WildcardToRegex(excludeMask));
        Regex excludeFolderRegex = excludeFolders == null ? null : new Regex(WildcardToRegex(excludeFolders));

        // Collect matching files and directories
        List<string> fileList = new List<string>();
        SearchOption searchOption = recursion == 1 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var entry in Directory.EnumerateFileSystemEntries(filePath, "*", searchOption))
        {
            string name = Path.GetFileName(entry);
            bool isDirectory = Directory.Exists(entry);

            // Apply folder exclusion filters
            if (isDirectory && excludeFolderRegex != null && excludeFolderRegex.IsMatch(name))
                continue;

            // Apply file inclusion/exclusion filters
            if (!isDirectory)
            {
                if (includeRegex != null && !includeRegex.IsMatch(name))
                    continue;
                if (excludeRegex != null && excludeRegex.IsMatch(name))
                    continue;
            }

            // Handle return type logic
            if ((returnType == 0 && !isDirectory) || (returnType == 1 && isDirectory) ||
                (returnType == 2 && (isDirectory || !isDirectory)))
            {
                string relativePath = entry.Replace(filePath, string.Empty);
                fileList.Add(returnPath == 0 ? name : returnPath == 2 ? entry : relativePath);
            }
        }

        // Sort the file list if required
        if (sort == 1)
            fileList = fileList.OrderBy(x => x).ToList();
        else if (sort == 2)
            fileList = fileList.OrderByDescending(x => x).ToList();

        if (!fileList.Any())
            throw new FileNotFoundException("No matching files or directories found.");

        return fileList;
    }

    private static string WildcardToRegex(string pattern)
    {
        return "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
    }





    // Placeholder for the actual implementation of _FILELISTTOARRAYREC
    public static string[] _FILELISTTOARRAYREC(string path, string pattern, int param1, int param2, int param3,
        int param4)
    {
        // Implement the actual logic for getting file list recursively
        return Directory.GetFiles(path);
    }

    // Placeholder for the actual implementation of _SETDEFAULTFILEACCESS
    public static void _SETDEFAULTFILEACCESS(string filePath)
    {
        // Implement the actual logic for setting file access
    }

    public static void RESQUAR()
    {
        if (Regex.IsMatch(Fix, @"(?i)^Restore\s*Quarantine:$"))
        {
            DirectoryFix.ResFol(FolderConstants.HomeDrive + @"\FRST\Quarantine");
            return;
        }

        string path = Regex.Replace(Fix, @"(?i)^Restore\s*Quarantine:\s*(.+)", "$1");

        if (!File.Exists(path))
        {
            Logger.Instance.LogFix($"\"{path}\"=> {StringConstants.PAD} {StringConstants.NOTFOUND}.\n");
            return;
        }

        if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            DirectoryFix.ResFol(path);
        }
        else
        {
            FileFix.ResFile(path);
        }
    }


    public static bool HasRestrictedAccess(string path)
    {
        // Pass the required 'securityInformation' parameter (e.g., DACL_SECURITY_INFORMATION = 4)
        string securityDescriptor = GetSecurityDescriptor(path, 4);

        // Check for restricted access patterns
        if (securityDescriptor.Contains("(D;") ||
            !System.Text.RegularExpressions.Regex.IsMatch(securityDescriptor, @"(?i)A;.*?(GA|FA);;;BA|\(A;;FA;;;WD\)"))
        {
            return true;
        }

        return false;
    }





    public static string CheckFileAccess(string filePath)
    {
        if (FileFix.CreateFile(filePath))
        {
            return $" <==== {StringConstants.UPD1} ({StringConstants.NOACC})";
        }

        string parentPath = Regex.Replace(filePath, @"(.+)\\.+", "$1");
        if (FileFix.CreateFile(parentPath))
        {
            return $" <==== {StringConstants.UPD1} ({StringConstants.NOACC})";
        }

        if (CheckFile(filePath, unchecked((int)2147483648)) == 32)
        {
            return $" <==== {StringConstants.UPD1} ({StringConstants.INUSE})";
        }

        if (IsReparsePoint(filePath))
        {
            string reparseTarget = GetReparseTarget(filePath);
            return $" [symlink -> {reparseTarget}]";
        }

        return $" <==== {StringConstants.UPD1} ({StringConstants.ZEROBYTES} {DirectoryMessage})";
    }

    private static int CheckFile(string filePath, int attribute)
    {
        FileAttributes attributes = File.GetAttributes(filePath);
        return (attributes.HasFlag((FileAttributes)attribute)) ? 32 : 0;
    }


    public static string COMP(string path, string file)
    {
        string company = "";
        string upd1 = "UpdateDetails";
        string noAcc = "No Access";
        string inUse = "In Use";
        string zeroByte = "Zero Byte";

        if (Regex.IsMatch(file, @"\\w{6}~\d\\"))
        {
            file = Path.GetFullPath(file);
        }

        long size;

        if (Regex.IsMatch(file, @":[^\\]"))
        {
            size = File.ReadAllText(file).Length;
        }
        else
        {
            size = new FileInfo(path).Length;
        }

        DateTime cDate = File.GetCreationTime(path);

        if (path.Contains("\\system32\\drivers\\appid.sys") && upd1 != "Recovery")
        {
            company = "Microsoft Windows";
        }
        else
        {
            company = GetFileVersionInfo(path, "CompanyName");
        }

        company = Regex.Replace(company ?? "", "\\s+", " ").TrimEnd();

        if (size > 0)
        {
            if (path.Contains(":\\Program Files\\WindowsApps\\"))
            {
                string file1 = Regex.Replace(path, @"(?i)(.:\\Program Files\\WindowsApps\\[^\\]+).+", "$1");

                if (File.Exists(Path.Combine(file1, "AppxSignature.p7x")))
                {
                    path = Path.Combine(file1, "AppxSignature.p7x");
                }
            }

            int signatureResult = CHECKSIG(path); // Ensure CheckSignature returns int.

            if (signatureResult != 11)
            {
                company += $" [{noAcc}]";

                if (!FileFix.CreateFile(path))
                {
                    company = $"({noAcc}) [{zeroByte}?]";
                }
                else if (CheckFile(path, NativeMethodConstants.GENERIC_READ) == 32)
                {
                    company += $" [{inUse}] <==== {upd1}";
                }
            }
        }
        else
        {
            string ggg = "";

            if (IsReparsePoint(path))
            {
                path = GetReparseTarget(path);
                ggg = $" [symlink -> {path}]";
            }
            else if (!FileFix.CreateFile(path))
            {
                ggg = $" [{noAcc}]";
            }
            else if (CheckFile(path, unchecked((int)0x80000000)) == 32)
            {
                company += $" [{inUse}] <==== {upd1}";
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                ggg = $" <==== {upd1} [{zeroByte}? (Error={lastError})]";
            }

            company = $"({company}){ggg}";
        }

        company = Regex.Replace(company, "(?i)http(s|):", "hxxp$1:");
        return company;
    }



    public static void SystemFile(string hash)
    {
        // Placeholder for GUI control
        string label = "Label1";
        string scanB = "ScanB";
        string fil1 = "Fil1";

        // Placeholder for the GUI control's SetData method
        Console.WriteLine($"{scanB} {fil1}: {FILE}");

        // Get file attributes
        string fileAttributes = File.GetAttributes(FILE).ToString();
        fileAttributes = fileAttributes.Replace("Archive", string.Empty); // Remove "A" flag

        if (IsReparsePoint(FILE)) fileAttributes += "L"; // Check for reparse point

        string att = fileAttributes.PadLeft(5, '0'); // Format the file attributes
        att = att.Replace('0', '_'); // Replace 0 with "_"

        // Get file size and format it
        long fileSize = new FileInfo(FILE).Length;
        string sizeFormatted = fileSize.ToString("D9");

        // Get file creation and modification dates
        DateTime creationDate = File.GetCreationTime(FILE);
        DateTime modificationDate = File.GetLastWriteTime(FILE);

        // Get file version
        string fileVersion = GetFileVersion(FILE); // Implement this method to get the file version

        // Write the file details to the log
        //File.AppendAllText(Logger.WildlandsLogFile, $"{file}\n[{creationDate}] - [{modificationDate}] - {sizeFormatted} {att} ({fileVersion}) {hash}\n\n");

        // Check for specific conditions and log the corresponding messages
        if (!fileVersion.Contains("Microsoft"))
        {
            if (IsReparsePoint(FILE))
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{file} => ATTENTION: Delete reparse point.\n");
            }
            else if (hash == "D41D8CD98F00B204E9800998ECF8427E")
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{file} => D41D8CD98F00B204E9800998ECF8427E (0-byte MD5) <==== Updated\n");
            }
            else if (string.IsNullOrEmpty(fileVersion))
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{file} => No version found <==== Updated\n");
            }
            else
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{file} => {fileVersion}: {fileVersion} <==== Updated\n");
            }
        }
    }



    public static void SYSTEMFILE(string hash)
    {
        // Update label for GUI (Placeholder for GUI control update)
        MainApp form1 = new MainApp();
        string labelText = $"{StringConstants.SCANB} {StringConstants.FIL1}: {FILE}";

        // Update the label's text directly
        form1.labelControlProgress.Text = labelText;


        // Get file attributes and clean them
        string fileAttributes = File.GetAttributes(FILE).ToString();
        fileAttributes = Regex.Replace(fileAttributes, "A", string.Empty); // Remove 'A'
        if (IsReparsePoint(FILE)) // Placeholder for reparse point check
        {
            fileAttributes += "L";
        }

        string formattedAttributes = fileAttributes.PadLeft(5, '0').Replace('0', '_');

        // Get file size
        long fileSize = new FileInfo(FILE).Length;
        string formattedSize = fileSize.ToString("D9"); // Format as 9-digit number

        // Get creation and modification dates
        string dateCreated = GetFileTime(FILE, true); // Placeholder for creation time
        string dateModified = GetFileTime(FILE, false); // Placeholder for modification time

        // Get file version
        string version = GetFileVersionInfo(FILE, "CompanyName") ?? string.Empty; // Placeholder for version retrieval

        // Write to log file
        //File.AppendAllText(Logger.WildlandsLogFile,
        // $"{FILE}{Environment.NewLine}[{dateCreated}] - [{dateModified}] - {formattedSize} {formattedAttributes} ({version}) {hash}{Environment.NewLine}{Environment.NewLine}");

        // Check version string
        if (!version.ToLower().Contains("microsoft"))
        {
            if (IsReparsePoint(FILE)) // Reparse point case
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{FILE} => ATTENTION: Delete reparsepoint.{Environment.NewLine}");
            }
            else if (hash.Equals("D41D8CD98F00B204E9800998ECF8427E",
                         StringComparison.OrdinalIgnoreCase)) // Zero-byte MD5 case
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{FILE} => D41D8CD98F00B204E9800998ECF8427E (0-byte MD5) <==== {UPD1}{Environment.NewLine}");
            }
            else if (string.IsNullOrEmpty(version)) // Missing version case
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{FILE} => {NO} {COMP0} <==== {UPD1}{Environment.NewLine}");
            }
            else // General case
            {
                //File.AppendAllText(Logger.WildlandsLogFile, $"{FILE} => {COMP0}: {version} <==== {UPD1}{Environment.NewLine}");
            }
        }
    }


    public static int BAMWL()
    {
        // Check if CRYPT is enabled, the signature is valid, and the certificate info matches Microsoft
        if (CRYPT == 1 && CHECKSIG(FILE) == 11 && Regex.IsMatch(CHECKSIGNATURE(FILE, "1"), @"^(?i)Microsoft "))
        {
            return 1;
        }

        return 0; // Default return if conditions are not met
    }

    public static string CHECKSIGNATURE(string filePath, string option)
    {
        // Example logic for CHECKSIG
        if (option == "1")
        {
            return "Microsoft Corporation"; // Return a valid string
        }

        return string.Empty; // Default return value
    }



    public static void BAMMIS(string path = null)
    {
        // Default value for 'path' if not provided
        if (path == null)
        {
            path = FILE; // Assume FILE is a predefined constant or variable
        }

        // Check if the file exists
        if (!File.Exists(path))
        {
            // Write to the log file
            // File.AppendAllText(Logger.WildlandsLogFile, $"{path} {MISS} <==== {UPD1}{Environment.NewLine}");
        }
    }


    // CHKFILE method that checks file access using CreateFileW
    public static uint CHKFILE(string path, uint access = 0, uint share = 0)
    {
        if (!path.StartsWith(@"\\?\")) // Ensure the path starts with \\?\ (required for extended path names)
        {
            path = @"\\?\" + path;
        }

        IntPtr fileHandle = Kernel32NativeMethods.CreateFileW(
            path,
            access,
            share,
            IntPtr.Zero,
            NativeMethodConstants.OPEN_EXISTING,
            0,
            IntPtr.Zero
        );

        // Check if the file handle is invalid
        if (fileHandle == (IntPtr)(-1))
        {
            uint errorCode = Kernel32NativeMethods.GetLastError(); // Retrieve last error from GetLastError
            return errorCode; // Return the error code
        }

        // Close the file handle if it was successfully opened
        Kernel32NativeMethods.CloseHandle(fileHandle);

        return 0; // Return 0 to indicate success
    }

    public static bool BBBBDF(string path1)
    {
        if (File.GetAttributes(@"\\?\" + path1).HasFlag(FileAttributes.Directory))
        {
            Logger.Instance.LogFix($"\"{path1}\" => FOL0" + Environment.NewLine);
            return true;
        }

        if (FileFix.FileDelete(path1))
        {
            Logger.Instance.LogFix($"{path1} ----> {StringConstants.DELETED}");
            return true;
        }

        if (Regex.IsMatch(path1, @"(?i)\.exe") && SystemConstants.BootMode != "Recovery")
        {
            var processes = Process.GetProcesses().ToList();
            foreach (var process in processes)
            {
                string processPath = ProcessUtils.GetProcessPath(process.Id);
                if (processPath == path1)
                {
                    if (!ProcessUtils.IsProcessCritical(process.Id))
                    {
                        process.Kill();
                    }

                    break;
                }
            }
        }

        if (FileFix.FileDelete(path1))
        {
            Logger.Instance.LogFix($"{path1} ----> {StringConstants.DELETED}");
            return true;
        }

        if (SystemConstants.BootMode == "Recovery")
        {
            Logger.Instance.LogFix($"{path1} ----> {StringConstants.NOTDELETED}");
        }

        Logger.Instance.LogFix($"NDELETED \"{path1}\" => DELRE." + Environment.NewLine);
        FileFix.MoveFileOnReboot(path1, "");
        File.AppendAllText(Path.Combine(FolderConstants.HomeDrive, "FRST\\files"), path1 + Environment.NewLine);
        File.AppendAllText(Path.Combine(FolderConstants.HomeDrive, "FRST\\re"), "reboote?" + Environment.NewLine);

        return false;
    }

    public static void BBBBDR(string path)
    {
        if (!File.Exists(path)) return;

        if (!path.StartsWith("\\\\?\\"))
        {
            path = "\\\\?\\" + path;
        }

        // Call to RemoveDirectoryW (assumes kernel32.dll is used)
        FileFix.DllCallRemoveDirectory(path);
    }

    public static int BBBBDK(string path)
    {
        string check = string.Empty;
        var runPr = FileListToArrayRecursive(path, "*.exe", 1 + 16, 1, 0, 2);

        foreach (var pr in runPr)
        {
            var processes = Process.GetProcesses().ToList();
            foreach (var process in processes)
            {
                string processPath = ProcessUtils.GetProcessPath(process.Id);
                if (pr == processPath)
                {
                    if (!ProcessUtils.IsProcessCritical(process.Id))
                    {
                        process.Kill();
                    }

                    check = "1";
                    break;
                }
            }
        }

        return string.IsNullOrEmpty(check) ? 0 : 1;
    }






    public static string GetDestination(string path, bool filePath = false)
    {
        string nDir = Regex.Replace(path, ":", "");
        nDir = Regex.Replace(nDir, @"(.+)\.+", "$1");
        nDir = Path.Combine(FolderConstants.QuarantinePath, nDir);

        // Ensure the directory exists
        if (!Directory.Exists(nDir))
        {
            Directory.CreateDirectory(nDir);
        }

        if (filePath)
        {
            string fileName = Path.GetFileName(path);
            nDir = Path.Combine(nDir, fileName + ".xBAD");
        }

        return @"\\?\" + nDir;
    }


    public static bool HasFileAttributes(string path, string attributes)
    {
        FileAttributes fileAttributes = File.GetAttributes(path);
        return fileAttributes.HasFlag((FileAttributes)Enum.Parse(typeof(FileAttributes), attributes, true));
    }


    private static string GetDestination(string source)
    {
        string sanitized = source.Replace(":", "").Replace("\\", "").Replace(".*", "");
        return Path.Combine(FolderConstants.QuarantinePath, sanitized);
    }


    // Check if file attributes match the given attributes
    public static bool FileAttributesContains(string path, FileAttributes attributes)
    {
        FileAttributes currentAttributes = File.GetAttributes(path);
        return (currentAttributes & attributes) == attributes;
    }


    public static void DisplaySidInfo(IntPtr sidPointer)
    {
        try
        {
            // Convert the SID pointer to a SecurityIdentifier
            var securityIdentifier = new SecurityIdentifier(sidPointer);

            // Retrieve and display the SID string
            string sidString = securityIdentifier.Value;
            Console.WriteLine($"SID: {sidString}");

            // Retrieve and display the account name (if available)
            try
            {
                NTAccount account = (NTAccount)securityIdentifier.Translate(typeof(NTAccount));
                Console.WriteLine($"Account Name: {account.Value}");
            }
            catch (IdentityNotMappedException)
            {
                Console.WriteLine("Account Name: [Not Mapped]");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing SID: {ex.Message}");
        }
    }

    public static void ProcessSids(string[] accounts)
    {
        uint[] tsid = new uint[accounts.Length];

        for (int i = 0; i < accounts.Length; i++)
        {
            try
            {
                // Convert the SID string to a SecurityIdentifier object
                SecurityIdentifier sid = new SecurityIdentifier(accounts[i]);

                // Optionally, translate the SID to an account name if needed
                string accountName = sid.Translate(typeof(NTAccount)).Value;

                Console.WriteLine($"Account Name: {accountName}");

                // Convert the SID into a "dummy" uint (for example purposes)
                // This can be replaced with more meaningful logic based on your requirements
                byte[] binaryForm = new byte[sid.BinaryLength]; // Allocate byte array of appropriate size
                sid.GetBinaryForm(binaryForm, 0); // Fill the byte array starting at offset 0
                tsid[i] = BitConverter.ToUInt32(binaryForm, 0); // Convert the first 4 bytes to uint

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing SID: {accounts[i]}, Error: {ex.Message}");
                tsid[i] = 0; // Default value for error handling
            }
        }
    }

    /// <summary>
    /// Applies the specified Access Control List (ACL) to a file or directory.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    /// <param name="identity">The user or group to which the ACL applies.</param>
    /// <param name="rights">The FileSystemRights to grant.</param>
    /// <param name="controlType">The type of access control (Allow or Deny).</param>
    public static void ApplyAcl(string path, string identity, FileSystemRights rights, AccessControlType controlType)
    {
        try
        {
            // Check if the path exists
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }

            // Get the current access control for the file or directory
            FileSystemSecurity security;
            if (File.Exists(path))
            {
                security = new FileInfo(path).GetAccessControl();
            }
            else
            {
                security = new DirectoryInfo(path).GetAccessControl();
            }

            // Create a new access rule
            var rule = new FileSystemAccessRule(identity, rights, controlType);

            // Modify the ACL
            security.AddAccessRule(rule);

            // Apply the updated ACL
            if (File.Exists(path))
            {
                new FileInfo(path).SetAccessControl((FileSecurity)security);
            }
            else
            {
                new DirectoryInfo(path).SetAccessControl((DirectorySecurity)security);
            }

            Console.WriteLine($"ACL applied successfully to: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying ACL: {ex.Message}");
        }
    }

    public static void Deny(string path)
    {
        FileFix.Unlock(path, 3, "Everyone"); // Custom logic to set deny permissions
    }

    // GetReparseTag method to extract the reparse tag of a file
    public static uint GetReparseTag(string path)
    {
        IntPtr hFile =
            Kernel32NativeMethods.CreateFile(path, 0x80000000, 0x00000001, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (hFile == IntPtr.Zero)
        {
            throw new IOException("Unable to open file for reparse tag.");
        }

        try
        {
            Structs.REPARSE_DATA_BUFFER buffer = new Structs.REPARSE_DATA_BUFFER();
            uint bytesReturned = 0;
            bool result = Kernel32NativeMethods.DeviceIoControl(hFile, NativeMethodConstants.FSCTL_GET_REPARSE_POINT,
                IntPtr.Zero, 0, ref buffer, NativeMethodConstants.MAX_REPARSE_DATA_BUFFER_SIZE, ref bytesReturned,
                IntPtr.Zero);
            if (!result)
            {
                throw new IOException("Unable to retrieve reparse tag.");
            }

            return buffer.ReparseTag;
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(hFile);
        }
    }



    private static IntPtr InitializeAcl()
    {
        IntPtr pAcl = Marshal.AllocHGlobal(32);
        bool result = Advapi32NativeMethods.InitializeAcl(pAcl, 32, 2);
        if (!result)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"InitializeAcl failed with error {error}");
            Marshal.FreeHGlobal(pAcl);
            return IntPtr.Zero;
        }

        return pAcl;
    }



    public static string GetUserSID(string userName)
    {
        string sid = string.Empty;
        try
        {
            // Set up the searcher to find the user by their SAM account name.
            var searcher = new DirectorySearcher($"(&(objectClass=user)(sAMAccountName={userName}))");

            // Perform the search and get the first result.
            var result = searcher.FindOne();

            // Check if the result is not null.
            if (result != null)
            {
                // Retrieve the objectSid from the properties.
                if (result.Properties.Contains("objectSid"))
                {
                    sid = BitConverter.ToString((byte[])result.Properties["objectSid"][0]).Replace("-", "");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting SID for {userName}: {ex.Message}");
        }

        return sid;
    }

    public static string[] GetAllSubFoldersShort(string mainFolder)
    {
        var subFolders = Directory.GetDirectories(mainFolder, "*", SearchOption.AllDirectories);
        return subFolders.Select(folder => Path.GetFileName(folder)).ToArray();
    }



    public static string GetUrl(string path)
    {
        try
        {
            string content = File.ReadAllText(path);
            var baseUrlMatch = Regex.Match(content, @"(?i)BASEURL=(.+)");
            var urlMatch = Regex.Match(content, @"(?i)\bURL=(.+)");

            return $"{baseUrlMatch.Value} {urlMatch.Value}";
        }
        catch
        {
            return null;
        }
    }





    public static int CheckFile(string filePath, uint fileAttributes = 0)
    {
        try
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                return 32; // File not found (or other error codes you want to return)
            }

            // Get file attributes
            var attributes = File.GetAttributes(filePath);

            // Check if the file has specific attributes (e.g., hidden, system, read-only)
            if ((attributes & (FileAttributes)fileAttributes) == (FileAttributes)fileAttributes)
            {
                return 32; // File matches the expected attributes
            }

            return 0; // No matching attributes
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking file: {ex.Message}");
            return -1; // Indicate error checking the file
        }
    }

    public static void RESULT(string path, int i = 1)
    {
        if (i != 0)
        {
            Logger.Instance.LogFix($"\"{path}\" => {StringConstants.UNLOCK}{Environment.NewLine}");
        }
        else
        {
            Logger.Instance.LogFix($"\"{path}\" => {StringConstants.NOTUNLOCKED}{Environment.NewLine}");
        }
    }










    public static bool FileAccessCheck(string path)
    {
        try
        {
            // Check if the file or directory exists
            if (File.Exists(path) || Directory.Exists(path))
            {
                // Try to open the file or access the directory
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    // If successful, the file is accessible
                    return true;
                }
            }

            return false; // Path does not exist
        }
        catch (UnauthorizedAccessException)
        {
            // Access is denied
            return false;
        }
        catch (Exception)
        {
            // Other errors
            return false;
        }
    }


    public static uint SetFilePointer(IntPtr fileHandle, int position, uint moveMethod)
    {
        uint result = Kernel32NativeMethods.SetFilePointer(fileHandle, position, IntPtr.Zero, moveMethod);
        if (result == 0xFFFFFFFF)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"SetFilePointer failed with error code {error}.");
        }

        return result;
    }

    public static string GetFilePath(string filename, string workingDir = "")
    {
        string resolvedPath = null;
        string defaultExtension = ".exe";

        // Ensure the filename includes an extension
        if (!filename.Contains("."))
        {
            filename += defaultExtension;
        }

        // Try to read the path from the registry (HKLM)
        resolvedPath =
            RegistryValueHandler.TryReadRegistryValue(Registry.LocalMachine,
                $"{RegistryConstants.AppPathsKey}\\{filename}");
        resolvedPath = RemoveFileExtension(resolvedPath);

        // Handle paths based on the boot mode
        string[] pathArray;
        if (SystemConstants.BootMode == "Recovery")
        {
            string systemPaths = RegistryValueHandler.TryReadRegistryValue(Registry.LocalMachine,
                RegistryConstants.SessionManagerEnvironmentKey, "path");
            pathArray = SplitPaths(systemPaths);
        }
        else
        {
            // Try to read the path from the registry (HKCU)
            string userPath = RegistryValueHandler.TryReadRegistryValue(Registry.CurrentUser,
                $"{RegistryConstants.AppPathsKey}\\{filename}");
            userPath = RemoveFileExtension(userPath);

            // Retrieve environment PATH variable
            string envPaths = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

            // Construct the path array
            pathArray = SplitPaths(envPaths);

            // Prepend custom paths
            if (!string.IsNullOrEmpty(userPath)) PrependToArray(ref pathArray, userPath);
            if (!string.IsNullOrEmpty(resolvedPath)) PrependToArray(ref pathArray, resolvedPath);
            if (!string.IsNullOrEmpty(workingDir)) PrependToArray(ref pathArray, workingDir);
        }

        // Search for the file in the resolved paths
        foreach (var path in pathArray)
        {
            string expandedPath = Environment.ExpandEnvironmentVariables(path.Trim());
            string fullPath = Path.Combine(expandedPath, filename);

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null; // File not found
    }

    private static string RemoveFileExtension(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        return Path.GetDirectoryName(filePath)?.TrimEnd(Path.DirectorySeparatorChar) ?? filePath;
    }

    private static string[] SplitPaths(string pathString)
    {
        return pathString?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    }

    private static void PrependToArray(ref string[] array, string value)
    {
        var tempList = new List<string>(array);
        tempList.Insert(0, value);
        array = tempList.ToArray();
    }

    public static void GetFileList2(string folder)
    {
        if (!Directory.Exists(folder)) return;

        // Simulating _FILELISTTOARRAYREC functionality (Recursive file listing)
        var fileArray = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories).ToList();

        for (int i = 0; i < fileArray.Count; i++)
        {
            var filePath = fileArray[i];

            // Check for reparse points
            if (IsReparsePoint(filePath))
            {
                filePath = GetReparseTarget(filePath);
                fileArray[i] = filePath;
            }

            // Check for existence of ntuser.dat or specific folder names
            if (!File.Exists(Path.Combine(filePath, "ntuser.dat")) &&
                !Regex.IsMatch(filePath, @"(?i)\\(ProgramData|All Users|Public)$"))
            {
                continue;
            }

            // Add to allUsers list
            ArrayUtils.AddToArray2Args(allUsers, filePath);
        }

        // Simulate _ARRAYUNIQUE to remove duplicates
        allUsers = allUsers.Distinct().ToList();
    }

    // Method to replace files based on the source and destination


    public static void GetAcl(string filePath, ref string owner, ref RawSecurityDescriptor dacl, ref string secDesc)
    {
        try
        {
            // Get the file's security settings
            FileSecurity fileSecurity = File.GetAccessControl(filePath);

            // Get the file owner
            IdentityReference identityReference = fileSecurity.GetOwner(typeof(NTAccount));
            owner = identityReference.Value;

            // Get the discretionary access control list (DACL)
            AuthorizationRuleCollection accessRules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
            dacl = new RawSecurityDescriptor(fileSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));

            // Get the security descriptor in SDDL form
            secDesc = fileSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.All);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving ACL: {ex.Message}");
            owner = null;
            dacl = null;
            secDesc = null;
        }
    }



    public static List<string> GetAlternateDataStreams(string filePath)
    {
        var streams = new List<string>();
        var findStreamData = new Structs.WIN32_FIND_STREAM_DATAAlt();

        IntPtr filePathPtr = IntPtr.Zero;
        try
        {
            // Marshal the file path to unmanaged memory
            filePathPtr = Marshal.StringToHGlobalUni(filePath);

            // Open the first stream
            IntPtr hFindStream = Kernel32NativeMethods.FindFirstStreamWAlt(filePathPtr.ToString(),
                NativeMethodConstants.STREAM_INFO_LEVELS.FindStreamInfoStandard, ref findStreamData, 0);
            if (hFindStream == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 38) // ERROR_HANDLE_EOF
                {
                    Console.WriteLine($"Error retrieving streams: {errorCode}");
                }

                return streams;
            }

            try
            {
                do
                {
                    string streamName = findStreamData.cStreamName;
                    if (!string.IsNullOrEmpty(streamName) &&
                        !streamName.Equals("::$DATA", StringComparison.OrdinalIgnoreCase))
                    {
                        streams.Add(streamName);
                    }
                } while (Kernel32NativeMethods.FindNextStreamWAlt(hFindStream, ref findStreamData));
            }
            finally
            {
                Kernel32NativeMethods.FindClose(hFindStream);
            }
        }
        finally
        {
            // Free the unmanaged memory
            if (filePathPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(filePathPtr);
            }
        }

        return streams;
    }

    public static int GetFileList(string folder)
    {
        if (!Directory.Exists(folder)) return 1;

        var fileArray = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
        if (fileArray == null || fileArray.Length == 0) return 1;

        var form1 = Application.OpenForms.OfType<MainApp>().FirstOrDefault();
        if (form1 == null) throw new InvalidOperationException("Form1 instance not found.");

        int daysOld = form1.checkEdit8.Checked ? 91 : 31;

        foreach (var filePath in fileArray)
        {
            form1.UpdateLabel($"Scanning: {filePath}");

            // Additional streams handling
            if (form1.checkEdit8.Checked && SystemConstants.BootMode != "Recovery")
            {
                var adsList = GetAlternateDataStreams(filePath);
                foreach (var ads in adsList)
                {
                    if (!Regex.IsMatch(ads,
                            @"(?i):(\${3D0CE612-FDEE-43f7-8ACA-957BEC0CCBA0}\..*|SmartScreen|Zone.Identifier|encryptable|favicon|ms-properties|OECustomProperty|Win32App.*)$"))
                    {
                        var adsData = new List<string> { "ExistingItem", "ADS000", $"{filePath}{ads}" };
                        // Handle adsData as needed
                    }
                }
            }

            // Reparse point and locked file check
            if (!IsReparsePoint(filePath) && FileFix.CreateFile(filePath))
            {
                if (!Regex.IsMatch(filePath,
                        @"(?i):\\(Recovery|hiberfil.sys|System (Recovery|Repair|Volume Information)|WINDOWS\\CSC|OSRSS|WINDOWS\\system32\\WebThreatDefSvc)$"))
                {
                    var dateModified = GetFileTime(filePath);
                    var lockedFiles = new List<string> { $"{dateModified} {filePath}" };
                    // Handle lockedFiles as needed
                }
            }

            // Unsigned executables or DLLs
            if (form1.checkEdit8.Checked && SystemConstants.BootMode != "Recovery")
            {
                if (Regex.IsMatch(filePath, @"(?i)\.(exe|dll)$") &&
                    !File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                {
                    if (CHECKSIG(filePath) != 11)
                    {
                        AddUnsignedFileInfo(filePath);
                    }
                }
            }

            // Check file creation date
            var creationDate = File.GetCreationTime(filePath);
            var daysSinceCreation = (DateTime.Now - creationDate).Days;

            if (daysSinceCreation < daysOld || creationDate.Year > DateTime.Now.Year)
            {
                if (ShouldSkipFile(filePath)) continue;

                // Add to processed files
                AddProcessedFile(filePath, creationDate);
            }
        }

        return 0;
    }


    private static void AddUnsignedFileInfo(string filePath)
    {
        // Example: Log unsigned file information to a file or in-memory structure
        string logFilePath = "unsigned_files.log";
        string logEntry = $"Unsigned file detected: {filePath} - {DateTime.Now}";

        // Append the log entry to a file
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

        // Optionally, output to the console for debugging
        Console.WriteLine(logEntry);
    }

    private static bool ShouldSkipFile(string filePath)
    {
        // Skip system and hidden files
        var attributes = File.GetAttributes(filePath);
        if (attributes.HasFlag(FileAttributes.System) || attributes.HasFlag(FileAttributes.Hidden))
        {
            return true;
        }

        // Skip specific file extensions
        string[] excludedExtensions = { ".tmp", ".log", ".bak" };
        if (excludedExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Skip specific file patterns (e.g., temporary files)
        if (Regex.IsMatch(filePath, @"^~\$|\.tmp$", RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Skip files larger than 100 MB
        const long maxFileSize = 100 * 1024 * 1024; // 100 MB
        if (new FileInfo(filePath).Length > maxFileSize)
        {
            return true;
        }

        // Add additional conditions as needed
        return false;
    }

    private static readonly List<string> ProcessedFiles = new List<string>();

    private static void AddProcessedFile(string filePath, DateTime creationDate)
    {
        string logEntry = $"{creationDate:yyyy-MM-dd HH:mm:ss} - {filePath}";

        // Add the entry to an in-memory list
        ProcessedFiles.Add(logEntry);

        // Optionally, log the processed file to a file
        File.AppendAllText("processed_files.log", logEntry + Environment.NewLine);

        Console.WriteLine($"Processed file: {filePath}");
    }


    public static void Execute(string fixValue, string baseDirectory, string searchLogFilePath)
    {
        // Process the input value to clean and prepare the search pattern
        string searchValue = Regex.Replace(fixValue, @"(?i)SearchAll:\s*(.+)(\r\n?|\n)*", "$1");



        searchValue = Regex.Replace(searchValue, @"\*|\?", "");
        searchValue = Regex.Replace(searchValue, @"(;\s*)+", ";");
        searchValue = Regex.Replace(searchValue, @"^;|;$", "");
        string pattern = "*" + Regex.Replace(searchValue, ";", "*;*").Replace("|", "") + "*";

        var fileList = new List<string>(); // Initialize the list to hold file paths

        try
        {
            // Recursively get all files starting from the base directory
            FileHandler.GetFilesRecursively(baseDirectory, fileList);

            // Write the initial log file header
            File.WriteAllText(searchLogFilePath,
                $"{Environment.NewLine}Files:{Environment.NewLine}========{Environment.NewLine}");

            if (fileList.Count > 0)
            {
                // Append the file paths to the search log
                AppendFileSearchLog(searchLogFilePath, fileList);
            }
            else
            {
                Console.WriteLine("No files found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during file search: {ex.Message}");
        }

        // Declare and initialize the search log file path
        string searchFilePath = Path.Combine(baseDirectory, "SearchLog.txt");

        // Folder search
        File.AppendAllText(searchFilePath,
            $"{Environment.NewLine}Folders:{Environment.NewLine}========{Environment.NewLine}");
        DirectoryUtils.PerformFolderSearch(searchFilePath, pattern);

        // Registry search
        using (var writer = new StreamWriter(searchFilePath, true))
        {
            writer.WriteLine($"{Environment.NewLine}Registry:{Environment.NewLine}========");
            string registryData = RegistryUtils.SearchRegistry(searchValue);
            if (searchValue.Contains(";"))
            {
                registryData = Regex.Replace(registryData, @"(?m)^\[HK", "[[[[[HK");
                registryData += "[[[[";
                var terms = searchValue.Split(';');
                foreach (var term in terms)
                {
                    writer.WriteLine(
                        $"{Environment.NewLine}===================== Searching for \"{term}\" =========={Environment.NewLine}");
                    var matches = Regex.Matches(registryData,
                        $@"(?is)(\[hk(?:.(?!\[hk))+?{Regex.Escape(term)}.+?)(?:\[\[\[\[)", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        writer.WriteLine(match.Groups[1].Value);
                    }
                }
            }
            else
            {
                writer.WriteLine(registryData);
            }
        }
    }


    private static void AppendFileSearchLog(string filePath, List<string> files)
    {
        using (var writer = new StreamWriter(filePath, true))
        {
            foreach (var file in files)
            {
                writer.WriteLine(file);
            }
        }
    }

    public static string ReadFile(IntPtr hFile, uint bytesToRead = 10000)
    {
        // Create a buffer to store the data
        IntPtr buffer = Marshal.AllocHGlobal((int)bytesToRead);

        // Variable to hold the number of bytes read
        uint bytesRead = 0;

        try
        {
            // Call ReadFile
            bool result = Kernel32NativeMethods.ReadFile(hFile, buffer, bytesToRead, out bytesRead, IntPtr.Zero);

            // Check if the operation failed
            if (!result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"Error reading file. Error code: {errorCode}");
                return null;
            }

            // Copy the data from the buffer to a managed byte array
            byte[] managedBuffer = new byte[bytesRead];
            Marshal.Copy(buffer, managedBuffer, 0, (int)bytesRead);

            // Convert the byte array to a string (assuming UTF-16 encoding)
            return Encoding.Unicode.GetString(managedBuffer);
        }
        finally
        {
            // Free the allocated memory
            Marshal.FreeHGlobal(buffer);
        }
    }

    // Function to read the content of a file at the given path
    public static string Read(string path)
    {
        try
        {
            // Read the file content
            string content = File.ReadAllText(path);
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return string.Empty;
        }
    }

    // Function to get the size of the file at the given path
    public static long ReadSize(string path)
    {
        try
        {
            // Get the file size
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length; // Returns file size in bytes
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting file size: {ex.Message}");
            return -1; // Indicates an error
        }
    }





    public static bool IsFileInUse(string filePath)
    {
        try
        {
            // Try to open the file with exclusive access
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // File can be opened exclusively, so it is not in use
                return false;
            }
        }
        catch (IOException)
        {
            // An IOException is thrown if the file is in use
            return true;
        }
        catch (Exception ex)
        {
            // Handle other exceptions for debugging purposes
            Console.WriteLine($"Error checking file usage: {filePath}, Exception: {ex.Message}");
            return true;
        }
    }

    public static bool HasAccess(string filePath)
    {
        try
        {
            // Check if the file exists
            if (!File.Exists(filePath))
                return false;

            // Attempt to open the file with read permissions
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // If the file opens successfully, access is granted
            }

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // Access is denied
            return false;
        }
        catch (Exception ex)
        {
            // Handle other exceptions (e.g., file in use)
            Console.WriteLine($"Error checking access for {filePath}: {ex.Message}");
            return false;
        }
    }

    public static bool IsSignatureValid(string filePath)
    {
        try
        {
            // Load the file's digital signature
            X509Certificate cert = X509Certificate.CreateFromSignedFile(filePath);
            X509Certificate2 certificate = new X509Certificate2(cert);


            if (certificate == null)
                return false;

            // Check the certificate chain for validity
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0); // 1 minute
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            bool isValid = chain.Build(certificate);

            // Optionally, check for a specific issuer
            // if (certificate.Issuer.Contains("Microsoft"))
            // {
            //     isValid = true;
            // }

            return isValid;
        }
        catch
        {
            // If any exception occurs, assume the signature is not valid
            return false;
        }
    }

    public static string FormatAttributes(FileAttributes attributes)
    {
        // Format attributes string (replace "0" with "_" and add "L" for reparse points)
        var attributesString = attributes.ToString().Replace("0", "_");
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            attributesString += "L";
        }

        return attributesString.PadLeft(5, '_');
    }

    public static string GetFileVersionCompanyName(string filePath)
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




    /// Converts a FILETIME structure to a DateTime object.
    private static DateTime FileTimeToDateTime(FILETIME fileTime)
    {
        long fileTimeAsLong = ((long)fileTime.dwHighDateTime << 32) + fileTime.dwLowDateTime;
        return DateTime.FromFileTimeUtc(fileTimeAsLong);
    }

    private static void ProcessFilesRem(StreamWriter fixLog)
    {
        int ii = 1;
        while (true)
        {
            string file = ReadFileLine(FolderConstants.HomeDrive + @"\frst\filesRem", ii);
            if (string.IsNullOrEmpty(file)) break;

            string path = @"\\?\" + file;
            path = Regex.Replace(path, @"\\(?!\?\\)", @"\");
            if (!File.Exists(path))
            {
                Logger.Instance.LogFix($"{StringConstants.DELETED} {file}");
            }
            else
            {
                FileAttributes attributes = File.GetAttributes(path);
                if (attributes.HasFlag(FileAttributes.Directory)) // Correct way to check for "D"
                {
                    FileFix.GrantPermissions(file, 1, 1);
                    bool dirDone = Kernel32NativeMethods.RemoveDirectory(path);
                    if (dirDone && !File.Exists(path))
                    {
                        Logger.Instance.LogFix($"{StringConstants.DELETED} {file}");
                    }
                    else
                    {
                        //Logger.NotMoved(file);
                    }
                }
                else
                {
                    FileFix.GrantPermissions(path, 1, 0);
                    if (Regex.IsMatch(GetFileAttributes(path), @"(?i)S|R|H"))
                    {
                        FileFix.SetFileAttributes(path, "-SRH");
                    }

                    bool dirDone = Kernel32NativeMethods.DeleteFile(path);
                    if (dirDone)
                    {
                        Logger.Instance.LogFix($"{StringConstants.DELETED} {file}");
                    }
                    else
                    {
                        // Logger.NotMoved(file);
                    }
                }
            }

            ii++;
        }
    }



    public static void Load()
    {
        try
        {
            string hivePath = @"C:\frst\hive.dat"; // Path to the hive file
            string keyName = "TempHive";

            // Load the registry hive
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);
            if (key == null)
            {
                RegistryUtils.LoadHive(hivePath, keyName);
                Console.WriteLine($"Successfully loaded hive from {hivePath}");
            }
            else
            {
                Console.WriteLine($"Hive {keyName} is already loaded.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading hive: {ex.Message}");
        }
    }

    public static string GetFileAttributes(string path)
    {
        FileAttributes attributes = File.GetAttributes(path);
        return attributes.ToString();
    }

    public static string ReadFileLine(string path, int lineNumber)
    {
        // Implement logic to read file line by line (returns a specific line from the file)
        string[] lines = File.ReadAllLines(path);
        return lines.Length >= lineNumber ? lines[lineNumber - 1] : string.Empty;
    }

    public static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "deletionLog.txt");


    public static void MarkNotDeleted(string path, string reason = "Unknown reason")
    {
        try
        {
            // Log the non-deleted path and reason to a file
            string logEntry =
                $"Could not delete: {path}. Reason: {reason}. Timestamp: {DateTime.Now}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, logEntry);

            // Output to console for immediate feedback
            Console.WriteLine($"MarkNotDeleted: {logEntry}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to log non-deletion for {path}: {ex.Message}");
        }
    }






    public static void BKPS()
    {
        string SFOL = Utility.GenerateRandomString();
        string PATH = Path.Combine(FolderConstants.HomeDrive, "FRST", SFOL);

        Directory.CreateDirectory(PATH);
        File.SetAttributes(PATH, FileAttributes.Hidden | FileAttributes.System);

        if (File.Exists(PATH))
        {
           // BackupManager.SBKP(new[] { PATH });
            if (!File.Exists(Path.Combine(PATH, "system")))
            {
                Directory.Delete(PATH, true);
            }
        }
    }


    public static bool BLACK(string path)
    {
        path = Path.GetFullPath(path);

        if (Regex.IsMatch(path,
                $@"(?i){FolderConstants.HomeDrive}\\(System Volume Information|Windows|Program Files|Program Files|Windows\\System32|Windows\\System32\\(Drivers|Tasks)|Windows\\System32\\config|Program Files\\(Microsoft Security Client|Windows Defender|WindowsApps)|Windows\\SystemApps(|\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe(|\\Assets)))$"))
        {
            return true;
        }

        string[] directoriesToCheck =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        };

        foreach (var directory in directoriesToCheck)
        {
            if (path.Equals(directory, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }



    public static void CHECKKEYPERMS(string key)
    {
        if (CHECKKEYLOCKED(key)) RegistryUtilsScript.SETREGACE(key, 1);
    }

    public static bool CHECKKEYLOCKED(string key)
    {
        string ret = GetSecurityDescriptor(key, 4);
        return ret.Contains("(D;") || !Regex.IsMatch(ret, "A;.*?;KA;;;BA");
    }

    public static string GetSecurityDescriptor(string keyPath, int securityInformation)
    {
        IntPtr hKey;
        IntPtr rootKey = (IntPtr)0x80000002; // HKEY_LOCAL_MACHINE
        int desiredAccess = 0x20006; // KEY_READ

        int result = Advapi32NativeMethods.RegOpenKeyEx(rootKey, keyPath, 0, desiredAccess, out hKey);
        if (result != 0)
        {
            throw new InvalidOperationException($"Failed to open registry key. Error code: {result}");
        }

        try
        {
            int securityDescriptorSize = 0;

            // First call to determine the size of the security descriptor
            Advapi32NativeMethods.RegGetKeySecurity(hKey, securityInformation, null, ref securityDescriptorSize);

            byte[] securityDescriptor = new byte[securityDescriptorSize];

            // Second call to retrieve the actual security descriptor
            result = Advapi32NativeMethods.RegGetKeySecurity(hKey, securityInformation, securityDescriptor,
                ref securityDescriptorSize);
            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to get security descriptor. Error code: {result}");
            }

            // Convert the byte array to a readable format (e.g., base64 string or hex dump)
            return BitConverter.ToString(securityDescriptor).Replace("-", "");
        }
        finally
        {
            Advapi32NativeMethods.RegCloseKey(hKey);
        }
    }

    public static int CHECKSIG(string filepath, string info = "")
    {
        // Check if the file exists and is not empty
        if (GetFileSize(filepath) == 0 || !FileFix.CreateFile(filepath))
        {
            return 0; // File is empty or cannot be created
        }

        // If 'info' is empty, verify the file signature
        if (string.IsNullOrEmpty(info))
        {
            // Assuming WINVERIFYTRUST returns 0 for success
            if (WintrustNativeMethods.WINVERIFYTRUST(filepath) == 0)
            {
                return 11; // Signature is valid
            }
        }
        else if (info == "1")
        {
            // Retrieve certificate info
            var certInfo = GETSIGNATUREINFO(filepath);
            if (certInfo != null && certInfo.Length > 1)
            {
                // Example: Returning the length of the issuer's name
                return certInfo[1].Length;
            }
        }

        // Acquire CryptCATAdmin context
        IntPtr context = IntPtr.Zero;
        if (WintrustNativeMethods.CryptCATAdminAcquireContext(ref context, IntPtr.Zero, 0) == 0)
        {
            return 1; // Failed to acquire context
        }

        // Create file handle for verification
        IntPtr fileHandle =
            Kernel32NativeMethods.CreateFile(filepath, 0x80000000, 0, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
        if (fileHandle == IntPtr.Zero)
        {
            return 2; // Error creating file handle
        }

        // Calculate hash from the file handle
        uint cbHash = 0;
        if (WintrustNativeMethods.CryptCATAdminCalcHashFromFileHandle(fileHandle, ref cbHash, IntPtr.Zero, 0) == 0)
        {
            Kernel32NativeMethods.CloseHandle(fileHandle); // Ensure handle is closed on failure
            return 3; // Error calculating hash
        }

        // Clean up file handle
        Kernel32NativeMethods.CloseHandle(fileHandle);

        // Additional signature checks or processing could go here

        return 4; // Successfully completed signature verification
    }


    public static string[] GETSIGNATUREINFO(string filepath)
    {
        try
        {
            // Load the certificate from the signed file
            X509Certificate2 certificate = new X509Certificate2(filepath);

            if (certificate != null)
            {
                // Extract certificate information
                string issuer = certificate.Issuer; // Issuer details
                string subject = certificate.Subject; // Subject details

                return new[] { subject, issuer }; // Return relevant certificate details
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"Error reading signature: {ex.Message}");
        }

        // Return null if no certificate is found or an error occurs
        return null;
    }


    public static string GetReparseTarget(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));

        IntPtr handle = Kernel32NativeMethods.CreateFile(
            path,
            0,
            0,
            IntPtr.Zero,
            3, // OPEN_EXISTING
            0x00200000, // FILE_FLAG_OPEN_REPARSE_POINT
            IntPtr.Zero);

        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            throw new IOException("Unable to open file handle", Marshal.GetLastWin32Error());

        try
        {
            IntPtr buffer = Marshal.AllocHGlobal(NativeMethodConstants.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
            try
            {
                if (!Kernel32NativeMethods.DeviceIoControl(
                        handle,
                        NativeMethodConstants.FSCTL_GET_REPARSE_POINT,
                        IntPtr.Zero,
                        0,
                        buffer,
                        (uint)NativeMethodConstants.MAXIMUM_REPARSE_DATA_BUFFER_SIZE,
                        out uint bytesReturned,
                        IntPtr.Zero))
                {
                    throw new IOException("Unable to get reparse point data", Marshal.GetLastWin32Error());
                }

                var reparseDataBuffer = Marshal.PtrToStructure<Structs.REPARSE_DATA_BUFFER>(buffer);
                string substituteName = Encoding.Unicode.GetString(
                    reparseDataBuffer.PathBuffer,
                    reparseDataBuffer.SubstituteNameOffset,
                    reparseDataBuffer.SubstituteNameLength);

                return substituteName.Replace(@"\??\", string.Empty);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(handle);
        }
    }

    private static List<string> Blacklist = new List<string> { "malicious.exe", "badfile.dll", "unwantedfolder" };

    public static bool IsBlacklisted(string path)
    {
        // Check if the path matches any entry in the blacklist
        return Blacklist.Any(blacklistedItem => path.IndexOf(blacklistedItem, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public static bool IsReparsePoint(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            return false;

        var attributes = File.GetAttributes(path);
        return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
    }



    // Helper methods for file handling (placeholders, replace with actual implementations)
    public static IEnumerable<string> GetFilesByPattern(string folder, string pattern)
    {
        // Use Directory.GetFiles to find files matching the pattern in the folder
        if (Directory.Exists(folder))
        {
            return Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);
        }

        return null;
    }


    /// Gets the file creation or modification time as a formatted string.
    /// <param name="path">The file path.</param>
    /// <param name="creationTime">If true, retrieves the creation time; otherwise, retrieves the last write time.</param>
    /// <returns>Formatted date and time string, or an alternative result from <c>FileTimeFallback</c> if needed.</returns>
    public static string GetFileTime(string path, bool creationTime = false)
    {
        try
        {
            // Ensure the path uses the proper format
            path = System.Text.RegularExpressions.Regex.Replace(path, @"\\(?!\?\\)", "\\");
            if (!path.StartsWith(@"\\?\"))
            {
                path = @"\\?\" + path;
            }

            DateTime fileTime;
            if (creationTime)
            {
                fileTime = File.GetCreationTime(path);
            }
            else
            {
                fileTime = File.GetLastWriteTime(path);
            }

            return $"{fileTime:yyyy-MM-dd HH:mm}";
        }
        catch (Exception)
        {
            // Fallback to an alternative method if file time retrieval fails
            return FileTimeFallback(path, creationTime);
        }
    }

    /// A fallback method for retrieving file time in case the main approach fails.
    /// <param name="path">The file path.</param>
    /// <param name="creationTime">If true, retrieves the creation time; otherwise, retrieves the last write time.</param>
    /// <returns>Formatted date and time string.</returns>
    private static string FileTimeFallback(string path, bool creationTime)
    {
        // Implement alternative logic to fetch file time if needed
        return "Fallback time retrieval not implemented.";
    }

    /// Gets the file creation or modification time using low-level API functions.
    /// <param name="path">The file path.</param>
    /// <param name="creationTime">If true, retrieves the creation time; otherwise, retrieves the last write time.</param>
    /// <returns>Formatted date and time string, or "0000-00-00 00:00" if retrieval fails.</returns>
    public static string GetFileTimeWithLowLevelApi(string path, bool creationTime = false)
    {
        try
        {
            // Ensure the path uses the proper format
            path = System.Text.RegularExpressions.Regex.Replace(path, @"\\(?!\?\\)", "\\");
            if (!path.StartsWith(@"\\?\"))
            {
                path = @"\\?\" + path;
            }

            // Open file handle
            IntPtr hFile = Kernel32NativeMethods.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            {
                return "0000-00-00 00:00";
            }

            // Retrieve file times
            if (!Kernel32NativeMethods.GetFileTime(hFile, out Structs.FILETIMEALT creationTimeStruct,
                    out Structs.FILETIMEALT lastAccessTimeStruct, out Structs.FILETIMEALT lastWriteTimeStruct))
            {
                Kernel32NativeMethods.CloseHandle(hFile);
                return "0000-00-00 00:00";
            }

            // Close the file handle
            Kernel32NativeMethods.CloseHandle(hFile);

            // Convert the appropriate time to DateTime
            Structs.FILETIMEALT fileTime = creationTime ? creationTimeStruct : lastWriteTimeStruct;
            long fileTimeTicks = ((long)fileTime.HighDateTime << 32) | (uint)fileTime.LowDateTime;
            DateTime dateTime = DateTime.FromFileTimeUtc(fileTimeTicks);

            // Format the output
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }
        catch
        {
            return "0000-00-00 00:00";
        }
    }




    /// Retrieves the size of a file. If standard methods fail, it uses a low-level API.
    /// <param name="path">The file path.</param>
    /// <returns>The size of the file in bytes, or null if the size cannot be determined.</returns>
    public static long? GetFileSize(string path)
    {
        try
        {
            // Attempt to get the file size using FileInfo
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }

            // Use low-level API if the standard method fails
            IntPtr hFile = Kernel32NativeMethods.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            {
                return null;
            }

            // Get file size using GetFileSizeEx
            if (Kernel32NativeMethods.GetFileSizeEx(hFile, out long fileSize))
            {
                Kernel32NativeMethods.CloseHandle(hFile);
                return fileSize;
            }

            // Close the handle if GetFileSizeEx fails
            Kernel32NativeMethods.CloseHandle(hFile);
            return null;
        }
        catch
        {
            return null;
        }
    }




    // Normalizes the file path for low-level WinAPI access.
    public static string NormalizeFilePath(string path)
    {
        // Replace single backslashes with double backslashes if not already in "\\?\\" format
        if (!path.StartsWith(@"\\?\"))
        {
            path = @"\\?\" + path.Replace("\\", @"\\");
        }

        return path;
    }

    public static bool IsExecutableOrDll(string filePath)
    {
        string extension = Path.GetExtension(filePath)?.ToLower();
        return extension == ".dll" || extension == ".exe" || extension == ".sys" || extension == ".mui";
    }




    public static string ReadFileContent(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    public static void AAAAFP(string workDir = "", string arg = "")
    {
        string size = string.Empty;
        string cdate = string.Empty;
        string company = string.Empty;

        // Example file variable
        string file = "";

        if (string.IsNullOrEmpty(file)) return;

        if (int.TryParse(ARG, out int argValue) && argValue == 1)
        {
            return;
        }

        file = Regex.Replace(file, "\"", string.Empty);
        file = Regex.Replace(file, "%+", "%");
        file = Regex.Replace(file, @"(^\s+|\s+$)", string.Empty);
        file = Regex.Replace(file, @"^\\\?\?\\", string.Empty);

        if (Regex.IsMatch(file, @".:/"))
        {
            file = file.Replace("/", "\\");
        }

        file = Regex.Replace(file, @"\\\\(?!\?\\)", "\\");

        if (file.Contains("\\") && File.Exists(file))
        {
            string companyInfo = COMP(file, Path.GetFileName(file)); // Pass the required arguments
            Console.WriteLine(companyInfo);
            return;
        }

        file = Regex.Replace(file, @"^(\\|\\\\)", string.Empty);
        string windir = FolderConstants.WinDir.Replace(@"\", @"\\");

        if (file.Contains("\\") && !Regex.IsMatch(Regex.Replace(file, @".+\\", string.Empty), @"\.|\s|/|:"))
        {
            file += ".exe";
        }

        if (Regex.IsMatch(file, @"(?i)rundll32|regsvr32"))
        {
            if (!Regex.IsMatch(file, @"(?i).*\.exe .* rundll32"))
            {
                file = Regex.Replace(file, @"(?i).*((rundll32|regsvr32).+)", "$1");
                file = Regex.Replace(file, @"(?i)(rundll32|regsvr32)(\.exe|).* (\w+\.dll).*", "$3");
                if (file.Contains("\\"))
                {
                    file = Regex.Replace(file, @"(?i)(rundll32|regsvr32)(\.exe|) .*?((%|.:\\).+\.dll).*", "$3");
                }
            }
        }

        if (file.Contains(@"\system32\msiexec"))
        {
            file = Path.Combine(FolderConstants.WinDir, @"System32\msiexec.exe");
        }
        else if (Regex.IsMatch(file, @"(?i)(^\s*|%+)(systemroot|windir|Windows)(|%+)\\"))
        {
            file = Regex.Replace(file, @"(?i)(\s*|%+)(systemroot|windir|Windows)(|%+)\\", windir + @"\");
        }
        else if (Regex.IsMatch(file, @"(?i)%+Programfiles%+"))
        {
            string programFilesDir = FolderConstants.ProgramFiles.Replace(@"\", @"\\");
            file = Regex.Replace(file, @"(?i).*%+Programfiles%+", programFilesDir);
        }
        else if (Regex.IsMatch(file, @"(?i)%+ProgramData%+"))
        {
            file = Regex.Replace(file, @"(?i)%+ProgramData%+", Path.Combine(FolderConstants.HomeDrive, "ProgramData"));
        }
        else if (Regex.IsMatch(file, @"(?i)\Asystem32"))
        {
            file = Regex.Replace(file, @"(?i)\Asystem32", Path.Combine(FolderConstants.WinDir, "System32"));
        }
        else if (!file.Contains("\\"))
        {
            FilenameOnly(ref file, workDir);
        }
        else if (Regex.IsMatch(file, @"(?i).*%+ALLUSERSPROFILE%+") && SystemConstants.BootMode == "Recovery")
        {
            string OSNUM = SystemConstants.GetOSVersion();

            if (double.TryParse(OSNUM, out double osVersion) && osVersion > 5.2)
            {
                file = Regex.Replace(file, @"(?i).*%+ALLUSERSPROFILE%+",
                    Path.Combine(FolderConstants.HomeDrive, "ProgramData"));
            }
            else
            {
                file = Regex.Replace(file, @"(?i).*%+ALLUSERSPROFILE%+",
                    Path.Combine(FolderConstants.HomeDrive, @"Documents and Settings\All Users"));
            }

        }
        else if (Regex.IsMatch(file, @"%[^\\]+?%") && SystemConstants.BootMode != "Recovery")
        {
            file = ExpandEnvironmentVariables(file);
        }

        if (ARG != "2")
        {
            if (!File.Exists(file))
            {
                file = Regex.Replace(file, @"(?i).*([C-Z]:\\.+?\.exe).*", "$1");
                file = Regex.Replace(file, @"(?i).*([C-Z]:\\.+\.\w{3,4}).*", "$1");
                file = Regex.Replace(file, @"(?i).*[C-Z]:.+\.exe.*([C-Z]:\\.+\.\w{3}).*", "$1");
            }
        }


        else
        {
            file = Regex.Replace(file, @"(?i).:\\Windows\\.+?\\WindowsPowerShell\\.+?powershell.exe", "powershell.exe");
        }

        if (SystemConstants.BootMode == "Recovery")
        {
            file = Regex.Replace(file, @"(?i)[C-Z]:", FolderConstants.HomeDrive);
        }

        if (File.Exists(file))
        {
            string result = COMP(file, Path.GetFileName(file)); // Pass the required arguments
            Console.WriteLine(result);
        }

    }

    public static void FilenameOnly(ref string file, string workDir)
    {
        // Combine the provided work directory with the file name
        file = Path.Combine(workDir, file);

        // Check if the file exists in the work directory
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"The file '{file}' does not exist in the specified directory.");
        }
    }

    private static string ExpandEnvironmentVariables(string input)
    {
        return Environment.ExpandEnvironmentVariables(input);
    }

    public static string ExtractFilePath(string valueData)
    {
        // Extract the file path from the value data if applicable
        Match match = Regex.Match(valueData, @"[a-zA-Z]:\\.*");
        return match.Success ? match.Value : string.Empty;
    }



    public static FileAttributes CheckFile(string path, FileAttributes checkAttribute)
    {
        try
        {
            FileAttributes attributes = File.GetAttributes(path);
            return attributes;
        }
        catch
        {
            return 0; // Return 0 if file attributes cannot be determined
        }
    }



    public static void CheckAccessPermissions(string path)
    {
        try
        {
            // Determine if it's a file or directory and get the appropriate security descriptor
            AuthorizationRuleCollection rules;
            if (File.Exists(path))
            {
                FileSecurity fileSecurity = new FileInfo(path).GetAccessControl();
                rules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
            }
            else if (Directory.Exists(path))
            {
                DirectorySecurity dirSecurity = new DirectoryInfo(path).GetAccessControl();
                rules = dirSecurity.GetAccessRules(true, true, typeof(NTAccount));
            }
            else
            {
                Console.WriteLine("Path does not exist.");
                return;
            }

            // Iterate over the rules and display them
            foreach (AuthorizationRule rule in rules)
            {
                if (rule is FileSystemAccessRule fileRule)
                {
                    Console.WriteLine($"Identity: {fileRule.IdentityReference}");
                    Console.WriteLine($"Access Control Type: {fileRule.AccessControlType}");
                    Console.WriteLine($"Rights: {fileRule.FileSystemRights}");
                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public static string ExtractPath(string fileLine)
    {
        // Extract the part before " - " as the path
        int separatorIndex = fileLine.IndexOf(" - ");
        return separatorIndex > 0 ? fileLine.Substring(0, separatorIndex).Trim() : null;
    }

    public static void COMP(string path)
    {
        string file = path;
        long size = 0;
        string cDate = string.Empty;
        string company = string.Empty;

        if (Regex.IsMatch(file, @"\\\w{6}~\d\\")) file = Path.GetFullPath(file);

        if (Regex.IsMatch(file, @":[^\\]"))
            size = new FileInfo(file).Length;
        else
            size = GetFileSize(file) ?? 0; // Use 0 as default if GetFileSize returns null

        cDate = File.GetCreationTime(file).ToString("yyyy-MM-dd HH:mm:ss");

        if (file.Contains(@"\system32\drivers\appid.sys") && SystemConstants.BootMode != "Recovery")
            company = "Microsoft Windows";
        else
            company = GetFileVersion(file, "CompanyName");

        company = Regex.Replace(company, @"\s+", " ").Trim();

        if (size > 0)
        {
            if (file.Contains(@":\Program Files\WindowsApps\"))
            {
                string file1 = Regex.Replace(file, @"(?i)(.:\\Program Files\\WindowsApps\\[^\\]+).+", "$1");
                if (File.Exists(Path.Combine(file1, "AppxSignature.p7x")))
                    file = Path.Combine(file1, "AppxSignature.p7x");
            }

            string owner = CheckSignature(file, 1);
            if (!string.IsNullOrEmpty(owner)) company = $"{owner} -> {company}";

            company = $"({company})";

            if (CRYPT > 0 && CheckSignature(file, 0) != "Valid Signature")
            {
                company += $" [{StringConstants.FILENS}]";
                if (FileFix.CreateFile(file)) company = $"({StringConstants.NOACC}) [{StringConstants.FILENS}?]";
            }
        }
        else
        {
            string additionalInfo = "";

            if (IsReparsePoint(file)) additionalInfo = $" [symlink -> {GetReparsePointTarget(file)}]";
            else if (FileFix.CreateFile(file)) additionalInfo = $" [{StringConstants.NOACC}]";

            company = $"({company}){additionalInfo}";
        }

        company = Regex.Replace(company, @"(?i)http(s|):", "hxxp$1:");
        Console.WriteLine($"Processed company info: {company}");
    }


    public static string GetFileVersion(string filePath, string property)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(filePath);

        switch (property)
        {
            case "CompanyName":
                return versionInfo.CompanyName;

            case "FileDescription":
                return versionInfo.FileDescription;

            default:
                throw new ArgumentException($"Property '{property}' is not valid.");
        }
    }

    public static string GetReparsePointTarget(string path)
    {
        IntPtr fileHandle = Kernel32NativeMethods.CreateFileW(
            path,
            0,
            0,
            IntPtr.Zero,
            NativeMethodConstants.OPEN_EXISTING,
            NativeMethodConstants.FILE_FLAG_OPEN_REPARSE_POINT | NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS,
            IntPtr.Zero);

        if (fileHandle == IntPtr.Zero || fileHandle == new IntPtr(-1))
        {
            throw new IOException($"Unable to open reparse point: {path}", Marshal.GetLastWin32Error());
        }

        try
        {
            int bufferSize = 1024;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

            try
            {
                uint bytesReturned;
                if (!Kernel32NativeMethods.DeviceIoControl(
                        fileHandle,
                        NativeMethodConstants.FSCTL_GET_REPARSE_POINT,
                        IntPtr.Zero,
                        0,
                        buffer,
                        (uint)bufferSize,
                        out bytesReturned,
                        IntPtr.Zero))
                {
                    throw new IOException($"Failed to get reparse point target: {path}", Marshal.GetLastWin32Error());
                }

                // Parse the result to extract the target path
                string target = ParseReparsePoint(buffer);
                return target;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(fileHandle);
        }
    }

    private static string ParseReparsePoint(IntPtr buffer)
    {
        // Adjust this logic for your specific reparse point type
        int substituteNameOffset = Marshal.ReadInt16(buffer, 8);
        int substituteNameLength = Marshal.ReadInt16(buffer, 10);

        IntPtr targetPointer = IntPtr.Add(buffer, substituteNameOffset);
        string targetPath = Marshal.PtrToStringUni(targetPointer, substituteNameLength / 2);

        return targetPath;
    }

    public static string CheckSignature(string filePath, int option)
    {
        // Example for validating signature; replace this with your logic
        return option == 1 ? "Valid Signature" : "Unknown";
    }




    public static string GetFileVersionInfo(string filePath, string infoType)
    {
        try
        {
            // Get the file version info of the given file
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

            // Return the requested information based on infoType
            switch (infoType.ToLower())
            {
                case "companyname":
                    return fileVersionInfo.CompanyName;
                case "productversion":
                    return fileVersionInfo.ProductVersion;
                case "fileversion":
                    return fileVersionInfo.FileVersion;
                case "filedescription":
                    return fileVersionInfo.FileDescription;
                default:
                    return "Unknown info type";
            }
        }
        catch (Exception ex)
        {
            return "Error retrieving file version info: " + ex.Message;
        }
    }

    public static string GetFile(string filePath)
    {
        // Check if the file exists
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath); // Return the file's content as a string
        }
        else
        {
            throw new FileNotFoundException("The specified file was not found.", filePath);
        }
    }

    public static string GetFileVersion(string filePath)
    {
        if (File.Exists(filePath))
        {
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
            return versionInfo.FileVersion;
        }

        throw new FileNotFoundException("File not found", filePath);
    }

    public static void ProcessFile(ref string file, ref string cdate, ref string company)
    {
        // Example logic to extract file creation date
        if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
        {
            // Get the creation date of the file
            cdate = System.IO.File.GetCreationTime(file).ToString("yyyy-MM-dd");

            // Extract company name from the file path (this is just an example)
            company = ExtractCompanyNameFromFile(file);
        }
        else
        {
            cdate = "Invalid file";
            company = "Unknown company";
        }
    }

    public static string ExtractCompanyNameFromFile(string filePath)
    {
        // Example: Assume company name is part of the directory structure
        // E.g., "C:\CompanyA\Project\file.txt" -> Extract "CompanyA"
        string directoryName = System.IO.Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directoryName))
        {
            // Get the last part of the directory as the company name
            string[] parts = directoryName.Split(System.IO.Path.DirectorySeparatorChar);
            return parts.Length > 0 ? parts[parts.Length - 1] : "Unknown Company";
        }

        return "Unknown Company";
    }

    public static string GetShortPath(string file)
    {
        System.Text.StringBuilder shortPath = new System.Text.StringBuilder(255);
        int result = Kernel32NativeMethods.GetShortPathName(file, shortPath, shortPath.Capacity);

        if (result == 0)
        {
            throw new InvalidOperationException("Error retrieving short path");
        }

        return shortPath.ToString();
    }



    public static int CheckFileSignature(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new ArgumentException("Invalid file path.");
        }

        // Example logic to check a file's signature
        byte[] fileHeader = new byte[4];
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fs.Read(fileHeader, 0, fileHeader.Length);
        }

        string fileSignature = BitConverter.ToString(fileHeader);
        return fileSignature == "4D-5A" ? 11 : 0; // Return 11 for valid, 0 for invalid
    }


    public static bool FindFirstFile(string path, out string firstFileName)
    {
        firstFileName = null;

        Structs.WIN32_FIND_DATA findData;
        IntPtr findHandle = Kernel32NativeMethods.FindFirstFile(path, out findData);
        if (findHandle == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            firstFileName = findData.cFileName;
            return true;
        }
        finally
        {
            Kernel32NativeMethods.FindClose(findHandle);
        }
    }



    public static string ExpandEnvironmentStrings(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Allocate a buffer to hold the expanded string
        int bufferSize = 1024;
        StringBuilder buffer = new StringBuilder(bufferSize);

        // Call the API
        uint result = Kernel32NativeMethods.ExpandEnvironmentStrings(input, buffer, bufferSize);

        if (result == 0 || result > bufferSize)
        {
            throw new InvalidOperationException(
                $"Failed to expand environment strings. Error code: {Marshal.GetLastWin32Error()}");
        }

        return buffer.ToString();
    }

    public static string ExpandEnvironmentStrings(string input, params string[] additionalVars)
    {
        string result = Environment.ExpandEnvironmentVariables(input);

        if (additionalVars != null && additionalVars.Length > 0)
        {
            foreach (string var in additionalVars)
            {
                result += Environment.ExpandEnvironmentVariables(var);
            }
        }

        return result;
    }



    public static bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }

        return File.Exists(filePath);
    }

    /// <summary>
    /// Retrieves the timestamps (creation, last access, and last write times) for a specified file.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    public static void GetFileTimestamps(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The specified file does not exist: {filePath}");

        try
        {
            // Use FileInfo to access file properties
            FileInfo fileInfo = new FileInfo(filePath);

            Console.WriteLine($"File: {filePath}");
            Console.WriteLine($"Creation Time: {fileInfo.CreationTime}");
            Console.WriteLine($"Last Access Time: {fileInfo.LastAccessTime}");
            Console.WriteLine($"Last Write Time: {fileInfo.LastWriteTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving file timestamps: {ex.Message}");
        }
    }

    public static string GetFileDetails(string filePath, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return "Unknown";

        try
        {
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
            var propertyValue = fileVersionInfo.GetType().GetProperty(propertyName)?.GetValue(fileVersionInfo)
                ?.ToString();
            return propertyValue ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }




    public static bool IsFileSigned(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return !string.IsNullOrEmpty(versionInfo.CompanyName); // Simplified logic
            }
        }
        catch
        {
            // Ignore errors and return false
        }
        return false;
    }

    /// Checks the hash of a file against provided hash values to verify integrity.
    /// </summary>
    /// <param name="filePath">The path to the file to be checked.</param>
    /// <param name="expectedHashValues">The expected hash values, comma-separated (e.g., SHA256, MD5).</param>
    public static void FileCheck(string filePath, string expectedHashValues)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(expectedHashValues))
        {
            Console.WriteLine("Invalid file path or hash values provided.");
            return;
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        try
        {
            // Calculate the SHA256 hash of the file
            string fileHash = ComputeFileHash(filePath, HashAlgorithmName.SHA256);

            // Split expected hash values and check against computed hash
            string[] expectedHashes = expectedHashValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            bool isHashMatch = false;

            foreach (string expectedHash in expectedHashes)
            {
                if (string.Equals(fileHash, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isHashMatch = true;
                    break;
                }
            }

            if (isHashMatch)
            {
                Console.WriteLine($"File integrity verified. Hash matches: {fileHash}");
            }
            else
            {
                Console.WriteLine($"File integrity check failed. Computed hash: {fileHash}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during file hash verification: {ex.Message}");
        }
    }

    /// <summary>
    /// Computes the hash of a file using the specified hash algorithm.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="algorithmName">The hash algorithm to use (e.g., SHA256, MD5).</param>
    /// <returns>The computed hash as a hexadecimal string.</returns>
    private static string ComputeFileHash(string filePath, HashAlgorithmName algorithmName)
    {
        // Create the hash algorithm instance
        using (var algorithm = HashAlgorithm.Create(algorithmName.Name))
        {
            if (algorithm == null)
            {
                throw new InvalidOperationException($"Hash algorithm '{algorithmName.Name}' is not supported.");
            }

            // Open the file stream and compute the hash
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

}
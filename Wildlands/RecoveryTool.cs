using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class RecoveryTool
{
    private string bootMode = "Normal";
    private string currentDirectory = @"C:\";
    private int osVersion = 10; // Replace with actual OS version detection logic

    private Form form1;
    private Button buttonScan, buttonFix, buttonSearch, buttonSearchReg;
    private CheckBox checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox8, checkBox9, checkBox11, checkBox12, checkBox13, checkBox14, checkBox15, checkBox0;
    private Label label1;
    private GroupBox groupWhitelist, groupScanOptions;
    private TextBox editBox;

    public void Run(string[] args)
    {
        InitializeGUI();

        if (bootMode != "Recovery" && (File.Exists(Path.Combine(currentDirectory, @"frst\files")) ||
                                       File.Exists(Path.Combine(currentDirectory, @"frst\filesRem")) ||
                                       File.Exists(Path.Combine(currentDirectory, @"frst\keysrem"))))
        {
            HandleRecoveryFiles();
        }

        if (!Directory.Exists(Path.Combine(currentDirectory, "FRST")) && args.Length <= 1)
        {
            var yn = MessageBox.Show("Disclaimer message here.", "Farbar Recovery Scan Tool", MessageBoxButtons.YesNo);
            if (yn == DialogResult.No)
            {
                return;
            }
        }

        HandleHivesDirectory();
        PerformStartupActions();

        Application.Run(form1);
    }

    private void InitializeGUI()
    {
        form1 = new Form
        {
            Text = $"Farbar Recovery Scan Tool (x86) SCAN0: {GetFileVersion(Application.ExecutablePath)}",
            Width = 600,
            Height = 400,
            StartPosition = FormStartPosition.CenterScreen
        };

        label1 = new Label
        {
            Text = "Ready",
            Location = new Point(50, 50)
        };
        form1.Controls.Add(label1);

        buttonScan = CreateButton("Scan", 55, 115, 100, 30, Color.FromArgb(0x7D, 0x4E, 0x00), ButtonScan_Click);
        buttonFix = CreateButton("Fix", 412, 115, 100, 30, Color.FromArgb(0x7D, 0x4E, 0x00), ButtonFix_Click);
        buttonSearch = CreateButton("Search Best", 163, 115, 115, 30, Color.FromArgb(0xBA, 0xC8, 0x8E), ButtonSearch_Click);
        buttonSearchReg = CreateButton("Search Reg", 286, 115, 118, 30, Color.FromArgb(0xBA, 0xC8, 0x8E), ButtonSearchReg_Click);

        editBox = new TextBox
        {
            Location = new Point(56, 57),
            Size = new Size(483, 50),
            Multiline = true
        };
        form1.Controls.Add(editBox);

        groupWhitelist = CreateGroupBox("Whitelist", 80, 150, 460, 70);
        checkBox1 = CreateCheckBox("Reg", 100, 170, 100, 20, groupWhitelist);
        checkBox2 = CreateCheckBox("Services", 205, 170, 120, 20, groupWhitelist);
        checkBox3 = CreateCheckBox("Drivers", 310, 170, 100, 20, groupWhitelist);

        if (bootMode == "Recovery")
        {
            checkBox4 = CreateCheckBox("KnownDLLs", 415, 170, 80, 20, groupWhitelist);
        }

        groupScanOptions = CreateGroupBox("Scan Options", 80, 225, 440, 70);
        checkBox9 = CreateCheckBox("BCD", 100, 245, 100, 20, groupScanOptions);
        checkBox14 = CreateCheckBox("90 Days", 100, 270, 150, 20, groupScanOptions);

        if (bootMode != "Recovery")
        {
            checkBox0 = CreateCheckBox("Tasks0", 415, 170, 100, 20, groupWhitelist);
            checkBox5 = CreateCheckBox("SigCheckExt", 205, 245, 100, 20, groupScanOptions);
            checkBox8 = CreateCheckBox("Process B", 100, 200, 100, 20, groupWhitelist);
            checkBox11 = CreateCheckBox("Internet", 205, 200, 90, 20, groupWhitelist);
            checkBox15 = CreateCheckBox("1 Month", 310, 200, 90, 20, groupWhitelist);
            checkBox13 = CreateCheckBox("Shortcut.txt", 310, 245, 90, 20, groupScanOptions);
            checkBox12 = CreateCheckBox("Addition.txt", 415, 245, 85, 20, groupScanOptions);
        }
    }

    private Button CreateButton(string text, int x, int y, int width, int height, Color backColor, EventHandler onClick)
    {
        Button button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = backColor
        };
        button.Click += onClick;
        form1.Controls.Add(button);
        return button;
    }

    private CheckBox CreateCheckBox(string text, int x, int y, int width, int height, Control parent)
    {
        CheckBox checkBox = new CheckBox
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Checked = true
        };
        parent.Controls.Add(checkBox);
        return checkBox;
    }

    private GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
    {
        GroupBox groupBox = new GroupBox
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height)
        };
        form1.Controls.Add(groupBox);
        return groupBox;
    }

    private string GetFileVersion(string filePath)
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
        }
        catch
        {
            return "Unknown Version";
        }
    }

    private void ButtonScan_Click(object sender, EventArgs e)
    {
        buttonScan.Text = "Scanning...";
        buttonScan.Enabled = false;

        PerformScanActions();

        buttonScan.Text = "Scan";
        buttonScan.Enabled = true;
    }

    private void ButtonFix_Click(object sender, EventArgs e)
    {
        buttonFix.Text = "Fixing...";
        buttonFix.Enabled = false;

        PerformFixActions();

        buttonFix.Text = "Fix";
        buttonFix.Enabled = true;
    }

    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        string searchQuery = editBox.Text.Trim();
        if (string.IsNullOrEmpty(searchQuery))
        {
            MessageBox.Show("Search query is empty.", "Farbar Recovery Scan Tool");
        }
        else
        {
            PerformSearchActions(searchQuery);
        }
    }

    private void ButtonSearchReg_Click(object sender, EventArgs e)
    {
        if (bootMode == "Recovery")
        {
            MessageBox.Show("Search Reg -> Output here.", "Farbar Recovery Scan Tool");
        }
        else
        {
            PerformSearchRegActions();
        }
    }

    private void HandleHivesDirectory()
    {
        string hivesDir = Path.Combine(currentDirectory, @"FRST\Hives");
        if (Directory.Exists(hivesDir))
        {
            DateTime lastModified = Directory.GetLastWriteTime(hivesDir);
            int daysDifference = (DateTime.Now - lastModified).Days;

            if (daysDifference > 60)
            {
                string oldHivesDir = Path.Combine(hivesDir, "Old");
                if (Directory.Exists(oldHivesDir))
                {
                    Directory.Delete(oldHivesDir, true);
                }
                Directory.Move(hivesDir, Path.Combine(currentDirectory, @"FRST\tmphives"));
            }
        }

        if (Directory.Exists(Path.Combine(currentDirectory, @"FRST\tmphives")))
        {
            Directory.Move(Path.Combine(currentDirectory, @"FRST\tmphives"), Path.Combine(hivesDir, "Old"));
        }
    }

    private void PerformStartupActions()
    {
        // Add startup logic here
    }

    private void PerformScanActions()
    {
        // Add scan logic here
    }

    private void PerformFixActions()
    {
        // Add fix logic here
    }

    private void PerformSearchActions(string query)
    {
        // Add search logic here
    }

    private void PerformSearchRegActions()
    {
        // Add registry search logic here
    }

    private void HandleRecoveryFiles()
    {
        // Add logic for handling recovery files
    }

    public static void LogSystemInfo(string workingDirectory, string arg)
    {
        string filePath = "";
        if (string.IsNullOrEmpty(filePath))
            return;

        filePath = CleanFilePath(filePath);

        if (File.Exists(filePath))
        {
            LogFileDetails(filePath);
            return;
        }

        // Handle specific replacements and validations for paths
        filePath = ReplaceSpecialPaths(filePath);

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        LogFileDetails(filePath);
    }

    private static string CleanFilePath(string path)
    {
        // Remove quotes and excess spaces
        path = Regex.Replace(path, "\"", string.Empty);
        path = Regex.Replace(path, "%+", "%");
        path = Regex.Replace(path, @"^\s+|\s+$", string.Empty);
        path = Regex.Replace(path, @"^\\\\\?\\", string.Empty);

        // Normalize directory separators
        path = path.Replace("/", "\\");

        return Regex.Replace(path, @"\\\\(?!\\)", "\\");
    }

    private static string ReplaceSpecialPaths(string path)
    {
        string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        if (path.IndexOf(@"\system32\msiexec", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            path = Path.Combine(windir, "System32", "msiexec.exe");
        }
        else if (Regex.IsMatch(path, @"(?i)(^\s*|%+)(systemroot|windir|Windows)(|%+)\\"))
        {
            path = Regex.Replace(path, @"(?i)(\s*|%+)(systemroot|windir|Windows)(|%+)\\", windir + "\\");
        }
        else if (Regex.IsMatch(path, @"(?i)%+ProgramFiles%+"))
        {
            path = Regex.Replace(path, @"(?i)%+ProgramFiles%+", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        }
        else if (Regex.IsMatch(path, @"(?i)%+ProgramData%+"))
        {
            path = Regex.Replace(path, @"(?i)%+ProgramData%+", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        }
        else if (Regex.IsMatch(path, @"(?i)\Asystem32"))
        {
            path = Regex.Replace(path, @"(?i)\Asystem32", Path.Combine(windir, "System32"));
        }

        return path;
    }

    private static void LogFileDetails(string filePath)
    {
        // Fetch file details like size, creation date, etc.
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"File: {filePath}");
        Console.WriteLine($"Size: {fileInfo.Length} bytes");
        Console.WriteLine($"Created: {fileInfo.CreationTime}");
    }

    public static void LogFarbarInfo(string scriptDir, string frstLogFilePath, string scanDetails)
    {
        string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        using (StreamWriter frstLog = new StreamWriter(frstLogFilePath, true))
        {
            string adminStatus = IsAdmin() ? "(Administrator)" : "(Standard User)";
            string osVersion = GetOSVersion();
            string defaultBrowser = GetDefaultBrowser();
            string language = GetSystemLanguage();

            frstLog.WriteLine($"Farbar Recovery Scan Tool (FRST)");
            frstLog.WriteLine($"User: {Environment.UserName} {adminStatus}");
            frstLog.WriteLine($"Computer: {Environment.MachineName}");
            frstLog.WriteLine($"OS: {osVersion}");
            frstLog.WriteLine($"Default Browser: {defaultBrowser}");
            frstLog.WriteLine($"Language: {language}");

            // Additional details
            frstLog.WriteLine($"Script Directory: {scriptDir}");
            frstLog.WriteLine($"Scan Details: {scanDetails}");
        }
    }

    private static bool IsAdmin()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string GetOSVersion()
    {
        return $"{Environment.OSVersion.VersionString} {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}";
    }


    private static string GetDefaultBrowser()
    {
        try
        {
            string regKey = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            string progId = Registry.GetValue(@"HKEY_CURRENT_USER\" + regKey, "Progid", null)?.ToString();
            if (!string.IsNullOrEmpty(progId))
            {
                string browserPath = Registry.GetValue(@"HKEY_CLASSES_ROOT\" + progId + @"\shell\open\command", "", null)?.ToString();
                return browserPath;
            }

            return "Unknown Browser";
        }
        catch
        {
            return "Unknown Browser";
        }
    }

    private static string GetSystemLanguage()
    {
        return System.Globalization.CultureInfo.CurrentCulture.DisplayName;
    }

    // Dictionary to store all localization strings
    private readonly Dictionary<string, string> localizationStrings;

    /*/
    public RecoveryToolLanguage()
    {
        localizationStrings = new Dictionary<string, string>
        {
            { "SCAN", "Scan" },
            { "FIXB", "Fix" },
            { "SEARCHB", "Search" },
            { "SEARCHBES", "Search Files" },
            { "SEARCHREG", "Search Registry" },
            { "WHITELB", "Whitelist" },
            { "REGB", "Registry" },
            { "SERVB", "Services" },
            { "DRIVB", "Drivers" },
            { "OBTSCAN", "Optional Scan" },
            { "PROC0", "process" },
            { "PROCB", "Processes" },
            { "BCD", "List BCD" },
            { "90DAYS", "90 Days Files" },
            { "DISCLAIM", "Disclaimer of warranty!\n\nThis software is provided \"AS IS\" without warranty of any kind. You may use this software at your own risk.\n\nUnless you have a licensed version, this software is not permitted for commercial purposes.\n\nAre you sure you want to continue?\n\nClick Yes to continue. Click No to exit." },
            { "READY", "The tool is ready to use." },
            { "SCANB", "Scanning" },
            { "BOOT1", "Is this the operating system you want to repair:" },
            { "BOOT2", "This operating system is on" },
            { "BOOT3", " drive when booted to the recovery mode." },
            { "BOOT4", "The tool is setting up itself to read the drives." },
            { "BOOT5", "The tool is run from the same drive the operating system is located. For this reason you will be presented to select the operating system once more. This is normal." },
            { "BOOT6", "Click OK to continue" },
            { "BOOT7", "The tool is setting up itself to read Local Disk." },
            { "UPD1", "ATTENTION" },
            { "UPD2", "This version of Farbar Recovery Scan Tool is " },
            { "UPD3", "days old and outdated." },
            { "UPD4", "Please download the latest version." },
            { "UPD5", "Do you want to continue?" },
            { "FIX1", "Fixing is in progress." },
            { "FIX2", "Fixing ..." },
            { "FIX3", "Result of scheduled files to move (Boot Mode: " },
            { "FIXREG", "Result of scheduled keys to remove after reboot:" },
            { "FIX4", "System is not rebooted." },
            { "FIX5", "is moved successfully" },
            { "MOVED", "moved successfully" },
            { "DELETED", "removed successfully." },
            { "FIX8", "Could not move" },
            { "FIX9", "Date&Time" },
            { "FIX10", "Fix completed." },
            { "UPD7", "Checking for update." },
            { "UPD8", "New update found." },
            { "UPD9", "Update Completed." },
            { "BKU1", "Backing up registry, this should take a few seconds..." },
            { "SCAN1", "Scanning is started." },
            { "SCAN2", "version is" },
            { "SCAN3", "days old and could be outdated" },
            { "SCAN4", "The user is not administrator" },
            { "SCAN5", "Default browser" },
            { "SCAN7", "not detected" },
            { "SCAN0", "Version" },
            { "SCAN8", "Temporary Profile" },
            { "SCAN9", "Loaded Profiles" },
            { "SCAN99", "Available Profiles" },
            { "SCAN10", "Scan result of" },
            { "SCAN11", "Ran by" },
            { "ON", "on" },
            { "SCAN13", "Running from" },
            { "SCAN14", "Language" },
            { "SCAN15", "Boot Mode" },
            { "SCAN16", "If the system is bootable FRST must be run from normal or Safe mode to create a complete log." },
            { "SCAN17", "Could not load system hive." },
            { "SCAN18", "System hive is missing." },
            { "SCAN19", "Tutorial for" },
            { "PROCESS1", "Failed to access process" },
            { "PROCESS3", "If an entry is included in the fixlist, the process will be closed. The file will not be moved." },
            { "REGIST1", "Scanning Registry" },
            { "REGIST2", "Software hive is missing." },
            { "REGIST3", "Software hive is not loaded." },
            { "REGIST4", "Value Name with invalid characters" },
            { "REGIST5", "Reading users keys" },
            { "REGIST7", "No CLSID Value" },
            { "REGIST8", "No File" },
            { "PAD", "path" },
            { "ALL", "All" },
            { "REGIST9", "If an entry is included in the fixlist, the registry item will be restored to default or removed." },
            { "REGIST10", "The file will not be moved." },
            { "WLISTED", "Whitelisted" },
            { "RESTRICT", "Restriction" },
            { "SOFTW", "software" },
            { "NFOUND", "not found" },
            { "RESTORED", "restored successfully" },
            { "NDELETED", "could not remove" },
            { "DETECTED", "detected" },
            { "INTERNET", "If an item is included in the fixlist, if it is a registry item it will be removed or restored to default." },
            { "INTERNET1", "is set" },
            { "INTERNET2", "is enabled." },
            { "INTERNET3", "Default URLSearchHook is missing" },
            { "INTERNET4", "value is missing" },
            { "INTERNET5", "should be" },
            { "INTERNET6", "There are more than one entry in Hosts. See Hosts section of" },
            { "INTERNET7", "Hosts file not detected in the default directory" },
            { "FF1", "No Name" },
            { "FF2", "Points to *.cfg file" },
            { "SERV1", "If an entry is included in the fixlist, it will be removed from the registry." },
            { "SERV2", "The file will not be moved unless listed separately" },
            { "UNLOCK", "was unlocked" },
            { "NUNLOCK", "could not be unlocked" },
            { "1MONT", "One month" },
            { "3MONT", "Three months" },
            { "FIL0", "File" },
            { "FIL1", "files" },
            { "FOL0", "Folder" },
            { "FOL1", "folders" },
            { "AND1", " and " },
            { "CREATED", "created" },
            { "MODIFIED", "modified" },
            { "FILFOL", "If an entry is included in the fixlist, the file/folder will be moved" },
            { "FILESR", "Files in the root of some directories" },
            { "0BYTE", "Some zero byte size" },
            { "BAM", "There is no automatic fix for files that do not pass verification" },
            { "MISS", "IS MISSING" },
            { "ASS", "Association" },
            { "RP1", "Restore Points" },
            { "RP2", "Could not list restore points" },
            { "RP3", "System Restore is disabled" },
            { "UACC", "Accounts" },
            { "CUST", "Custom" },
            { "DATAX", "the data entry has" },
            { "DATAY", "more characters" },
            { "ADD1", "Additional scan result of" },
            { "INSPRO", "Installed Programs" },
            { "CHECKWMI", "Check \"winmgmt\" service or repair WMI." },
            { "CHECKWMI1", "Check \"VSS\" service" },
            { "SECCENT", "Security Center" },
            { "SECCENT1", "If an entry is included in the fixlist, it will be removed." },
            { "CONTENT", "content" },
            { "HOSTS2", "If needed Hosts: directive could be included in the fixlist to reset Hosts." },
            { "HOSTS3", "HTML script in Hosts detected. See Hosts section of" },
            { "TASKS0", "Scheduled Tasks" },
            { "MOD1", "Loaded Modules" },
            { "ADS1", "If an entry is included in the fixlist, only the ADS will be removed." },
            { "SAFEB1", "is missing and should be manually restored." },
            { "SAFEB2", "Safe Mode" },
            { "REST1", "There are" },
            { "REST2", "more sites" },
            { "NOFIX", "Currently there is no automatic fix for this section." },
            { "DSN", "Media is not connected to internet." },
            { "ANDERE", "Other Areas" },
            { "DNS1", "Error getting" },
            { "MSCONF", "disabled items" },
            { "DEVICE1", "Faulty Device Manager Devices" },
            { "DEVICE2", "Could not list Devices." },
            { "EVENTS1", "Event log errors" },
            { "EVENTS2", "Could not start eventlog service, could not read events." },
            { "EVENTS3", "Application errors" },
            { "EVENTS4", "System errors" },
            { "SHORT1", "Shortcuts" },
            { "SHORT2", "The entries could be listed to be restored or removed" },
            { "SCANED", "Scan completed." },
            { "FIXRES", "Fix result of" },
            { "NOFIX1", "No fixlist.txt found." },
            { "NOFIX2", "The fixlist.txt should be in the same folder/directory the tool is located." },
            { "FIX12", "Looks you don't know what to do. To prevent damage to the system the tool will exit." },
            { "WARN", "Warning" },
            { "FIX13", "FRST is scripted not to move this directory." },
            { "FIXER1", "Error: The entry should be fixed outside recovery mode." },
            { "FIXER2", "Error: The restore operation should be done in the recovery mode." },
            { "FIXER3", "Error: This directive works only outside recovery mode." },
            { "FDIR", "File/Folder" },
            { "PROCL", "Processes closed successfully." },
            { "OF", "of" },
            { "OF2", "of" },
            { "END", "End" },
            { "FOUND1", "found" },
            { "TO", "to" },
            { "ZBYTE", "zero byte" },
            { "IS", "is" },
            { "NOT", "not" },
            { "MOVEREB", "Scheduled to move on reboot" },
            { "MOVE", "move" },
            { "DELRE", "Scheduled to remove on reboot" },
            { "DEFA", "Default" },
            { "NRESTORE", "Could not restore" },
            { "RESTORE", "from registry back up" },
            { "FROM", "from" },
            { "BACK", "back up" },
            { "VAL0", "value" },
            { "ERDEL", "Error deleting product" },
            { "USE", "Use" },
            { "REP1", "Deleting reparse point and unlocking" },
            { "DONE", "completed" },
            { "STAR", "started" },
            { "REP2", "reparsepoint" },
            { "CAT1", "The possible legit Catalog entry" },
            { "CAT2", "will not be deleted with FRST. Instead, \"netsh winsock reset\" can be used" },
            { "RENUM", "will be renumbered" },
            { "CHR1", "The Chrome \"Settings\" can be used to fix the entry" },
            { "KEYY", "key" },
            { "COP", "copied successfully to" },
            { "COP1", "copied successfully" },
            { "REP", "Could not replace" },
            { "STOPS", "Service stopped successfully" },
            { "NSTOPS", "Unable to stop service" },
            { "DISS", "service was disabled" },
            { "NDISS", "Unable to disable service" },
            { "DATA0", "Value data" },
            { "PRO4", "Can not verify if process existed" },
            { "PRO5", "process closed successfully" },
            { "PRO6", "Could not open process" },
            { "PRO7", "Could not close process" },
            { "PRO8", "No running process found" },
            { "ERR0", "Error" },
            { "SHORTERR", "Could not remove or repair shortcut argument. The shortcut could be damaged." },
            { "ARG0", "argument" },
            { "SHORT0", "Shortcut" },
            { "PW0", "Please wait..." },
            { "CREAMOD", "Creation and modification date" },
            { "SZ0", "Size" },
            { "ATT0", "Attributes" },
            { "NAME0", "Name" },
            { "COMP0", "Company Name" },
            { "INT0", "Internal" },
            { "OR0", "Original" },
            { "PROD0", "Product" },
            { "DES0", "Description" },
            { "COPR0", "Copyright" },
            { "SYMLINK0", "Symbolic link" },
            { "FILENS", "File not signed" },
            { "FILENS1", "not signed" },
            { "NPERMS", "Getting permissions failed" },
            { "NOACC", "Access Denied" },
            { "MKEYNR", "Main key is not recognized" },
            { "PROT0", "could be protected" },
            { "FIRSTA", "at first attempt" },
            { "NEXTL", "see next line" },
            { "CORRU", "is possibly corrupted" },
            { "INVALKEY", "subkey with invalid name" },
            { "NCOPY", "Could not copy" },
            { "PERMS", "permissions" },
            { "NOCRYPT", "Could not perform signature verification. Cryptographic Service is not running" },
            { "FILESIG", "File is digitally signed" },
            { "ERRSIG", "Error verifying file signature" },
            { "NOFIXENTRY", "No automatic fix found for this entry." },
            { "RESQUA", "Restoring from Quarantine completed." },
            { "MBR0", "is made successfully." },
            { "MBR1", "is not made." },
            { "DELTEMP0", "Deleting temporary files" },
            { "DELTEMP1", "temporary data Removed." },
            { "REBOOT0", "The computer will be restarted to complete the fix" },
            { "REBOOT1", "The system needed a reboot." },
            { "COMPLETED", "is saved in the same directory FRST is located." },
            { "REBOOT2", "The computer needs a restart. Please close all open windows. Note that you will not get any notification from the tool after restart." },
            { "REBOOT3", "Click OK to restart." },
            { "SEARCH0", "Search is in progress" },
            { "SEARCH1", "Searching" },
            { "SEARCH2", "No entry is entered in the search box." },
            { "SEARCH3", "in some cases this can take more than 10 minutes." },
            { "ADMINIS", "administrator" },
            { "SEARCH4", "Search result for" },
            { "TASKS1", "If an entry is included in the fixlist, the task (.job) file will be moved. The file which is running by the task will not be moved." },
            { "SEARCH5", "No search term is entered. Please enter the search term and press \"Search files\" button." },
            { "ADD0", "Only the adware programs with \"Hidden\" flag could be added to the fixlist to unhide them. The adware programs should be uninstalled manually." },
            { "NREMOV", "Will not be removed with FRST." },
            { "NRP", "Restore point can only be created in normal mode." },
            { "YRP", "Restore point was successfully created." },
            { "NHOSTS", "Hosts file not detected in the default directory" },
            { "ITEMPRO", "The item is protected. Make sure the software is uninstalled and its services is removed." },
            { "DRIVE0", "drive" },
            { "WBOOTC", "with boot components" },
            { "OBTFROM", "obtained from" },
            { "SYSTEM0", "system" },
            { "FWNRUN", "Firewall Service is not running." },
            { "FWDIS", "is disabled." },
            { "FILEMD", "Files to move or delete" },
            { "USCANR", "Users shortcut scan result" },
            { "FWRUL", "FirewallRules" },
            { "MINFO", "Memory info" },
            { "SERV", "service" },
            { "MD5L", "MD5 is legit" },
            { "ALTSH", "The \"AlternateShell\" value will be restored" },
            { "MOLI", "more lines" },
            { "FPAD", "filepath" },
            { "NO", "no" },
            { "CRRP", "Creating Restore Point. This can take a few minutes," },
            { "CRRPN", "Failed to create a restore point." },
            { "LOCLSRV", "Locked Service" },
            { "WINSOCKBR", "broken internet access due to missing entry." },
            { "DRS0", "Drives" },
            { "IETR", "trusted/restricted" },
            { "PARTT", "Partition Table" },
            { "OUTRE", "This functions outside Recovery Environment" },
            { "MOS", "More than one Windows operating system detected. They will be presented to select one to be scanned.\n\nIn case you made the wrong choice please restart and boot to recovery environment again before running the tool.\n\nClick OK to continue." },
            { "TESTS", "'testsigning' is set. Check for possible unsigned driver" },
            { "BCDSM", "The system is configured to boot to Safe Mode" },
            { "BCDNR", "Could not access BCD." },
            { "MEM1", "Percentage of memory in use" },
            { "MEM2", "Total physical RAM" },
            { "MEM3", "Available physical RAM" },
            { "MEM4", "Total Virtual" },
            { "MEM5", "Available Virtual" },
            { "FFPROX", "Firefox Proxy settings were reset." },
            { "ERRSV", "Error setting value." },
            { "WINFW", "Windows Firewall" },
            { "REN0", "renamed" },
            { "RESD0", "Restore point date" },
            { "PROCESSOR", "Processor" },
            { "REINS", "Reinstall Chrome." },
            { "INFEC", "Infected" },
            { "LEGACY", "Legacy" },
            { "REPAIR", "Repaired successfully" },
            { "NREPAIR", "Could not repair" },
            { "MOTH", "Motherboard" },
            { "INUSE", "File is in use" },
            { "ERRF", "Error Reading file" },
            { "DUMMY", "Dummy is met succes aangemaakt" },
            { "PLATF", "Platform" },
            { "FRSTVER86", "This version of FRST is not compatible with your operating system. Please use FRST64." },
            { "EXIT86", "Click OK to exit." },
            { "SCAN86A", "THE OPERATING SYSTEM IS A X64 SYSTEM BUT THE BOOT DISK THAT IS USED TO BOOT TO RECOVERY ENVIRONMENT IS A X86 SYSTEM DISK." },
            { "SCAN86B", "THE OPERATING SYSTEM IS A X64 SYSTEM BUT FRST IS A X86 VERSION." }
        };
    }
    /*/

    // Retrieve a string by its key
    public string GetLocalizedString(string key)
    {
        return localizationStrings.TryGetValue(key, out string value) ? value : $"[Missing: {key}]";
    }
}

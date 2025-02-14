using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Wildlands_System_Scanner.Backup;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scanning;
using Wildlands_System_Scanner.Scanning.AccountScan;
using Wildlands_System_Scanner.Scanning.FileScan;
using Wildlands_System_Scanner.Scanning.RegistryScan;
using Wildlands_System_Scanner.Scanning.RegistryScan.Browsers;
using Wildlands_System_Scanner.Scanning.ServicesScan;
using Wildlands_System_Scanner.Scripting;
using Wildlands_System_Scanner.Utilities;
using static Wildlands_System_Scanner.Backup.RegistryBackup;



namespace Wildlands_System_Scanner
{
    public partial class MainApp : DevExpress.XtraEditors.XtraForm
    {
        public string AppName = "Wildlands System Analyzer by Sneakyone";
        public string AppVersion = "Beta";
        public string AppHyperlink = "http://www.hackforums.net";
        public string AppOwner = "by Sneakyone";
        public string AppStatus = "0";

        public Thread AnalyzeThread;


        public MainApp()
        {
            InitializeComponent();
            AnalyzeThread = new Thread(StartScan);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Check if the application is running with administrative privileges
            if (!IsRunningAsAdministrator())
            {
                // If not running as administrator, restart the application with elevated privileges
                DialogResult result = MessageBox.Show(
                    "This application must be run as an administrator. Restart with administrative privileges?",
                    "Administrator Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    RestartAsAdministrator();
                }
                else
                {
                    // Close the application if the user declines
                    MessageBox.Show("The application cannot run without administrative privileges.", "Exiting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return; // Ensure no further code runs
                }
            }

            // Proceed with normal initialization
            progressBar1.Visible = false;
            simpleButtonStartFix.Visible = true;

            // Get the OS version
            var osVersion = Environment.OSVersion;
            var version = osVersion.Version;

            // Check if the OS is older than Windows XP
            // Windows XP: Version 5.1
            if (version.Major < 5 || (version.Major == 5 && version.Minor < 1))
            {
                MessageBox.Show("This application is not supported on operating systems older than Windows XP.");
                Environment.Exit(1); // Exit the application gracefully
            }
        }

        public void UpdateLabel(string text)
        {
            if (labelControlProgress.InvokeRequired)
            {
                // Ensure thread-safe updates
                labelControlProgress.Invoke((MethodInvoker)(() => labelControlProgress.Text = text));
            }
            else
            {
                // Update the label text directly
                labelControlProgress.Text = text;
            }
        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void simpleButtonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButtonStartScan_Click(object sender, EventArgs e)
        {
            simpleButtonExit.Visible = false;
            simpleButtonStartScan.Visible = false;
            progressBar1.Visible = true;
            simpleButtonStartFix.Visible = false;
            this.Cursor = Cursors.WaitCursor;
            AnalyzeThread.Start();
        }

        public void StartScan()
        {
            //Registry Backup
            this.AppStatus = "Progress: Backing up Registry";
            AccessMain();

            string backupDirectory = FolderConstants.DestinationPath;
            RegistryBackup.BackupEntireRegistry(backupDirectory);

            //Start Scanning

            this.AppStatus = "Progress: Header";
            AccessMain();

            AppName = "Wildlands System Analyzer";
            AppVersion = "Beta";
            AppOwner = "by Sneakyone";
            AppHyperlink = "http://www.hackforums.net";

            string getLoggedUsers = LoggedInUsersHandler.GetLoggedUsers();
            string strUserName = Environment.UserName;
            string strComputerName = Environment.MachineName;
            string isAdmin = UserUtils.IsUserAdministrator() ? "Administrator" : "Standard User";
            string dateTime = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToString("T");
            string applicationPath = AppDomain.CurrentDomain.BaseDirectory;
            string osPlatform = SystemUtils.GetOperatingSystem();
            string osArchitecture = RuntimeInformation.OSArchitecture.ToString(); // OS architecture (e.g., X64)
            string osLanguage = CultureInfo.CurrentCulture.DisplayName;
            string defaultWebBrowser = DefaultBrowserScan.GetDefaultWebBrowser();
            string bootMode = SystemUtils.GetBootMode();
            string totalPhysicalMemory = SystemUtils.GetTotalPhysicalMemory();
            string availablePhysicalMemory = SystemUtils.GetAvailablePhysicalMemory();

            Logger.Instance.LogPrimary($"{AppName} ({AppVersion}) ");
            Logger.Instance.LogPrimary($"Ran by: {strUserName} ({isAdmin}) on {strComputerName} at {dateTime}");
            Logger.Instance.LogPrimary($"Ran From: {applicationPath}");
            Logger.Instance.LogPrimary($"Profiles Logged in: {getLoggedUsers}");
            Logger.Instance.LogPrimary($"Platform: {osPlatform} {osArchitecture}" + " | " + $"Language: {osLanguage}");
            Logger.Instance.LogPrimary("Default Web Browser: " + defaultWebBrowser);
            Logger.Instance.LogPrimary("Boot Mode: " + bootMode);

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary($"Total Memory: {totalPhysicalMemory} | Available Memory: {availablePhysicalMemory}");
            SystemUtils.LogDrives();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Wildlands Scan Results & Deletions]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: Wildlands File Scan";
            AccessMain(); 

            Logger.Instance.LogPrimary("Wildlands File Scan:");
            Logger.Instance.LogPrimary("");
            AppDataFileBlacklist.ScanForBlacklistedFiles();
            DesktopFileBlacklist.ScanForBlacklistedFiles();
            LocalAppDataFileBlacklist.ScanForBlacklistedFiles();
            ProgramFilesFileBlacklist.ScanForBlacklistedFiles();
            ProgramFilesX86FileBlacklist.ScanForBlacklistedFiles();
            UserRootFileBlacklist.ScanForBlacklistedFiles();
            FavoritesBlacklist.ScanForBlacklistedFiles();

            this.AppStatus = "Progress: Wildlands Folder Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("Wildlands Folder Scan:");
            Logger.Instance.LogPrimary("");

            AppDataFolderBlacklist.ScanForBlacklistedItems();
            LocalAppDataFolderBlacklist.ScanForBlacklistedFolders();
            ProgramFilesFolderBlacklist.ScanForBlacklistedFolders();
            ProgramFilesX86FolderBlacklist.ScanForBlacklistedFolders();
            UserRootFolderBlacklist.ScanForBlacklistedFolders();
            
            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Processes]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: Processes";
            AccessMain();

            ProcessUtils.DisplayAllProcesses();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Wildlands Registry Scan]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: Registry Scan";
            AccessMain();

            
            ActiveSetupHandler.HandleActiveSetup(true);
            AeDebug.ScanAeDebug();
            AppCertHandler.HandleAppCert();
            AMSI.ScanAMSI();
            AppCompatFlags.ScanAppCompatFlags();
            AppContainer.ScanAppContainer();
            AppModel.ScanAppModelRegistry();
            AutoplayHandlers.ScanAutoPlayHandlers();
            AppInitHandler.HandleAppInitDlls();
            AuthenticationHandler.ScanAuthenticationRegistry();
            BCD.ScanBCDRegistry();
            BFE.ScanBFEPolicyKeys();
            BITS.ScanBITSKey();
            BHOHandler.ScanBrowserHelperObjects();
            BootVerification.ScanBootVerificationProgram();
            CertificateHandler.ScanDisallowedCertificatesRegistry();
            CodeStoreDatabase.ScanCodeStoreDatabase();
            CommandProcessorHandler.ScanCommandProcessor();
            Scanning.RegistryScan.Control.ScanSystemControlRegistry();
            Cryptography.ScanCryptographyRegistry();
            CTF.ScanCtfLangBarAddinRegistry();
            DNSCache.ScanDnscacheRegistry();
            Drivers32Handler.ScanDrivers32();

            // Get all user registry keys
            var userKeys = RegistryUserHandler.GetUserRegistryKeys();

            // Log the retrieved keys for debugging
            Console.WriteLine("Found the following user keys:");
            foreach (var userKey in userKeys)
            {
                Console.WriteLine(userKey);
            }

            Scanning.RegistryScan.EventLog.ScanEventLog();
            Hivelist.ScanHiveList();
            i8042prt.ScanI8042prt();
            Ialm.ScanIalm();
            IAS.ScanIAS();
            IniFileMapping.ScanIniFileMapping();
            Installer.ScanWindowsInstaller();
            KeyboardLayout.ScanKeyboardLayout();
            LsaHandler.ScanLsaRegistry();
            MountpointsHandler.ScanMountPoints2();
            MpsSvc.ScanMpsSvc();
            NetBt.ScanNetBT();
            NetSh.ScanNetSh();
            NetworkProvider.ScanNetworkProvider();
            Notepad.ScanNotepad();
            PolicyHandler.ScanRegistry();
            Print.ScanPrintRegistry();
            ProfilesListHandler.ScanProfileList();
            ProtocolsScan.ScanProtocols();
            SafebootHandler.ScanSafeBoot();
            Scripts.ScanGroupPolicyScripts();
            SessionManager.ScanSessionManager();
            ShellEx.ScanShellExtensions();
            ShellExecuteHooks.ScanShellExecuteHooks();
            ShellExtensions.ScanShellExtensions();
            ShellFolder.ScanShellFolders();
            ShellIconOverlayIdentifiers.ScanShellIconOverlay();
            ShellServiceObjectDelayLoad.ScanShellServiceObjectDelayLoad();
            ShellServiceObjects.ScanShellServiceObjects();
            SilentProcessExit.ScanSilentProcessExit();
            SrService.ScanSrService();
            Startup.ScanStartupRegistry();
            Svchost.ScanSvchost();
            SystemSelect.ScanSystemSelect();
            SystemSetup.ScanSetupCmdLine();
            Tasks.ScanTaskScheduler();
            Tcpip.ScanTcpIpParameters();
            ExplorerKeyHandler.HandleExplorerKeys(userKeys);
            Filter.ScanFilterRegistry();
            FilterHandler.LUFIL();
            FontDrivers.ScanFontDrivers();
           
            IFEOHandler.ScanImageFileExecutionOptions();
            InspectPackagesHandler.InspectPackages();

            NetworkAdapterHandler.NETBIND1();
            NetworkServiceHandler.NETSVC();
            PrinterMonitorHandler.PrintMonitors();
            PrinterProcessorHandler.PrintProc();
            
            SecurityProviderHandler.ScanSecurityProviders();
            SymlinkHandler.LogBootExecute();
            TerminalServerHandler.ScanTerminalServer();
            WindowsDefenderPolicyHandler.ScanWindowsDefenderPolicies();
            WinLogonHandler.WinLogonFunc();
            WinLogonHandler.ScanAllWinlogonKeys();

            FileAssociations.BATASS();
            FileAssociations.COMAssociation();
            FileAssociations.CheckExeAssociations();
            FileAssociations.RegAssMethod();
            FileAssociations.SCRASS();
            FileAssociations.CMDASS();

            PDFProcessorHandler.ProcessPDF();
            ProviderHandler.Provider();
            RunHandler.RunKey();
            RunHandler.RunKeyEx();
            RunHandler.HandleRunKeys();
            RunHandler.RunRunOnceKeys();

            SEHHandler.SEH();
            SHIMHandler.SHIM();
            SSOHandler.SSO();
            SSOHandler.SSODL();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[MSConfig/Task Manager Disabled Items]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: MSConfig/Task Manager Disabled Items Scan";
            AccessMain();

            MSConfigHandler.EnumerateAllDisabledEntries();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Wildlands Scan]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: Wildlands Scan";
            AccessMain();

            P2PSoftwareScan.ScanP2PSoftware();
            CDEmulatorSoftwareScan.ScanForCDEmulatorSoftware();

            this.AppStatus = "Progress: Browser Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Browsers]============================================================================");
            Logger.Instance.LogPrimary("");


            this.AppStatus = "Progress: Scheduled Tasks Scan";
            AccessMain(); 

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Scheduled Tasks]============================================================================");
            Logger.Instance.LogPrimary("");

            TaskSchedulerHandler.EnumerateScheduledTasks();

            this.AppStatus = "Progress: Services Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Services]============================================================================");
            Logger.Instance.LogPrimary("");

            ServiceHandler.ScanServices();

            this.AppStatus = "Progress: Drivers Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Drivers]============================================================================");
            Logger.Instance.LogPrimary("");

            DriverScanHandler.EnumerateDrivers();

            this.AppStatus = "Progress: Files Recently Created Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[One month (created)]============================================================================");
            Logger.Instance.LogPrimary("");

            CreatedLastFileScanner.EnumerateRecentFiles(Scanning.FolderScan.DirectoryEnum.directories, 30);

            this.AppStatus = "Progress: Files Recently Modified Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[One month (modified)]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: File Signature Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Signature Scan]============================================================================");
            Logger.Instance.LogPrimary("");

            FileSignatureScan.SignatureScan();

            this.AppStatus = "Progress: Code Integrity Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Code Integrity]============================================================================");
            Logger.Instance.LogPrimary("");

            CodeIntegrityHandler.Execute();

            this.AppStatus = "Progress: MBR Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[MBR & Partition Table]============================================================================");
            Logger.Instance.LogPrimary("");

            MBRHandler.AnalyzeAllDrives();

            this.AppStatus = "Progress: Account Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Account List]============================================================================");
            Logger.Instance.LogPrimary("");

            UserAccountScan.EnumerateUserAccounts();

            this.AppStatus = "Progress: Shortcuts/WMI Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Shortcuts/WMI]============================================================================");
            Logger.Instance.LogPrimary("");

            this.AppStatus = "Progress: Loaded Modules Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Loaded Modules]============================================================================");
            Logger.Instance.LogPrimary("");

            LoadedModulesHandler.EnumerateLoadedModules();

            this.AppStatus = "Progress: ADS Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Alternate Data Streams]============================================================================");
            Logger.Instance.LogPrimary("");

            string rootDirectory = ($"{FolderConstants.xHomeDrive}"); // Root directory to start the scan
            Console.WriteLine($"Scanning all directories under {rootDirectory} for alternate data streams...");
            AlternateDataStreamScan.ScanAllDirectories(rootDirectory);

            this.AppStatus = "Progress: HOSTS File Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Full Hosts]============================================================================");
            Logger.Instance.LogPrimary("");

            HOSTSHandler.EnumerateHostsFile();

            this.AppStatus = "Progress: System Information Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Detailed System Information]============================================================================");
            Logger.Instance.LogPrimary("");

            DetailedSystemScan.LogDetailedSystemInfo();

            this.AppStatus = "Progress: Installed Programs Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Installed Programs]============================================================================");
            Logger.Instance.LogPrimary("");

            InstalledProgramsScan.EnumerateInstalledPrograms();

            this.AppStatus = "Progress: Firewall Rules Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Firewall Rules]============================================================================");
            Logger.Instance.LogPrimary("");

            FirewallRulesScan.EnumerateFirewallRules();

            this.AppStatus = "Progress: System Restore Point Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[System Restore Points]============================================================================");
            Logger.Instance.LogPrimary("");

            SystemRestoreHandler.EnumerateSystemRestorePoints();

            this.AppStatus = "Progress: Device Manager Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Faulty Device Manager Devices]============================================================================");
            Logger.Instance.LogPrimary("");

            FaultyDevicesScan.EnumerateFaultyDevices();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Disabled Device Manager Items]============================================================================");
            Logger.Instance.LogPrimary("");

            DisabledDevicesScan.EnumerateDisabledDevices();

            this.AppStatus = "Progress: Event Log Scan";
            AccessMain();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Event Log]============================================================================");
            Logger.Instance.LogPrimary("");

            Logger.Instance.LogPrimary("System Event Log Entries:");
            Logger.Instance.LogPrimary("");
            EventLogScan.EnumerateEventLogEntries("System", 10);

            Logger.Instance.LogPrimary("\nApplication Event Log Entries:");
            Logger.Instance.LogPrimary("");
            EventLogScan.EnumerateEventLogEntries("Application", 10);

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[Windows Defender Log]============================================================================");
            Logger.Instance.LogPrimary("");

            WindowsDefenderEventScan.EnumerateDefenderDetections();

            Logger.Instance.LogPrimary("");
            Logger.Instance.LogPrimary("[End of Wildlands.txt]============================================================================");

            //Scanning Done!

            AppStatus = "Progress: Done!";
            AccessMain();
            Process.Start(@"C:\Wildlands\WildlandsLog.txt");
        }

        private void AccessMain()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AccessMain));
            }
            else
            {
                if (AppStatus == "1")
                {
                    Cursor = Cursors.Default;
                }

                if (AppStatus.StartsWith("Progress: "))
                {
                    labelControlProgress.Text = AppStatus;
                    progressBar1.Value += 1; // Assuming main_progress.Value is an integer. Adjust if necessary.
                    labelControlProgressB.Text = progressBar1.Value + "%";
                }

                if (AppStatus.StartsWith("Error: "))
                {
                    AnalyzeThread.Abort(); // Make sure AnalyzeThread is properly defined
                    MessageBox.Show(AppStatus);
                    Close(); // This closes the form
                }

                if (AppStatus.Contains("Done!"))
                {
                    Close(); // This closes the form
                }
            }
        }

        /// <summary>
        /// Checks if the current process is running as an administrator.
        /// </summary>
        /// <returns>True if running as administrator; otherwise, false.</returns>
        private bool IsRunningAsAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restarts the application with elevated privileges.
        /// </summary>
        private void RestartAsAdministrator()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas" // Request administrator privileges
                };

                Process.Start(startInfo);
                Application.Exit(); // Close the current instance after starting a new one
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart as administrator. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkEdit8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void simpleButtonStartFix_Click(object sender, EventArgs e)
        {
            ScriptProcessor.ProcessFixScript();
        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }
    }
}

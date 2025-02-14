using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Constants
{
    public class FolderConstants
    {
        public static string ProgramData;
        public static string Temp = "";
        public static string xHomeDrive = HomeDrive + "\\";

        public static string HomeDrive = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "HOMEDRIVE", null);
        public static string HomePath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "HOMEPATH", null);
        public static string AppData = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "APPDATA", null);
        public static string ProgramFiles = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion", "ProgramFilesDir", null);
        public static string ProgramFiles64 = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion", "ProgramFilesDir (x86)", null);
        public static string WinDir = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "systemroot", null);
        public static string System32 = WinDir + "\\system32";
        public static string LocalAppData = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "LOCALAPPDATA", null);
        public static string Username = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "USERNAME", null);
        public static string UserProfile = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "USERPROFILE", null);
        public static string TempDir = Path.GetTempPath();
        public static string SystemDrivers = System32 + "\\Drivers";

        public static readonly string Startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static readonly string StartupCommon = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

        public static readonly string WildlandsFolderPath = FolderConstants.xHomeDrive + "Wildlands";
        public static readonly string QuarantinePath = Path.Combine(WildlandsFolderPath, "Quarantine");
        public static readonly string LogsPath = Path.Combine(WildlandsFolderPath, "Logs");
        public static readonly string BinPath = Path.Combine(WildlandsFolderPath, "bin");
        public static readonly string DestinationPath = Path.Combine(WildlandsFolderPath, "Hives");
        public static readonly string WildlandsFilesPath = Path.Combine(WildlandsFolderPath, "Files");
        public static string RebootPath = Path.Combine(WildlandsFolderPath, "Reboot");

        public static string WildlandsLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wildlands.txt");
        public static string TemprpPath = Path.Combine(TempDir, "temprp");
        public static string Temprp2Path = Path.Combine(TempDir, "temprp2");

        public static void WindowsEnvironmentPaths()
        {
            if (RegistryConstants.RegistryOperatingSystem.Contains("Windows 8") || RegistryConstants.RegistryOperatingSystem.Contains("Windows 10") || RegistryConstants.RegistryOperatingSystem.Contains("Windows 11"))
            {
                ProgramData = HomeDrive + "\\programdata";
                UserProfile = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "USERPROFILE", null);
                Temp = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "LOCALAPPDATA", null) + "\\temp";
            }
            else if (RegistryConstants.RegistryOperatingSystem.Contains("Windows Vista") || RegistryConstants.RegistryOperatingSystem.Contains("Windows 7"))
            {
                ProgramData = HomeDrive + "\\programdata";
                UserProfile = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "USERPROFILE", null);
                Temp = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Volatile Environment", "LOCALAPPDATA", null) + "\\temp";
            }
            else if (RegistryConstants.RegistryOperatingSystem.Contains("Windows XP"))
            {
                UserProfile = HomeDrive + HomePath;
                Temp = UserProfile + "\\Local Settings\\Temp";
            }
        }
    }
}
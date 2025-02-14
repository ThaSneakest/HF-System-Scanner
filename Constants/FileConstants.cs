using System;
using System.IO;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Constants
{
    public class FileConstants
    {
        public static string ApplicationPath => AppDomain.CurrentDomain.BaseDirectory;

        public static string InternetExplorerPath => Path.Combine(FolderConstants.ProgramFiles, "Internet Explorer", "iexplore.exe");

        public static readonly string SearchFilePath = Path.Combine(ApplicationPath, "Search.txt");

        public static string ExplorerFile => $"{FolderConstants.WinDir}\\explorer.exe";
        public static string ExplorerFileSystem => GetSystemFilePath("explorer.exe");
        public static string WinlogonFile => GetSystemFilePath("winlogon.exe");
        public static string WininitFile => GetSystemFilePath("wininit.exe");
        public static string SvchostFile => GetSystemFilePath("svchost.exe");
        public static string ServicesFile => GetSystemFilePath("services.exe");
        public static string User32File => GetSystemFilePath("user32.dll");
        public static string UserinitFile => GetSystemFilePath("userinit.exe");
        public static string RpcssFile => GetSystemFilePath("rpcss.dll");
        public static string DnsapiFile => GetSystemFilePath("dnsapi.dll");
        public static string DllhostFile => GetSystemFilePath("dllhost.exe");
        public static string VolsnapFile => GetSystemFilePath("drivers\\volsnap.sys");

        /// <summary>
        /// Retrieves the full system file path, ensuring proper handling for 32-bit processes on a 64-bit system.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The full file path, or null if the file does not exist.</returns>
        private static string GetSystemFilePath(string fileName)
        {
            string systemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), fileName);

            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                // Use Sysnative to access true System32 from a 32-bit process on a 64-bit OS.
                string sysnativePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", fileName);

                if (File.Exists(sysnativePath))
                {
                    return sysnativePath;
                }
                else if (File.Exists(systemPath))
                {
                    return systemPath;
                }
                else
                {
                    Console.WriteLine($"File {fileName} does not exist in either System32 or Sysnative.");
                    return null;
                }
            }
            else
            {
                // Regular 64-bit process or 32-bit process on 32-bit OS.
                if (File.Exists(systemPath))
                {
                    return systemPath;
                }
                else
                {
                    Console.WriteLine($"File {fileName} does not exist in System32.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Disables WOW64 file system redirection for 32-bit processes.
        /// </summary>
        private static bool DisableWow64FsRedirection(out IntPtr oldRedirectionState)
        {
            return Kernel32NativeMethods.Wow64DisableWow64FsRedirection(out oldRedirectionState);
        }

        /// <summary>
        /// Reverts WOW64 file system redirection after it was disabled.
        /// </summary>
        private static bool RevertWow64FsRedirection(IntPtr oldRedirectionState)
        {
            return Kernel32NativeMethods.Wow64RevertWow64FsRedirection(oldRedirectionState);
        }
    }
}

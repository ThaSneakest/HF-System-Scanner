using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.Whitelist;

namespace Wildlands_System_Scanner
{
    public class LoadedModulesHandler
    {
        public static string[] ProcessGetLoadedModules(int processId)
        {
            IntPtr hProcess = Kernel32NativeMethods.OpenProcess(NativeMethodConstants.PROCESS_QUERY_INFORMATION | NativeMethodConstants.PROCESS_VM_READ, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                return new string[] { "Failed to open process." };
            }

            IntPtr[] modules = new IntPtr[200];
            uint bytesNeeded = 0;
            int result = PsapiNativeMethods.EnumProcessModules(hProcess, IntPtr.Zero, (uint)(modules.Length * IntPtr.Size), out bytesNeeded);

            if (result == 0)
            {
                Kernel32NativeMethods.CloseHandle(hProcess);
                return new string[] { "Failed to enumerate modules." };
            }

            uint moduleCount = bytesNeeded / (uint)IntPtr.Size;
            string[] moduleNames = new string[moduleCount];

            for (int i = 0; i < moduleCount; i++)
            {
                StringBuilder sb = new StringBuilder(1024);
                result = PsapiNativeMethods.GetModuleFileNameEx(hProcess, modules[i], sb, (uint)sb.Capacity);

                if (result == 0)
                {
                    Kernel32NativeMethods.CloseHandle(hProcess);
                    return new string[] { "Failed to get module file name." };
                }

                moduleNames[i] = sb.ToString();
            }

            Kernel32NativeMethods.CloseHandle(hProcess);
            return moduleNames;
        }

        public static void EnumerateLoadedModules()
        {
            StringBuilder log = new StringBuilder();

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    foreach (ProcessModule module in process.Modules)
                    {
                        try
                        {
                            string modulePath = module.FileName;
                            if (string.IsNullOrEmpty(modulePath))
                                continue;

                            string normalizedPath = Path.GetFullPath(modulePath).ToLowerInvariant(); // Normalize path for comparison

                            // Whitelist check (file path based)
                            if (LoadedModuleWhitelist.Whitelist.Any(whitelistedPath =>
                                string.Equals(Path.GetFullPath(whitelistedPath).ToLowerInvariant(), normalizedPath, StringComparison.OrdinalIgnoreCase)))
                            {
                                // Skip logging for whitelisted file paths
                                continue;
                            }

                            // Blacklist check
                            if (LoadedModuleBlacklist.Blacklist.Any(blacklistedPath =>
                                string.Equals(Path.GetFullPath(blacklistedPath).ToLowerInvariant(), normalizedPath, StringComparison.OrdinalIgnoreCase)))
                            {
                                Logger.Instance.LogPrimary($"{modulePath} <---- Malicious Loaded Module Found");
                                continue;
                            }

                            // File details
                            string fileVersion = FileVersionInfo.GetVersionInfo(modulePath)?.FileVersion ?? "Unknown";
                            string fileCompany = FileVersionInfo.GetVersionInfo(modulePath)?.CompanyName ?? "Unknown";
                            long fileSize = new FileInfo(modulePath).Length;
                            string signedStatus = FileUtils.IsFileSigned(modulePath) ? "Signed" : "File not signed";

                            // Format output
                            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {fileSize} ({fileCompany}) [{signedStatus}] {modulePath}";
                            log.AppendLine(logEntry);
                            Logger.Instance.LogPrimary(logEntry);
                        }
                        catch
                        {
                            // Skip inaccessible modules
                        }
                    }
                }
                catch
                {
                    // Skip processes that cannot be queried
                }
            }

            // Print log to console
            Console.WriteLine(log.ToString());
        }

    }
}

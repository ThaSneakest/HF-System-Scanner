using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

public static class SecurityUtils
{

    /// <summary>
    /// Sets default file access permissions for a given path.
    /// </summary>
    public static void SetDefaultFileAccess(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        string accountName = path.Contains(Environment.UserName)
            ? $"Administrators;System;Users;{Environment.UserName}"
            : "Administrators;System;Users;Authenticated Users";

        if (Environment.OSVersion.Version >= new Version(6, 0) &&
            (path.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) ||
             path.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))))
        {
            accountName += ";TrustedInstaller";
        }

        Unlock(path, accountName);
    }

    /// <summary>
    /// Sets the owner of a file or object at the specified path.
    /// </summary>
    public static void SetOwner(string path, string owner, uint seObjectType = 1)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(owner))
            return;

        IntPtr sid = GetSidStructure(owner);
        if (sid == IntPtr.Zero)
        {
            Console.WriteLine("Invalid SID structure.");
            return;
        }

        if (!SetPrivilege("SeTakeOwnershipPrivilege"))
        {
            Console.WriteLine("Failed to enable necessary privileges.");
            return;
        }

        int result = Advapi32NativeMethods.SetNamedSecurityInfo(path, seObjectType, NativeMethodConstants.OWNER_SECURITY_INFORMATION, sid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (result != 0)
        {
            Console.WriteLine($"Failed to set owner. Error code: {result}");
        }
    }

    /// <summary>
    /// Enables or disables a specified privilege for the current process.
    /// </summary>
    public static bool SetPrivilege(string privilege, bool enable = true)
    {
        if (string.IsNullOrWhiteSpace(privilege))
            return false;

        if (!Advapi32NativeMethods.OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle, 0x0020 | 0x0008, out IntPtr tokenHandle))
            return false;

        try
        {
            Structs.LUID luid = new Structs.LUID();
            if (!Advapi32NativeMethods.LookupPrivilegeValue(null, privilege, ref luid))
                return false;

            Structs.TOKEN_PRIVILEGES tp = new Structs.TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = enable ? NativeMethodConstants.SE_PRIVILEGE_ENABLED : NativeMethodConstants.SE_PRIVILEGE_DISABLED
            };

            if (!Advapi32NativeMethods.AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                return false;

            return true;
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(tokenHandle);
        }
    }

    /// <summary>
    /// Unlocks a file or object, setting default access permissions.
    /// </summary>
    public static int Unlock(string path, string accounts = "Administrators;System;Users", int acc = 2)
    {
        string owner = accounts.Contains("Everyone") ? "Everyone" : "Administrators";
        if (acc == 3)
            owner = "Administrators";

        IntPtr sid = GetSidStructure(owner);
        if (sid == IntPtr.Zero)
        {
            return SetError(1, 0, "Failed to retrieve SID.");
        }

        IntPtr dacl = _CreateAcl(accounts, 1, acc);
        if (dacl == IntPtr.Zero)
        {
            return SetError(2, 0, "Failed to create ACL.");
        }

        int result = Advapi32NativeMethods.SetNamedSecurityInfo(path, 1, NativeMethodConstants.DACL_SECURITY_INFORMATION | NativeMethodConstants.OWNER_SECURITY_INFORMATION, sid, IntPtr.Zero, dacl, IntPtr.Zero);
        if (result != 0)
        {
            return SetError(3, (int)result, "Failed to set security info.");
        }

        return 0; // Success
    }

    private static IntPtr GetSidStructure(string owner)
    {
        // Replace with actual implementation to convert account name to SID
        Console.WriteLine($"Retrieving SID for owner: {owner}");
        return IntPtr.Zero;
    }

    private static IntPtr _CreateAcl(string accounts, int seObjectType, int acc)
    {
        // Replace with logic to create an ACL based on provided parameters
        return IntPtr.Zero;
    }

    private static int SetError(int code, int extended, string message)
    {
        // Implement custom error handling
        Console.WriteLine($"Error {code}: {message} (Extended: {extended})");
        return code;
    }

    public static void ScanInstalledPrograms()
    {
        List<string> programs = new List<string>();
        string uninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        ScanRegistry(Registry.LocalMachine.OpenSubKey(uninstallKey), programs);

        foreach (string userHive in Registry.Users.GetSubKeyNames())
        {
            string userUninstallKey = $@"{userHive}\{uninstallKey}";
            ScanRegistry(Registry.Users.OpenSubKey(userUninstallKey), programs);
        }

        WriteToFile("Programs.txt", programs.Distinct().OrderBy(p => p).ToList());
    }

    private static void ScanRegistry(RegistryKey baseKey, List<string> programs)
    {
        if (baseKey == null) return;

        foreach (string subKeyName in baseKey.GetSubKeyNames())
        {
            using (RegistryKey subKey = baseKey.OpenSubKey(subKeyName))
            {
                if (subKey == null) continue;

                string displayName = subKey.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(displayName) && !IsIgnoredProgram(displayName))
                {
                    programs.Add(displayName);
                }
            }
        }
    }

    private static bool IsIgnoredProgram(string programName)
    {
        string[] ignoredPatterns = { @"^(Security Update for|Hotfix for|Update for Microsoft)" };
        return ignoredPatterns.Any(pattern => Regex.IsMatch(programName, pattern, RegexOptions.IgnoreCase));
    }

    private static void WriteToFile(string fileName, List<string> entries)
    {
        File.WriteAllLines(fileName, entries);
    }
}

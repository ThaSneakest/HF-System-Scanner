using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

public class ActiveSetupHandler
{
    private static string UPD1 = "Attention";

    public static void HandleActiveSetup()
    {
        string key = @"HKLM\Software\Microsoft\Active Setup\Installed Components";

        // Assuming OpenRegistryKey returns an IntPtr
        RegistryKey registryKey = RegistryKeyHandler.OpenRegistryKey(key); // Ensure the method returns a RegistryKey
        IntPtr registryHandle = registryKey.Handle.DangerousGetHandle(); // Get the IntPtr


        // Convert IntPtr to RegistryKey
        using (RegistryKey hkey = RegistryKey.FromHandle(new Microsoft.Win32.SafeHandles.SafeRegistryHandle(registryHandle, true)))
        {
            if (hkey == null) return;

            int index = 0;
            while (true)
            {
                string subKeyName = RegistrySubKeyHandler.EnumerateSubKey(hkey, index++);
                if (subKeyName == null)
                    break;

                string data = RegistryValueHandler.TryReadRegistryValue(hkey, subKeyName, "StubPath");
                if (string.IsNullOrEmpty(data))
                    continue;

                string shellComponent = RegistryValueHandler.TryReadRegistryValue(hkey, subKeyName, "ShellComponent");
                if (!string.IsNullOrEmpty(shellComponent))
                    data = shellComponent;

                string cdate = string.Empty;
                string company = string.Empty;
                string file = data;

                // Placeholder for file processing
                FileUtils.ProcessFile(ref file, ref cdate, ref company);

                if (IsCheckboxChecked(1)) // Simulating checkbox state
                {
                    if (company.Contains("Microsoft Corp"))
                    {
                        if (Regex.IsMatch(file, @"(?i):\\WINDOWS\\System32\\(unregmp2|ie4uinit)\.exe") ||
                            Regex.IsMatch(file, @"(?i):\\WINDOWS\\System32\\(themeui|shell32|mscories|IEDKCS32)\.dll") ||
                            Regex.IsMatch(file, @"(?i):\\Program Files\\Windows Mail\\WinMail.exe") ||
                            Regex.IsMatch(file, @"(?i):\\Program Files\\Microsoft\\Edge\\Application\\(\d+\.)+\d+\\Installer\\setup.exe"))
                        {
                            continue;
                        }
                    }

                    if (Regex.IsMatch(file, @"(?i)%SystemRoot%\\system32\\regsvr32.exe /s /n /i:/UserInstall %SystemRoot%\\system32\\themeui.dll$") ||
                        Regex.IsMatch(file, @"(?i)regsvr32.exe /s /n /i:U (%SystemRoot%\\System32\\|)shell32\.dll$"))
                    {
                        continue;
                    }
                }

                string attn = string.Empty;
                if (Regex.IsMatch(file, @"(?i)InstallDir\\Server.exe|(:\\Windows|%SystemRoot%)\\servicing\\(?!TrustedInstaller)[^\\]+\.exe"))
                {
                    attn = " <==== " + UPD1;
                }

                Logger.AddToLog($"HKLM\\Software\\Microsoft\\Active Setup\\Installed Components: [{subKeyName}] -> {data} {cdate} {company} {attn}");
            }
        }
    }

    // Simulate checkbox state check
    private static bool IsCheckboxChecked(int checkboxId)
    {
        // Simulate checkbox state (1 means checked, 0 means unchecked)
        return true;
    }
}

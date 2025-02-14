using System;
using System.Management;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;
using DevExpress.XtraEditors.TextEditController.Win32;
using System.Text;
using Wildlands_System_Scanner.NativeMethods;

//Tested and working

public class NetworkAdapterHandler
{
    public static void NETBIND0(ref List<string> arrBin1)
    {
        try
        {
            // Set up the management scope to interact with WMI
            ManagementScope scope = new ManagementScope(@"\\.\root\StandardCimv2");
            scope.Connect();

            // Create a WMI query to select network adapters
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSFT_NetAdapter");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection queryCollection = searcher.Get();

            // Iterate through the result collection and extract data
            foreach (ManagementObject obj in queryCollection)
            {
                string name = obj["Name"]?.ToString() ?? "Unknown";
                string description = obj["InterfaceDescription"]?.ToString() ?? "Unknown";
                string driverName = obj["DriverName"]?.ToString() ?? "Unknown";

                // Extract the driver name after the last backslash
                string driverFileName = driverName.Substring(driverName.LastIndexOf('\\') + 1);

                // Add the formatted information to the list
                arrBin1.Add($"{name}: {description} -> {driverFileName} ||||");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NETBIND0: {ex.Message}");
        }
    }
  public static void NETBIND1()
    {
        try
        {
            string keyPath = @"SYSTEM\CurrentControlSet\Control\Network";
            Console.WriteLine($"Opening registry key: HKLM\\{keyPath}");

            // Explicitly use the 64-bit registry view
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (RegistryKey hKey = baseKey.OpenSubKey(keyPath))
            {
                if (hKey == null)
                {
                    Console.WriteLine($"Registry key not found: HKLM\\{keyPath}");
                    return;
                }

                Console.WriteLine($"Successfully accessed registry key: HKLM\\{keyPath}");

                // Enumerate subkeys under "Control\Network"
                foreach (var subKeyName in hKey.GetSubKeyNames())
                {
                    Console.WriteLine($"Subkey: {subKeyName}");

                    using (RegistryKey subKey = hKey.OpenSubKey(subKeyName))
                    {
                        if (subKey == null)
                        {
                            Console.WriteLine($"Unable to access subkey: {subKeyName}");
                            continue;
                        }

                        // Further nested enumeration
                        foreach (var nestedSubKeyName in subKey.GetSubKeyNames())
                        {
                            Console.WriteLine($"Nested Subkey: {nestedSubKeyName}");

                            using (RegistryKey nestedSubKey = subKey.OpenSubKey(nestedSubKeyName))
                            {
                                if (nestedSubKey == null)
                                {
                                    Console.WriteLine($"Unable to access nested subkey: {nestedSubKeyName}");
                                    continue;
                                }

                                // Check for "ComponentId" value
                                string componentId = nestedSubKey.GetValue("ComponentId")?.ToString();
                                if (!string.IsNullOrEmpty(componentId))
                                {
                                    Console.WriteLine($"ComponentId found: {componentId}");

                                    // Exclude specific ComponentId values using regex
                                    if (System.Text.RegularExpressions.Regex.IsMatch(componentId, @"(?i)^(ms_winvfp|netvsc_vfpp|ms_(ndiscap|netbios|vwifi|wfplwf_upper|nativewifip|wfplwf_vswitch|rdma_ndk|netbt|ndiswanlegacy|wanarp|tcpip_tunnel|tcpip6_tunnel|pppoe|wanarpv6|netbt_smb|ndisuio|ndiswan|xboxgip|wfplwf_lower|l2bridge|rmcast|server|implat|lltdio|tcpip6|tcpip|msclient|lldp|rspndr|pacer|bridge)|vms_pp)$"))
                                    {
                                        Console.WriteLine($"Skipping excluded ComponentId: {componentId}");
                                        continue;
                                    }

                                    // Log the ComponentId and related information
                                    string description = nestedSubKey.GetValue("Description")?.ToString() ?? "No description";
                                    Logger.Instance.LogPrimary($"ComponentId: {componentId}, Description: {description}");
                                }
                                else
                                {
                                    Console.WriteLine($"No ComponentId found in nested subkey: {nestedSubKeyName}");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in NETBIND1: {ex.Message}");
        }
    }


    public static string NETBIND2(string fpath)
    {
        string sText = fpath;

        // If the path doesn't contain '@' or contains ';', return the part after ';'
        if (!Regex.IsMatch(fpath, "@") || Regex.IsMatch(fpath, ";"))
        {
            return Regex.Replace(fpath, @".+;", "");
        }

        // Extract file part before '@'
        string file = Regex.Replace(fpath, @"@(.+),.+", "$1");

        // Get system path for Windows
        string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows).Replace("\\", "\\\\");

        // Replace system-related environment variables with actual system paths
        if (Regex.IsMatch(file, @"(?i)(^\s*|%+)(systemroot|windir|Windows)(|%+)\\"))
        {
            file = Regex.Replace(file, @"(?i)(\s*|%+)(systemroot|windir|Windows)(|%+)\\", windir + "\\");
        }
        else if (Regex.IsMatch(file, @"(?i)%+Programfiles%+"))
        {
            string programFDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace("\\", "\\\\");
            file = Regex.Replace(file, @"(?i).*%+Programfiles%+", programFDir);
        }
        else if (Regex.IsMatch(file, @"(?i)%+ProgramFiles\(x86\)%+"))
        {
            file = Regex.Replace(file, @"(?i)%+ProgramFiles\(x86\)%+", @"C:\Program Files (x86)");
        }
        else if (Regex.IsMatch(file, @"(?i)%+ProgramData%+"))
        {
            file = Regex.Replace(file, @"(?i)%+ProgramData%+", @"C:\ProgramData");
        }
        else if (Regex.IsMatch(file, @"(?i)\Asystem32"))
        {
            file = Regex.Replace(file, @"(?i)\Asystem32", windir + "\\System32");
        }

        // Extract resource ID from fpath
        string resourceIdString = Regex.Replace(fpath, @".+,\s*-(\d+)", "$1");

        // Parse resource ID to uint
        if (!uint.TryParse(resourceIdString, out uint resourceId))
        {
            return fpath; // If parsing fails, return the original file path
        }

        // Load the library with the required flag
        const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;
        uint dwFlags = LOAD_WITH_ALTERED_SEARCH_PATH;

        IntPtr hInstance = Kernel32NativeMethods.LoadLibraryEx(file, IntPtr.Zero, dwFlags);
        if (hInstance != IntPtr.Zero)
        {
            // Create a StringBuilder to hold the loaded string
            StringBuilder loadedText = new StringBuilder(1024); // Set buffer size accordingly

            // Attempt to load the string from the resource
            int result = User32NativeMethods.LoadString(hInstance, resourceId, loadedText, loadedText.Capacity);

            // Return the loaded string if successful
            if (result > 0)
            {
                return loadedText.ToString();
            }
        }

        // If the string could not be loaded, return the original file path
        return fpath;
    }

}

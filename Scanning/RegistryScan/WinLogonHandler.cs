using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

//Tested and working

public class WinLogonHandler
{

    public static void WinLogonFunc()
    {
        try
        {
            // Define the registry path
            string registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
            string valueName = "Taskman";
            string attn = string.Empty;

            // Attention message for specific value
            if (valueName.Equals("Taskman", StringComparison.OrdinalIgnoreCase))
            {
                attn = " <=== Attention";
            }

            // Read the registry value
            object valUserObj = Microsoft.Win32.Registry.GetValue(registryKey, valueName, null);
            if (valUserObj == null || !(valUserObj is string valUser) || string.IsNullOrEmpty(valUser))
            {
                Console.WriteLine($"Value '{valueName}' not found in registry key '{registryKey}'.");
                return;
            }

            // Process the file path
            string file = valUser;
            FileUtils.AAAAFP(file); // Implement this method as needed

            if (File.Exists(file))
            {
                // Get file information
                var fileInfo = new FileInfo(file);
                string size = fileInfo.Length.ToString();
                string creationDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                string company = FileUtils.GetFileCompany(file); // Replace with an actual method to fetch company info

                Logger.Instance.LogPrimary(
                    $"HKLM\\...\\Winlogon: [{valueName}] {file} [{size} {creationDate}] {company} {attn}");
            }
            else
            {
                // If the file does not exist
                valUser += $" {StringConstants.REGIST8}";
                Logger.Instance.LogPrimary($"HKLM\\...\\Winlogon: [{valueName}] {valUser} {attn}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in WinLogonFunc: {ex.Message}");
        }
    }



    public static void ScanAllWinlogonKeys()
    {
        try
        {
            using (RegistryKey hkuKey = Registry.Users)
            {
                foreach (var userKey in hkuKey.GetSubKeyNames())
                {
                    Console.WriteLine($"Processing user key: {userKey}");

                    string winlogonPath = $@"{userKey}\Software\Microsoft\Windows NT\CurrentVersion\Winlogon";

                    using (RegistryKey winlogonKey = hkuKey.OpenSubKey(winlogonPath))
                    {
                        if (winlogonKey == null)
                        {
                            Console.WriteLine($"Key not found: HKU\\{winlogonPath}");
                            continue;
                        }

                        foreach (var valueName in winlogonKey.GetValueNames())
                        {
                            string valueData = winlogonKey.GetValue(valueName)?.ToString();
                            Logger.Instance.LogPrimary($"Winlogon Key: [{valueName}] => {valueData}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while scanning Winlogon keys: {ex.Message}");
        }
    }


 
    public string GetWinlogonNotifiers()
    {
        string Notifiers = string.Empty;

        try
        {
            // Open the registry key
            using (RegistryKey CSK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify"))
            {
                if (CSK != null)
                {
                    // Iterate through each subkey
                    foreach (string S in CSK.GetSubKeyNames())
                    {
                        try
                        {
                            // Retrieve the value for "DllName"
                            string A = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\" + S, "DllName", null));

                            // If value is not null, append to Notifiers string
                            if (!string.IsNullOrEmpty(A))
                            {
                                Notifiers += "Notifier: " + S + " - " + A + Environment.NewLine;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exception if necessary
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exception if necessary
            Console.WriteLine(ex.Message);
        }

        return Notifiers;
    }

}

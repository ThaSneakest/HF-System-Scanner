using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

//Tested and Working
public class ProviderHandler
{

    public static void Provider()
    {
        string keyPath = @"SYSTEM\CurrentControlSet\Control\Print\Providers";

        try
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (registryKey == null)
                {
                    Console.WriteLine($"Registry key not found: {keyPath}");
                    return;
                }

                foreach (string subKeyName in registryKey.GetSubKeyNames())
                {
                    string nameValue = registryKey.OpenSubKey(subKeyName)?.GetValue("Name")?.ToString();

                    if (string.IsNullOrEmpty(nameValue))
                    {
                        Console.WriteLine($"Subkey '{subKeyName}' has no 'Name' value.");
                        continue;
                    }

                    string att = string.Empty;

                    if (Regex.IsMatch(nameValue, @"(?i)local\d{2}spl\.dll"))
                    {
                        att = " <==== Attention";
                    }

                    string file = nameValue;

                    if (File.Exists(file))
                    {
                        var fileInfo = new FileInfo(file);
                        string size = fileInfo.Length.ToString();
                        string cDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                        string company = FileUtils.GetFileCompany(file); // Replace with your method for fetching company info

                        Logger.Instance.LogPrimary(
                            $"HKLM\\...\\Providers\\{subKeyName}: {file} [{size} {cDate}] {company} {att}"
                        );
                    }
                    else
                    {
                        Logger.Instance.LogPrimary($"HKLM\\...\\Providers\\{subKeyName}: {nameValue} {att}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Provider: {ex.Message}");
        }
    }

  
}
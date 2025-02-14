using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

//Tested and Working | Value draws from Wow64 instead of normal path
public class AppInitHandler
{
    public static void HandleAppInitDlls()
    {
        // Define the registry key path
        string registryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows";

        // Open the registry key for reading
        using (var key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
        {
            if (key == null)
            {
                Console.WriteLine($"Registry key not found: {registryKeyPath}");
                return;
            }

            // Read the registry value
            string valData = RegistryValueHandler.ReadRegistryValue(registryKeyPath, "AppInit_DLLs");
            if (!string.IsNullOrEmpty(valData))
            {
                Console.WriteLine($"Raw Value: {valData}");

                // Normalize whitespace and trim leading/trailing spaces
                valData = Regex.Replace(valData, @"\s{2,}", " ").Trim();

                // Process each DLL entry
                if (!string.IsNullOrEmpty(valData))
                {
                    var array = valData.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var item in array)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            string data = item.TrimStart(' ', ',');
                            string atten = string.Empty;

                            // Check for certain conditions (example condition shown here)
                            if (Regex.IsMatch(data, @"(?i)\\System\\symsrv.dll"))
                            {
                                atten = " <==== Attention";
                            }

                            // Check if the file exists
                            if (File.Exists(data))
                            {
                                FileInfo fileInfo = new FileInfo(data);
                                string fileDetails = $"{data} [{fileInfo.Length} {fileInfo.CreationTime}] {atten}";

                                // Log or display the file details
                                Logger.Instance.LogPrimary($"AppInit_DLLs: {fileDetails}");
                            }
                            else
                            {
                                Console.WriteLine($"File not found: {data}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("AppInit_DLLs value is empty.");
            }
        }
    }

    public string GetAppInitDLLs()
    {
        string AppInit_DLLs = string.Empty;

        try
        {
            // Open the registry key
            using (RegistryKey CSK = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows", true))
            {
                if (CSK != null)
                {
                    try
                    {
                        // Retrieve the value for "AppInit_DLLs"
                        object A = CSK.GetValue("AppInit_DLLs");

                        // If value is not null, append to AppInit_DLLs string
                        if (A != null)
                        {
                            AppInit_DLLs += "AppInit_DLLs: " + A.ToString() + Environment.NewLine;
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
        catch (Exception ex)
        {
            // Handle exception if necessary
            Console.WriteLine(ex.Message);
        }

        return AppInit_DLLs;
    }
}

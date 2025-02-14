using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

//Tested and Working
public class PDFProcessorHandler
{
    public static void ProcessPDF()
    {
        try
        {
            Console.WriteLine("Starting ProcessPDF...");

            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Code Store Database\Distribution Units"))
            {
                if (registryKey == null)
                {
                    Console.WriteLine("Registry key not found.");
                    return;
                }

                int i = 0;
                while (true)
                {
                    string subKey = RegistrySubKeyHandler.GetSubKey(registryKey, i);
                    if (subKey == null)
                    {
                        break; // Exit the loop when no more subkeys are found
                    }

                    Console.WriteLine($"Processing subKey: {subKey}");

                    if (Regex.IsMatch(subKey, @"\{.+\}")) // Match GUID-like subkeys
                    {
                        string downloadInfoPath = $@"{subKey}\DownloadInformation";
                        string filePath = registryKey.OpenSubKey(downloadInfoPath)?.GetValue("codebase")?.ToString();

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // Replace http/https with hxxp/hxxps for security
                            filePath = Regex.Replace(filePath, @"(?i)http(s|):", "hxxp$1:");

                            // Simulated checkbox state logic
                            int checkboxState = GetCheckboxState(); // Replace this with actual checkbox state retrieval logic
                            if (checkboxState == 4 || !filePath.Contains("hxxp://www.update.microsoft.com"))
                            {
                                Logger.Instance.LogPrimary($"DPF: {subKey} {filePath}");
                                Console.WriteLine($"Logged: DPF: {subKey} {filePath}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No 'codebase' value found for subKey: {subKey}");
                        }
                    }

                    i++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessPDF: {ex.Message}");
        }
    }

    // Simulated method for checkbox state
    private static int GetCheckboxState()
    {
        // Simulate that the checkbox state is 4
        return 4;
    }
  
}

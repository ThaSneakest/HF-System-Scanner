using System;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using Wildlands_System_Scanner.Registry;
//Tested and working
public class PrinterProcessorHandler
{
    public static void PrintProc()
    {
        try
        {
            string baseKeyPath = @"SYSTEM\CurrentControlSet\Control\Print\Environments";
            Console.WriteLine($"Opening registry key: HKLM\\{baseKeyPath}");

            using (RegistryKey hkey = Registry.LocalMachine.OpenSubKey(baseKeyPath))
            {
                if (hkey == null)
                {
                    Console.WriteLine($"Registry key not found: HKLM\\{baseKeyPath}");
                    return;
                }

                // Iterate through subkeys under the main key
                foreach (var subKeyName in hkey.GetSubKeyNames())
                {
                    Console.WriteLine($"Processing subkey: {subKeyName}");

                    string printProcessorsKeyPath = $@"{baseKeyPath}\{subKeyName}\Print Processors";

                    // Try to retrieve the 'Directory' value
                    string directory = hkey.GetValue("Directory") as string;

                    if (string.IsNullOrEmpty(directory))
                    {
                        Console.WriteLine($"No 'Directory' value found for subkey: {subKeyName}. Assuming default path.");
                        directory = "W32X86"; // Example fallback value, modify as needed
                    }

                    using (RegistryKey printProcessorsKey = Registry.LocalMachine.OpenSubKey(printProcessorsKeyPath))
                    {
                        if (printProcessorsKey == null)
                        {
                            Console.WriteLine($"No 'Print Processors' key found under: {subKeyName}");
                            continue;
                        }

                        // Iterate through subkeys under "Print Processors"
                        foreach (var processorKeyName in printProcessorsKey.GetSubKeyNames())
                        {
                            Console.WriteLine($"Found Print Processor: {processorKeyName}");

                            string driverName = printProcessorsKey.GetValue("Driver") as string;

                            if (string.IsNullOrEmpty(driverName))
                            {
                                Console.WriteLine($"No 'Driver' value found for processor: {processorKeyName}");
                                continue;
                            }

                            string filePath = $@"C:\Windows\System32\spool\prtprocs\{directory}\{driverName}";
                            Console.WriteLine($"Constructed file path: {filePath}");

                            if (File.Exists(filePath))
                            {
                                var fileInfo = new FileInfo(filePath);
                                string size = fileInfo.Length.ToString();
                                string creationDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                                string company = "Microsoft Corporation"; // Placeholder

                                string val1 = $"{filePath} [{size} {creationDate}] {company}";

                                Logger.Instance.LogPrimary($"HKLM\\...\\{subKeyName}\\Print Processors\\{processorKeyName}: {val1}");
                            }
                            else
                            {
                                Console.WriteLine($"File not found: {filePath}");
                                Logger.Instance.LogPrimary($"HKLM\\...\\{subKeyName}\\Print Processors\\{processorKeyName}: {driverName} (Registry Key)");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PrintProc: {ex.Message}");
        }
    }


  
}

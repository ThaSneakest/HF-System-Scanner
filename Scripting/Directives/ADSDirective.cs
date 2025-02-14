using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ADSDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("ADS::"))
                    {
                        string adsPath = line.Substring("ADS::".Length).Trim();
                        if (!string.IsNullOrEmpty(adsPath))
                        {
                            Console.WriteLine($"ADS:: directive found. Deleting ADS: {adsPath}");
                            DeleteAds(adsPath);
                        }
                        else
                        {
                            Console.WriteLine("ADS:: directive found, but no ADS path was specified.");
                        }
                    }

                    // Add additional directive handling logic here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeleteAds(string adsPath)
        {
            try
            {
                // Attempt to delete the specified ADS
                bool result = Kernel32NativeMethods.DeleteFile(adsPath);

                if (result)
                {
                    Console.WriteLine($"Successfully deleted ADS: {adsPath}");
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to delete ADS: {adsPath}. Error code: {errorCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting ADS '{adsPath}': {ex.Message}");
            }
        }
    }
}

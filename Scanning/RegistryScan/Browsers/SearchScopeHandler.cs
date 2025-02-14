using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class SearchScopeHandler
    {

        private static string SCANB = "Scanning";
        private static string INTERNET4 = "value is missing";
        const string SEARCHSCOPESTRING = "SearchScopeValue";
        private static string VAR = "";

        // Function for handling the registry search and writing results to a file
        public void SEARCHSCOPE(string key, string shive, int checkBoxValue)
        {
            string valDataD = RegistryUtils.RegRead(key, "DefaultScope").ToString();
            string valDataURL;
            string subKey;
            string[] arrayName;

            // Update the GUI control with the search information (Placeholder for UI handling)
            Console.WriteLine($"{SCANB} Internet: {key}");

            // Check if DefaultScope is available
            if (string.IsNullOrEmpty(valDataD))
            {
                if (shive == "HKLM")
                {
                 //   File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> DefaultScope {INTERNET4}{Environment.NewLine}");

                }
            }
            else
            {
                // Read URL associated with DefaultScope
                valDataURL = RegistryUtils.RegRead(key + "\\" + valDataD, "URL").ToString();
                valDataURL = Regex.Replace(valDataURL, "(?i)http(s|):", "hxxp$1:");

                if (checkBoxValue == 1)
                {
                    if (!Regex.IsMatch(valDataURL, "(?i)hxxp(|s)://www.(bing|google|search.live).com/"))
                    {
                    //    File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> DefaultScope {valDataD} URL = {valDataURL}{Environment.NewLine}");
                    }
                }
                else
                {
                 //   File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> DefaultScope {valDataD} URL = {valDataURL}{Environment.NewLine}");
                }
            }

            // Open the registry key for further processing
            RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key);
            if (hKey != null)
            {
                var allStrings = RegistryValueHandler.ListRegistryValues(hKey)
                    .SelectMany(innerList => innerList)
                    .ToArray();
                arrayName = allStrings;


                // Process the registry values
                if (arrayName.Length > 0)
                {
                    foreach (var valName in arrayName)
                    {
                        string valData = RegistryUtils.RegRead(key + "\\" + valName, "Data").ToString();
                        if (Regex.IsMatch(valData, @"\{.+\}") && valName != "DefaultScope")
                        {
                            valData = Regex.Replace(valData, "(?i)http(s|):", "hxxp$1:");
                        //    File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> {valName} {valData}{Environment.NewLine}");
                            break;
                        }
                    }
                }

                // Process subkeys
                int i = 0;
                while (true)
                {
                    subKey = RegistrySubKeyHandler.RegEnumSubKey(hKey, i);
                    if (subKey == null)
                    {
                        break;
                    }

                    valDataURL = RegistryUtils.RegRead(key + "\\" + subKey, "URL").ToString();
                    valDataURL = Regex.Replace(valDataURL, "(?i)http(s|):", "hxxp$1:");

                    if (checkBoxValue == 1)
                    {
                        if (!Regex.IsMatch(valDataURL, "(?i)hxxp(|s)://www.(bing|google|search.live).com/"))
                        {
                        //    File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> {subKey} URL = {valDataURL}{Environment.NewLine}");
                        }
                    }
                    else
                    {
                     //   File.AppendAllText(Logger.WildlandsLogFile, $"{SEARCHSCOPESTRING}: {shive} -> {subKey} URL = {valDataURL}{Environment.NewLine}");
                    }

                    i++;
                }
            }

            // Close the registry key
            hKey?.Close();
        }
       
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{

    internal class InternetExplorerHandler
    {
        public static void IE()
    {
        string[] mainKeys = { "Start Page", "Search Page", "Default_Page_URL", "Default_Search_URL" };
        string key = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Internet Explorer\Main";

        // Process main keys
        foreach (string mainKey in mainKeys)
        {
            IEMAIN(key, mainKey);
        }

        IEMAIN(key, "Local Page");

        foreach (string userReg in RegistryConstants.UserSubKeys)
        {
            string userKey = $@"HKEY_USERS\{userReg}\Software\Microsoft\Internet Explorer\Main";
            IEMAIN(userKey, "Local Page");
            IEMAINREST(userKey);
        }

        string defaultDownloadDirectory = RegistryValueHandler.TryReadRegistryValue(
            Microsoft.Win32.Registry.CurrentUser,
            @"Software\Microsoft\Internet Explorer\Main",
            "Default Download Directory"
        );

            if (!string.IsNullOrEmpty(defaultDownloadDirectory))
        {
            Logger.Instance.LogPrimary($"DownloadDir: {defaultDownloadDirectory}{Environment.NewLine}");
        }

        string hive = "HKLM";
        URLSEARCH();

        foreach (string userReg in RegistryConstants.UserSubKeys)
        {
            if (Regex.IsMatch(userReg, @"S-1-5-\d{2}-\d{3,}"))
            {
                string urlSearchHookKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Explorer\URLSearchHooks";
                string valueName = "{CFBFAE00-17A6-11D0-99CB-00C04FD64497}";

                object regValue;

                using (var baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\URLSearchHooks"))
                {
                    regValue = RegistryValueHandler.TryReadRegistryValue(baseKey, valueName);
                }

                    if (regValue == null)
                {
                    Logger.Instance.LogPrimary($"URLSearchHook: [{userReg}] {StringConstants.UPD1} => {StringConstants.INTERNET3}{Environment.NewLine}");
                }

                hive = $@"HKEY_USERS\{userReg}";
                URLSEARCH();
            }
        }

        if (Utility.GetOSVersion() < 6)
        {
            string aboutUrlsKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\AboutURLs";
            string tabsValue = null;

            // Open HKEY_LOCAL_MACHINE key
            using (var aboutUrlsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(aboutUrlsKeyPath))
            {
                if (aboutUrlsKey != null)
                {
                    tabsValue = RegistryValueHandler.TryReadRegistryValue(aboutUrlsKey, "Tabs")?.ToString();
                }
                else
                {
                    Console.WriteLine($"Registry key not found: {aboutUrlsKeyPath}");
                }
            }

            // Process the Tabs value
            if (!string.IsNullOrEmpty(tabsValue) && tabsValue != "res://ieframe.dll/tabswelcome.htm")
            {
                tabsValue = Regex.Replace(tabsValue, @"(?i)http(s|):", "hxxp$1:");
                Logger.Instance.LogPrimary($@"HKEY_LOCAL_MACHINE\{aboutUrlsKeyPath},Tabs: ""{tabsValue}"" <==== {StringConstants.UPD1}{Environment.NewLine}");
            }

            // Iterate through user registry paths
            foreach (string userReg in RegistryConstants.UserSubKeys)
            {
                string userAboutUrlsKeyPath = $@"{userReg}\SOFTWARE\Microsoft\Internet Explorer\AboutURLs";
                string userTabsValue = null;

                // Open HKEY_USERS key
                using (var userAboutUrlsKey = Microsoft.Win32.Registry.Users.OpenSubKey(userAboutUrlsKeyPath))
                {
                    if (userAboutUrlsKey != null)
                    {
                        userTabsValue = RegistryValueHandler.TryReadRegistryValue(userAboutUrlsKey, "Tabs")?.ToString();
                    }
                }

                // Process the user Tabs value
                if (!string.IsNullOrEmpty(userTabsValue))
                {
                    userTabsValue = Regex.Replace(userTabsValue, @"(?i)http(s|):", "hxxp$1:");
                    Logger.Instance.LogPrimary($@"HKEY_USERS\{userAboutUrlsKeyPath},Tabs: ""{userTabsValue}"" <==== {StringConstants.UPD1}{Environment.NewLine}");
                }
            }

        }

        SEARCHSCOPE(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\SearchScopes", "HKLM");

        foreach (string userReg in RegistryConstants.UserSubKeys)
        {
            SEARCHSCOPE($@"HKEY_USERS\{userReg}\SOFTWARE\Microsoft\Internet Explorer\SearchScopes", $@"HKEY_USERS\{userReg}");
        }

        BHO(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects");

        TOOLBAR(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Toolbar");

        foreach (string userReg in RegistryConstants.UserSubKeys)
        {
            string toolbarKey = $@"HKEY_USERS\{userReg}\Software\Microsoft\Internet Explorer\Toolbar\WebBrowser";
            TOOLBAR(toolbarKey);

            object continuousBrowsing = RegistryValueHandler.TryReadRegistryValue(
                Microsoft.Win32.Registry.Users,
                $@"{userReg}\Software\Microsoft\Internet Explorer\ContinuousBrowsing",
                "Enabled"
            );


                if (continuousBrowsing != null)
            {
                Logger.Instance.LogPrimary($"IE Session Restore: HKEY_USERS\\{userReg} -> {StringConstants.INTERNET2}{Environment.NewLine}");
            }
        }

        PDF(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Code Store Database\Distribution Units");
        HANDLER("Handler");
        HANDLER("Filter");

        IEPREFIX(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\URL\");

            string keyPath = @"SOFTWARE\Clients\StartMenuInternet";
            string[] startMenuInternetIE = { "0" };
            int p = 0;

            using (var hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (hKey == null)
                {
                    Console.WriteLine($"The registry key '{keyPath}' does not exist.");
                    return;
                }

                var subKeyNames = hKey.GetSubKeyNames(); // Get all subkey names

                while (p < subKeyNames.Length) // Iterate through the subkeys
                {
                    string operaBro = subKeyNames[p]; // Access subkey name by index

                    if (operaBro.IndexOf("IEXPLORE", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        BROWSERSTART(operaBro, ref startMenuInternetIE, "HKLM");
                    }

                    p++;
                }
            }



            if (startMenuInternetIE.Length > 1)
        {
            startMenuInternetIE[1] = Regex.Replace(startMenuInternetIE[1], @"(?i)http(s|):", "hxxp$1:");
            Logger.Instance.LogPrimary(startMenuInternetIE[1] + Environment.NewLine);
        }
    }
        public static void IEMAIN(string registryKeyPath, string valueName)
        {
            // Log file to record results
            string logFilePath = "HADDITION.log";

            try
            {
                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (registryKey != null)
                    {
                        // Attempt to read the registry value
                        object regValue = RegistryValueHandler.TryReadRegistryValue(registryKey, valueName);

                        if (regValue != null)
                        {
                            // Log the registry value
                            File.AppendAllText(logFilePath, $"{registryKeyPath}\\{valueName}: {regValue}{Environment.NewLine}");
                        }
                        else
                        {
                            // Handle cases where the registry value does not exist
                            File.AppendAllText(logFilePath, $"{registryKeyPath}\\{valueName}: Not Found{Environment.NewLine}");
                        }
                    }
                    else
                    {
                        // Handle cases where the registry key does not exist
                        File.AppendAllText(logFilePath, $"{registryKeyPath}: Key Not Found{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing {registryKeyPath}\\{valueName}: {ex.Message}{Environment.NewLine}");
            }
        }


        public static void IEMAINREST(string registryKeyPath)
        {
            // Log file to record results
            string logFilePath = "HADDITION.log";

            // Example of processing specific registry values for Internet Explorer
            string[] valueNames = { "Search Page", "Start Page", "Default_Page_URL", "Default_Search_URL" };

            try
            {
                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (registryKey == null)
                    {
                        File.AppendAllText(logFilePath, $"{registryKeyPath}: Key Not Found{Environment.NewLine}");
                        return;
                    }

                    foreach (string valueName in valueNames)
                    {
                        try
                        {
                            // Read the registry value
                            object regValue = registryKey.GetValue(valueName);

                            if (regValue != null)
                            {
                                // Log the registry value
                                File.AppendAllText(logFilePath, $"{registryKeyPath}\\{valueName}: {regValue}{Environment.NewLine}");
                            }
                            else
                            {
                                // Log if the value does not exist
                                File.AppendAllText(logFilePath, $"{registryKeyPath}\\{valueName}: Not Found{Environment.NewLine}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log any errors for individual values
                            File.AppendAllText(logFilePath, $"Error accessing {registryKeyPath}\\{valueName}: {ex.Message}{Environment.NewLine}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors when accessing the registry key
                File.AppendAllText(logFilePath, $"Error accessing {registryKeyPath}: {ex.Message}{Environment.NewLine}");
            }
        }

        public static void URLSEARCH()
        {
            // Define the registry key path for URL search hooks
            string registryKeyPath = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Internet Explorer\URLSearchHooks";

            // Log file for recording results (replace `HADDITION` with actual log file path)
            string logFilePath = "HADDITION.log";

            try
            {
                // Open the registry key
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Internet Explorer\URLSearchHooks"))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetValueNames())
                        {
                            // Read the registry value
                            object regValue = key.GetValue(subKeyName);

                            if (regValue != null)
                            {
                                // Log the registry value
                                File.AppendAllText(logFilePath, $"URLSearchHook: {subKeyName} = {regValue}{Environment.NewLine}");
                            }
                            else
                            {
                                // Log if the value does not exist
                                File.AppendAllText(logFilePath, $"URLSearchHook: {subKeyName} = Not Found{Environment.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        File.AppendAllText(logFilePath, $"URLSearchHooks key not found.{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error in URLSEARCH: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void SEARCHSCOPE(string registryKeyPath, string hiveName)
        {
            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"{hiveName}\\SearchScopes not found.{Environment.NewLine}");
                        return;
                    }

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                string displayName = subKey.GetValue("DisplayName")?.ToString() ?? "N/A";
                                string url = subKey.GetValue("URL")?.ToString() ?? "N/A";

                                // Log the search scope details
                                File.AppendAllText(logFilePath, $"{hiveName}\\{subKeyName}: DisplayName = {displayName}, URL = {url}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing {hiveName}\\SearchScopes: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void BHO(string registryKeyPath)
        {
            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"BHO key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        string subKeyPath = $@"{registryKeyPath}\{subKeyName}";
                        using (RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKeyPath))
                        {
                            if (subKey != null)
                            {
                                string clsid = subKeyName; // CLSID of the BHO
                                string friendlyName = subKey.GetValue("")?.ToString() ?? "N/A"; // Default value (optional)

                                // Log the BHO details
                                File.AppendAllText(logFilePath, $"BHO CLSID: {clsid}, Friendly Name: {friendlyName}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing BHO registry key: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void TOOLBAR(string registryKeyPath)
        {
            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"Toolbar key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    foreach (string subKeyName in key.GetValueNames())
                    {
                        string value = key.GetValue(subKeyName)?.ToString() ?? "N/A";

                        // Log the toolbar CLSID and associated value
                        File.AppendAllText(logFilePath, $"Toolbar CLSID: {subKeyName}, Value: {value}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing Toolbar registry key: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void PDF(string registryKeyPath)
        {
            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"PDF key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        string subKeyPath = $@"{registryKeyPath}\{subKeyName}";
                        using (RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKeyPath))
                        {
                            if (subKey != null)
                            {
                                string friendlyName = subKey.GetValue("FriendlyName")?.ToString() ?? "N/A";
                                string version = subKey.GetValue("Version")?.ToString() ?? "N/A";

                                // Log the PDF component details
                                File.AppendAllText(logFilePath, $"PDF Component: {friendlyName}, Version: {version}, Path: {subKeyPath}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing PDF registry key: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void HANDLER(string handlerType)
        {
            // Define the registry key path based on the handler type
            string registryKeyPath;

            if (handlerType.ToLower() == "handler")
            {
                registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\URL\Handlers";
            }
            else if (handlerType.ToLower() == "filter")
            {
                registryKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\Filter";
            }
            else
            {
                throw new ArgumentException($"Unknown handler type: {handlerType}");
            }

            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"Handler key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    foreach (string valueName in key.GetValueNames())
                    {
                        string valueData = key.GetValue(valueName)?.ToString() ?? "N/A";

                        // Log the handler details
                        File.AppendAllText(logFilePath, $"Handler: {valueName}, Data: {valueData}{Environment.NewLine}");
                    }

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        string subKeyPath = $@"{registryKeyPath}\{subKeyName}";
                        using (RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKeyPath))
                        {
                            if (subKey != null)
                            {
                                string subValueData = subKey.GetValue("")?.ToString() ?? "N/A";
                                File.AppendAllText(logFilePath, $"Handler SubKey: {subKeyName}, Data: {subValueData}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing Handler registry key: {ex.Message}{Environment.NewLine}");
            }
        }
        public static void IEPREFIX(string registryKeyPath)
        {
            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"IEPREFIX key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    foreach (string valueName in key.GetValueNames())
                    {
                        string valueData = key.GetValue(valueName)?.ToString() ?? "N/A";

                        // Modify the prefix if needed (e.g., replace "http" with "hxxp")
                        string modifiedData = System.Text.RegularExpressions.Regex.Replace(valueData, @"(?i)http(s|):", "hxxp$1:");

                        // Log the original and modified values
                        File.AppendAllText(logFilePath, $"Prefix: {valueName}, Original: {valueData}, Modified: {modifiedData}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing IEPREFIX registry key: {ex.Message}{Environment.NewLine}");
            }
        }

        public static void BROWSERSTART(string browserName, ref string[] browserDetails, string hiveName)
        {
            // Define the registry key path for the browser
            string registryKeyPath = $@"{hiveName}\SOFTWARE\Clients\StartMenuInternet\{browserName}";

            // Log file for recording results
            string logFilePath = "HADDITION.log"; // Replace with the actual log file path

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key == null)
                    {
                        File.AppendAllText(logFilePath, $"Browser key not found: {registryKeyPath}{Environment.NewLine}");
                        return;
                    }

                    // Retrieve and log browser details
                    string browserDisplayName = key.GetValue("")?.ToString() ?? "N/A";
                    string command = key.OpenSubKey(@"shell\open\command")?.GetValue("")?.ToString() ?? "N/A";

                    // Update the browserDetails array
                    browserDetails = new string[] { browserName, browserDisplayName, command };

                    // Log the results
                    File.AppendAllText(logFilePath, $"Browser: {browserName}, Display Name: {browserDisplayName}, Command: {command}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                File.AppendAllText(logFilePath, $"Error accessing browser registry key: {ex.Message}{Environment.NewLine}");
            }
        }
        public static int IEMAIN(string keyPath, string valueName, string additionFilePath, bool checkboxChecked)
        {
            try
            {
                // Determine the registry root and subkey
                RegistryKey rootKey = null;
                string subKeyPath = string.Empty;

                if (keyPath.StartsWith("HKLM"))
                {
                    rootKey = Microsoft.Win32.Registry.LocalMachine;
                    subKeyPath = keyPath.Replace("HKLM\\", "");
                }
                else if (keyPath.StartsWith("HKCU"))
                {
                    rootKey = Microsoft.Win32.Registry.CurrentUser;
                    subKeyPath = keyPath.Replace("HKCU\\", "");
                }
                else
                {
                    throw new ArgumentException("Unsupported registry root. Only HKLM and HKCU are supported.");
                }

                // Open the registry key
                using (var subKey = rootKey.OpenSubKey(subKeyPath))
                {
                    if (subKey == null)
                    {
                        // Registry key does not exist
                        File.AppendAllText(additionFilePath, $"{keyPath},{valueName} = {Environment.NewLine}");
                        return 1;
                    }

                    // Read the registry value
                    object valDataObj = RegistryValueHandler.TryReadRegistryValue(subKey, valueName);

                    // If the registry value does not exist
                    if (valDataObj == null)
                    {
                        File.AppendAllText(additionFilePath, $"{keyPath},{valueName} = {Environment.NewLine}");
                        return 1;
                    }

                    // Process the registry value
                    string valData = valDataObj.ToString();
                    valData = Regex.Replace(valData, @"(?i)http(s|):", "hxxp$1:");

                    // Conditional logic based on checkboxChecked
                    if (checkboxChecked)
                    {
                        if (valueName == "Local Page" &&
                            valData.IndexOf(@"Windows\System32\blank.htm", StringComparison.OrdinalIgnoreCase) == -1 &&
                            !Regex.IsMatch(valData, @"(?i)%\d\d%\\blank\.htm"))
                        {
                            File.AppendAllText(additionFilePath, $"{keyPath},{valueName} = {valData}{Environment.NewLine}");
                        }

                        if (Regex.IsMatch(valueName, @"Start Page|Search Page|Default_Page_URL|Default_Search_URL|Start Page Redirect Cache") &&
                            !Regex.IsMatch(valData, @"\A(?i)hxxp(|s)://(go\.microsoft\.com|www\.msn\.com)/"))
                        {
                            File.AppendAllText(additionFilePath, $"{keyPath},{valueName} = {valData}{Environment.NewLine}");
                        }
                    }
                    else
                    {
                        // Always write to the addition file if checkbox is not checked
                        File.AppendAllText(additionFilePath, $"{keyPath},{valueName} = {valData}{Environment.NewLine}");
                    }

                    return 0; // Success
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions if needed
                Console.WriteLine($"Error accessing registry key {keyPath} and value {valueName}: {ex.Message}");
                return 1; // Failure
            }
        }

        

        public static void IEMAINREST(string key, string additionFilePath, bool checkboxChecked)
        {
            int i = 1;

            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {key}");
                        return;
                    }

                    // Enumerate all values in the registry key
                    foreach (string valueName in registryKey.GetValueNames())
                    {
                        if (valueName == "Local Page")
                            continue;

                        object valueDataObj = registryKey.GetValue(valueName);

                        if (valueDataObj == null)
                            continue;

                        string valueData = valueDataObj.ToString();

                        // Replace "http" or "https" with "hxxp" for obfuscation
                        valueData = Regex.Replace(valueData, @"(?i)http(s|):", "hxxp$1:");

                        // Check if the value matches certain patterns
                        if (Regex.IsMatch(valueData, @"(?i)(hxxp(|s):|www\.|\.com)") || valueName == "Start Page")
                        {
                            if (checkboxChecked)
                            {
                                // Write specific values based on conditions
                                if (Regex.IsMatch(valueName, @"(Start Page|Search Page|Default_Page_URL|Default_Search_URL|Start Page Redirect Cache)") &&
                                    !Regex.IsMatch(valueData, @"\A(?i)hxxp(|s)://(go\.microsoft\.com|www\.msn\.com)/") &&
                                    !Regex.IsMatch(valueData, @"\A(?i)(hxxp(|s)://|)(www\.|)google\.co(m|m/)\Z"))
                                {
                                    File.AppendAllText(additionFilePath, $"{key},{valueName} = {valueData}{Environment.NewLine}");
                                }
                            }
                            else
                            {
                                // Write to the addition file
                                File.AppendAllText(additionFilePath, $"{key},{valueName} = {valueData}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing registry key {key}: {ex.Message}");
            }
        }

        public static void IEPREFIX(string key, string additionFilePath, string updateIndicator)
        {
            const string DEFAULT_PREFIX = "DefaultPrefix";
            const string PREFIXES = "Prefixes";

            try
            {
                // Open the base registry key
                using (RegistryKey baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key {key} not found.");
                        return;
                    }

                    // Check DefaultPrefix
                    using (RegistryKey defaultPrefixKey = baseKey.OpenSubKey("DefaultPrefix"))
                    {
                        if (defaultPrefixKey != null)
                        {
                            string defaultPrefixData = defaultPrefixKey.GetValue(string.Empty)?.ToString();
                            if (!string.IsNullOrEmpty(defaultPrefixData) && defaultPrefixData != "http://")
                            {
                                File.AppendAllText(additionFilePath, $"{DEFAULT_PREFIX}: => {defaultPrefixData} <==== {updateIndicator}{Environment.NewLine}");
                            }
                        }
                    }

                    // Check Prefixes
                    using (RegistryKey prefixesKey = baseKey.OpenSubKey("Prefixes"))
                    {
                        if (prefixesKey != null)
                        {
                            // Check "home" prefix
                            string homePrefixData = prefixesKey.GetValue("home")?.ToString();
                            if (!string.IsNullOrEmpty(homePrefixData) && homePrefixData != "http://")
                            {
                                File.AppendAllText(additionFilePath, $"{PREFIXES}: [home]=> {homePrefixData} <==== {updateIndicator}{Environment.NewLine}");
                            }

                            // Check "www" prefix
                            string wwwPrefixData = prefixesKey.GetValue("www")?.ToString();
                            if (!string.IsNullOrEmpty(wwwPrefixData) && wwwPrefixData != "http://")
                            {
                                File.AppendAllText(additionFilePath, $"{PREFIXES}: [www]=> {wwwPrefixData} <==== {updateIndicator}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error to console or optionally append to the log file
                Console.WriteLine($"Error in IEPREFIX: {ex.Message}");
            }
        }



       

        public static void IEZONESITES(string additionFilePath, string[] userReg)
        {
            List<string> trustedSites = new List<string>();
            List<string> restrictedSites = new List<string>();
            string ieVersion = "Internet Explorer";

            try
            {
                // Determine the Internet Explorer version
                if (Environment.OSVersion.Version.Major < 6 ||
                    (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2))
                {
                    string softwareKey = @"SOFTWARE\Microsoft\Internet Explorer";
                    using (RegistryKey ieKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(softwareKey))
                    {
                        if (ieKey != null)
                        {
                            string ieVersion0 = ieKey.GetValue("svcVersion")?.ToString()
                                                ?? ieKey.GetValue("Version")?.ToString();

                            if (!string.IsNullOrEmpty(ieVersion0))
                            {
                                ieVersion0 = Regex.Replace(ieVersion0, @"((\d|\d\d))\..+", "$1");
                                ieVersion = $"{ieVersion} (Detected Version: {ieVersion0})";
                            }
                        }
                    }
                }

                // Log IE version and a header
                File.AppendAllText(additionFilePath, $"\n==================== {ieVersion} ====================\n\n");

                // Iterate through user registry paths
                foreach (string userRegKey in userReg)
                {
                    string domainKeyPath = $@"{userRegKey}\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains";

                    try
                    {
                        using (RegistryKey domainKey = Microsoft.Win32.Registry.Users.OpenSubKey(domainKeyPath))
                        {
                            if (domainKey == null)
                                continue;

                            foreach (string domain in domainKey.GetSubKeyNames())
                            {
                                string domainPath = $@"{domainKeyPath}\{domain}";

                                using (RegistryKey subDomainKey = Microsoft.Win32.Registry.Users.OpenSubKey(domainPath))
                                {
                                    if (subDomainKey == null)
                                        continue;

                                    foreach (string subDomain in subDomainKey.GetSubKeyNames())
                                    {
                                        string fullDomain = $"{subDomain}.{domain}";
                                        ProcessDomain(fullDomain, subDomainKey, trustedSites, restrictedSites, userRegKey, additionFilePath);
                                    }

                                    ProcessDomain(domain, subDomainKey, trustedSites, restrictedSites, userRegKey, additionFilePath);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing registry path {domainKeyPath}: {ex.Message}");
                    }
                }

                // Log the collected trusted and restricted sites
                File.AppendAllText(additionFilePath, "\nTrusted Sites:\n");
                File.AppendAllLines(additionFilePath, trustedSites);

                File.AppendAllText(additionFilePath, "\nRestricted Sites:\n");
                File.AppendAllLines(additionFilePath, restrictedSites);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IEZONESITES: {ex.Message}");
            }
        }


        public static void ProcessDomain(string domain, RegistryKey subDomainKey, List<string> trustedSites, List<string> restrictedSites, string userRegKey, string additionFilePath)
        {
            foreach (string valueName in subDomainKey.GetValueNames())
            {
                string prefix = valueName.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0 ? "hxxp://" : string.Empty;


                string valueData = subDomainKey.GetValue(valueName)?.ToString();
                if (valueData == "2") // Trusted Sites
                {
                    trustedSites.Add($"IE trusted site: HKU\\{userRegKey}\\...\\{domain} -> {prefix}{domain}");
                }
                else if (valueData == "4") // Restricted Sites
                {
                    restrictedSites.Add($"IE restricted site: HKU\\{userRegKey}\\...\\{domain} -> {prefix}{domain}");
                }
            }
        }


        public static string[] AppendToArray(string[] array, string item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
            return array;
        }

        public static void IEZONESITES1(string[] pu, string additionFilePath, string rest1, string rest2)
        {
            // Check if there are elements in the array beyond the first one
            if (pu.Length > 1)
            {
                for (int i = 1; i < pu.Length; i++)
                {
                    // Write the current element to the log file
                    File.AppendAllText(additionFilePath, pu[i] + Environment.NewLine);

                    // If more than 19 elements have been logged, write a summary and exit the loop
                    if (i > 19)
                    {
                        int remainingCount = pu.Length - (i + 1);
                        File.AppendAllText(additionFilePath, $"{Environment.NewLine}{rest1} {remainingCount} {rest2}.{Environment.NewLine}{Environment.NewLine}");
                        break;
                    }
                }
            }
        }
       

        public static void INTERNET(string logFilePath, string[] userRegKeys, string restrictMessage, string updateIndicator)
        {
            try
            {
                // Initialize the log file
                File.WriteAllText(logFilePath, "\n==================== Internet Settings ====================\n");

                // Handle Internet Settings Zones for HKLM
                ProcessInternetZones(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones", logFilePath);

                // Process user-specific Internet Settings Zones
                foreach (string userKey in userRegKeys)
                {
                    string userZoneKey = $@"HKEY_USERS\{userKey}\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones";
                    ProcessInternetZones(userZoneKey, logFilePath);
                }

                // Handle Proxy Settings
                HandleProxySettings(logFilePath, updateIndicator, restrictMessage);

                // Handle additional Internet settings
                HandleAdditionalInternetSettings(logFilePath, updateIndicator, restrictMessage, userRegKeys);


                Console.WriteLine("Internet settings processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Internet settings: {ex.Message}");
            }
        }

        public static void ProcessInternetZones(string registryPath, string logFilePath)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key == null) return;

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        File.AppendAllText(logFilePath, $"Internet Zone Key: {subKeyName}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Internet zones for {registryPath}: {ex.Message}");
            }
        }

        public static void HandleProxySettings(string logFilePath, string updateIndicator, string restrictMessage)
        {
            const string proxyKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

            try
            {
                using (RegistryKey proxyKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(proxyKeyPath))
                {
                    if (proxyKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {proxyKeyPath}");
                        return;
                    }

                    // Proxy Enable
                    object proxyEnable = proxyKey.GetValue("ProxyEnable");
                    if (proxyEnable is int && (int)proxyEnable == 1)
                    {
                        File.AppendAllText(logFilePath, $"ProxyEnable: [HKLM] => Proxy Enabled {restrictMessage}\n");
                    }

                    // Proxy Server
                    string proxyServer = proxyKey.GetValue("ProxyServer")?.ToString();
                    if (!string.IsNullOrEmpty(proxyServer))
                    {
                        proxyServer = Regex.Replace(proxyServer, "(?i)http(s|):", "hxxp$1:");
                        File.AppendAllText(logFilePath, $"ProxyServer: [HKLM] => {proxyServer}\n");
                    }

                    // Auto Config URL
                    string autoConfigUrl = proxyKey.GetValue("AutoConfigURL")?.ToString();
                    if (!string.IsNullOrEmpty(autoConfigUrl))
                    {
                        autoConfigUrl = Regex.Replace(autoConfigUrl, "(?i)http(s|):", "hxxp$1:");
                        File.AppendAllText(logFilePath, $"AutoConfigURL: [HKLM] => {autoConfigUrl} {updateIndicator}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling proxy settings: {ex.Message}");
            }
        }


        public static void HandleAdditionalInternetSettings(string logFilePath, string updateIndicator, string restrictMessage, string[] userRegKeys)
        {
            const string tcpipParametersPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";

            try
            {
                // Handle Tcpip Parameters
                using (RegistryKey tcpipParameters = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(tcpipParametersPath))
                {
                    if (tcpipParameters != null)
                    {
                        string dhcpNameServer = tcpipParameters.GetValue("DhcpNameServer")?.ToString();
                        if (!string.IsNullOrEmpty(dhcpNameServer))
                        {
                            File.AppendAllText(logFilePath, $"Tcpip\\Parameters: [DhcpNameServer] {dhcpNameServer}\n");
                        }

                        string nameServer = tcpipParameters.GetValue("NameServer")?.ToString();
                        if (!string.IsNullOrEmpty(nameServer))
                        {
                            File.AppendAllText(logFilePath, $"Tcpip\\Parameters: [NameServer] {nameServer}\n");
                        }
                    }
                }

                // Process PersistentRoutes
                const string persistentRoutesPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\PersistentRoutes";
                using (RegistryKey persistentRoutes = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(persistentRoutesPath))
                {
                    if (persistentRoutes != null)
                    {
                        foreach (string valueName in persistentRoutes.GetValueNames())
                        {
                            object valueData = persistentRoutes.GetValue(valueName);
                            if (valueData != null)
                            {
                                File.AppendAllText(logFilePath, $"PersistentRoute: {valueName} => {valueData}\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling additional Internet settings: {ex.Message}");
            }
        }


        public static void INTERNET0(string key, string subKey, string valueName, string logFilePath)
        {
            try
            {
                // Construct the full registry key path
                string fullKeyPath = $@"{key}\{subKey}";

                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullKeyPath))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {fullKeyPath}");
                        return;
                    }

                    // Read the registry value
                    object valueData = registryKey.GetValue(valueName);

                    // If the value exists and is not empty, write it to the log file
                    if (valueData != null && valueData.ToString() != "")
                    {
                        File.AppendAllText(logFilePath, $@"Tcpip\..\Interfaces\{subKey}: [{valueName}] {valueData}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in INTERNET0 for key {key}\\{subKey}, value {valueName}: {ex.Message}");
            }
        }

        public static void CHROME1(List<string> ret, string logFilePath, string browser = "Edge", bool isCheckboxChecked = false)
        {
            try
            {
                // Check if the input list has at least two elements
                if (ret == null || ret.Count < 2)
                    return;

                // Write the browser header to the log file
                File.AppendAllText(logFilePath, $"\n{browser}:\n=======\n");

                // Remove duplicates from the list
                ret = new HashSet<string>(ret).ToList();

                // Iterate through the entries in the list
                foreach (var entry in ret)
                {
                    string modifiedEntry = entry;

                    // Skip certain entries for Yandex when the checkbox is checked
                    if (isCheckboxChecked && browser == "Yandex" && Regex.IsMatch(modifiedEntry, @"(?i)YAN (DefaultSearchURL: Default -> {yandex:baseURL}{yandex:searchPath}|DefaultSearchKeyword: Default -> yandex.ru|DefaultSuggestURL: Default -> {yandex:baseSuggestURL})"))
                    {
                        continue;
                    }

                    // Replace "http" or "https" with "hxxp" for obfuscation
                    modifiedEntry = Regex.Replace(modifiedEntry, "(?i)http(s|):", "hxxp$1:");

                    // Write the modified entry to the log file
                    File.AppendAllText(logFilePath, modifiedEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CHROME1 for browser {browser}: {ex.Message}");
            }
        }
        public static void URLSEARCH(string hive, string defA, string ff1, string regist8, string hadAddition, string checkbox11)
        {
            string key = hive + @"\SOFTWARE\Microsoft\Internet Explorer\URLSearchHooks";
            Console.WriteLine($"Internet: {key}");

            // Read the default value from the registry key
            string devalue = RegistryValueHandler.ReadRegistryValue(key, "");
            if (!string.IsNullOrEmpty(devalue))
            {
                //Logger.Instance.WriteToFile(hadAddition, $"URLSearchHook: {hive} -> {defA} = {devalue}");
            }

            int i = 1;
            while (true)
            {
                string valUrl = RegistryValueHandler.EnumerateRegistryValue(key, i);
                if (string.IsNullOrEmpty(valUrl)) break;

                if (Regex.IsMatch(valUrl, @"\{.+\}"))
                {
                    string valName = RegistryValueHandler.ReadRegistryValue($"HKCR\\CLSID\\{valUrl}", "");
                    if (string.IsNullOrEmpty(valName)) valName = $"({ff1})";

                    string filePath = RegistryValueHandler.ReadRegistryValue($"HKCR\\CLSID\\{valUrl}\\InprocServer32", "");
                    if (filePath.Contains("mscoree.dll"))
                    {
                        MSCOREEHandler.MSCOREE($"HKCR\\CLSID\\{valUrl}\\InprocServer32", ref filePath);
                    }

                    string file = filePath;
                    FileUtils.AAAAFP();

                    if (!File.Exists(file))
                    {
                        filePath = $"{filePath} {regist8}";
                    }
                    else
                    {
                        filePath = file;
                    }

                    if (checkbox11 == "1")
                    {
                        if (valUrl != "{CFBFAE00-17A6-11D0-99CB-00C04FD64497}" || !filePath.Contains("\\System32\\ieframe.dll"))
                        {
                            //Logger.WriteToFile(hadAddition, $"URLSearchHook: {hive} - {valName} - {valUrl} - {filePath}");
                        }
                    }
                    else
                    {
                        //Logger.WriteToFile(hadAddition, $"URLSearchHook: {hive} - {valName} - {valUrl} - {filePath}");
                    }
                }

                i++;
            }
        }
       

        public static void GetInternetExplorerVersion()
        {
            string internetExplorerVersion = FileVersionInfo.GetVersionInfo(FileConstants.InternetExplorerPath).ProductVersion;
        }

        public static string GetURLSearchHooks()
        {
            StringBuilder URLSearchHooks = new StringBuilder();

            // Check CurrentUser registry
            ReadURLSearchHooks(@"Software\Microsoft\Internet Explorer\URLSearchHooks", "HKCU\\URLSH", URLSearchHooks);

            // Check LocalMachine registry
            ReadURLSearchHooks(@"Software\Microsoft\Internet Explorer\URLSearchHooks", "HKLM\\URLSH", URLSearchHooks);

            // Check Users registry
            ReadURLSearchHooks(@".Default\Software\Microsoft\Internet Explorer\URLSearchHooks", "HKU\\URLSH", URLSearchHooks);

            return URLSearchHooks.ToString();
        }

        public static void ReadURLSearchHooks(string subKey, string prefix, StringBuilder output)
        {
            try
            {
                RegistryKey CSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subKey);

                if (CSK != null)
                {
                    foreach (string S in CSK.GetValueNames())
                    {
                        try
                        {
                            string A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S, null, null);
                            string B = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + S + @"\InprocServer32", null, null);

                            if (B != null)
                            {
                                output.Append(prefix + ": \t" + S + " ");
                                if (A != null) output.Append(A + " - ");
                                output.Append(B + Environment.NewLine);
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
        }

        public static string GetTrustedSites()
        {
            StringBuilder TrustedSites = new StringBuilder();

            try
            {
                using (RegistryKey trustSitesCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains"))
                {
                    if (trustSitesCSK != null)
                    {
                        foreach (string S1 in trustSitesCSK.GetSubKeyNames())
                        {
                            string A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\" + S1, "*", null);
                            string B = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\" + S1, "https", null);

                            if (A == "2") TrustedSites.Append("Trusted: http://*." + S1 + Environment.NewLine);
                            if (B == "2") TrustedSites.Append("Trusted: https://*." + S1 + Environment.NewLine);

                            using (RegistryKey subCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\" + S1))
                            {
                                if (subCSK != null)
                                {
                                    foreach (string S2 in subCSK.GetSubKeyNames())
                                    {
                                        A = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\" + S1 + @"\" + S2, "*", null);
                                        B = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\" + S1 + @"\" + S2, "https", null);

                                        if (A == "2") TrustedSites.Append("Trusted: http://" + S2 + "." + S1 + Environment.NewLine);
                                        if (B == "2") TrustedSites.Append("Trusted: https://" + S2 + "." + S1 + Environment.NewLine);
                                    }
                                }
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

            return TrustedSites.ToString();
        }
    }
}

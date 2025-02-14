using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan.Browsers
{
    public class InternetExplorerScan
    {
        public object GetRegistryValue(string key, string valueName)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        return registryKey.GetValue(valueName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public string GetIEDefaultURLs()
        {
            string HKLM_SearchPage =
                ConvertRegistryValueToString(GetRegistryValue(@"SOFTWARE\Microsoft\Internet Explorer\MAIN",
                    "Search Page"));
            string HKLM_StartPage =
                ConvertRegistryValueToString(GetRegistryValue(@"SOFTWARE\Microsoft\Internet Explorer\MAIN",
                    "Start Page"));

            // Note the change here to use the correct method for accessing the current user's registry keys
            string HKCU_SearchPage = ConvertRegistryValueToString(
                GetRegistryValueFromCurrentUser(@"Software\Microsoft\Internet Explorer\Main", "Search Page"));
            string HKCU_StartPage =
                ConvertRegistryValueToString(
                    GetRegistryValueFromCurrentUser(@"Software\Microsoft\Internet Explorer\Main", "Start Page"));

            string IEDefaultURLs =
                "HKLM\\Search Page: " + HKLM_SearchPage + Environment.NewLine +
                "HKLM\\Start Page: " + HKLM_StartPage + Environment.NewLine +
                "HKCU\\Search Page: " + HKCU_SearchPage + Environment.NewLine +
                "HKCU\\Start Page: " + HKCU_StartPage + Environment.NewLine;

            return IEDefaultURLs;
        }

        public string GetProxySettings()
        {
            string ProxyEnable = ConvertRegistryValueToString(
                GetRegistryValueFromCurrentUser(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                    "ProxyEnable"));
            string ProxyOverride = ConvertRegistryValueToString(
                GetRegistryValueFromCurrentUser(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                    "ProxyOverride"));
            string ProxyServer = ConvertRegistryValueToString(
                GetRegistryValueFromCurrentUser(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings",
                    "ProxyServer"));

            string ProxySettings =
                "Proxy Enable: " + ProxyEnable + Environment.NewLine +
                "Proxy Override: " + ProxyOverride + Environment.NewLine +
                "Proxy Server: " + ProxyServer + Environment.NewLine;

            return ProxySettings;
        }

        private string ConvertRegistryValueToString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is string stringValue)
            {
                return stringValue;
            }

            if (value is int intValue)
            {
                return intValue.ToString();
            }

            // Handle other types if necessary
            return value.ToString();
        }

        private object GetRegistryValueFromCurrentUser(string key, string valueName)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        return registryKey.GetValue(valueName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.RegistryScan.Browsers
{
    public class DefaultBrowserScan
    {
        public static string GetDefaultWebBrowser()
        {
            try
            {
                // Open the registry key for the default web browser
                using (RegistryKey userChoiceKey =
                       Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                           @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
                {
                    if (userChoiceKey != null)
                    {
                        object progId = userChoiceKey.GetValue("ProgId");
                        if (progId != null)
                        {
                            // Fetch browser display name based on ProgId
                            return GetBrowserDisplayName(progId.ToString());
                        }
                    }
                }
            }
            catch
            {
                // Log the error or handle accordingly
            }

            return "Unknown Browser";
        }

        public static string GetBrowserDisplayName(string progId)
        {
            try
            {
                // Query the browser name based on ProgId from the registry
                using (RegistryKey progIdKey =
                       Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(progId + @"\Application"))
                {
                    if (progIdKey != null)
                    {
                        object appName = progIdKey.GetValue("ApplicationName");
                        if (appName != null)
                        {
                            return appName.ToString();
                        }
                    }
                }

                // Fallback to the ProgId if ApplicationName is not available
                return progId;
            }
            catch
            {
                return progId; // Return ProgId as a fallback
            }
        }
    }
}
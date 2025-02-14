using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Constants
{
    public class SystemConstants
    {
        public static string OperatingSystemVersion = Environment.OSVersion.VersionString;
        public static string ServicePack = "";
        public static string BootMode = "";
        public static int OperatingSystemNumberMajor = Environment.OSVersion.Version.Major;
        public static int OperatingSystemNumberMinor = Environment.OSVersion.Version.Minor;
        public static OperatingSystem OperatingSystemNumber = Environment.OSVersion;
        public static string CurrentUserName = Environment.UserName;

        public static string GetOSVersion()
        {
            try
            {
                string osDescription = RuntimeInformation.OSDescription;
                Version osVersion = Environment.OSVersion.Version;

                return $"OS: {osDescription}, Version: {osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";
            }
            catch (Exception ex)
            {
                return $"Failed to retrieve OS version: {ex.Message}";
            }
        }

    }
}

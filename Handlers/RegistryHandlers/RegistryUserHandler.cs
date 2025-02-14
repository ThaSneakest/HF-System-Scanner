using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryUserHandler
    {
        public static string EnumerateUserKeys(int index)
        {
            try
            {
                using (var usersKey = Microsoft.Win32.Registry.Users)
                {
                    var subKeyNames = usersKey.GetSubKeyNames();
                    return index < subKeyNames.Length ? subKeyNames[index] : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static List<string> GetUserRegistryKeys()
        {
            using (var usersKey = Microsoft.Win32.Registry.Users)
            {
                return new List<string>(usersKey.GetSubKeyNames());
            }
        }

        public static string ExtractUser(string fix)
        {
            // Use regular expression to extract user from the FIX string
            var regex = new System.Text.RegularExpressions.Regex(@"(?i)HKU\\(.+?)\\.+");
            var match = regex.Match(fix);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
        // Method to extract the user SID from the FIX string (mimicking StringRegExpReplace)
        private string ExtractUserFromFix(string fix)
        {
            string pattern = @"(?i)HKU\\(.+?)\\.+";
            var match = System.Text.RegularExpressions.Regex.Match(fix, pattern);
            return match.Groups[1].Value;
        }

        public static string[] GetAllUsers()
        {
            // Define the registry key for user profiles
            const string profileListKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

            // Open the registry key
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(profileListKey))
            {
                if (key == null)
                {
                    throw new InvalidOperationException("Unable to open ProfileList registry key.");
                }

                // Retrieve all subkey names (user SIDs)
                return key.GetSubKeyNames();
            }
        }

    }
}

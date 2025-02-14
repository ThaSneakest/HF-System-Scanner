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

        public static IEnumerable<string> GetUserRegistryKeys()
        {
            // Replace this with logic to enumerate user registry keys (HKU)
            return new List<string> { "S-1-5-21-1234567890-1234567890-1234567890-1001" };
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

    }
}

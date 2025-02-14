using System;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public class AccountHelper
{
    public static string LookupAccount(string acc)
    {
        if (Regex.IsMatch(acc, @"S-1-\d+-\d+"))  // Check if it's a SID
        {
            string accountName = SecurityLookupAccountSid(acc);
            if (accountName != null && accountName.Length > 0)
                return $"{accountName[1]}\\{accountName[0]}";
        }
        else
        {
            switch (acc)
            {
                case "BA": return "BUILTIN\\Administrators";
                case "BU": return "BUILTIN\\Users";
                case "SU": return "NT AUTHORITY\\SERVICE";
                case "WD": return "EVERYONE";
                // Add other cases as needed
                default: return acc;
            }
        }

        return acc;  // If no match, return original
    }

    public static string SecurityLookupAccountSid(string sid)
    {
        try
        {
            SecurityIdentifier securityIdentifier = new SecurityIdentifier(sid);
            NTAccount account = (NTAccount)securityIdentifier.Translate(typeof(NTAccount));
            return account.Value;
        }
        catch
        {
            return null;
        }
    }

    public static string GetProfilesDirectory()
    {
        uint size = 4096;
        StringBuilder profileDir = new StringBuilder((int)size);

        if (!UserenvNativeMethods.GetProfilesDirectoryW(profileDir, ref size))
        {
            throw new InvalidOperationException("Failed to get profiles directory.");
        }

        return profileDir.ToString();
    }
}

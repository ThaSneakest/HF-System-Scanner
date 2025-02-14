using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WhiteListDll
{
    private static readonly string[] WhitelistedDlls =
    {
        "advapi32", "clbcatq", "combase", "coml2", "comdlg32", "DifxApi", "gdi32", "gdiplus",
        "IERTUTIL", "imagehlp", "IMM32", "kernel32", "LPK", "lz32", "MSCTF", "MSVCRT",
        "NORMALIZ", "NSI", "ole32", "oleaut32", "olecli32", "olecnv32", "olesvr32",
        "olethk32", "PSAPI", "rpcrt4", "sechost", "Setupapi", "SHCORE", "shell32",
        "SHLWAPI", "url", "urlmon", "user32", "USP10", "version", "wininet", "wldap32", "WS2_32"
    };

    public static bool IsWhitelisted(string dllName)
    {
        if (string.IsNullOrWhiteSpace(dllName))
            return false;

        // Regular expression pattern for matching the DLL name
        string pattern = @"(?i)\[.+\(Microsoft Corporation\) .+\\Windows\\System32\\("
                         + string.Join("|", WhitelistedDlls) + @")\.dll";

        return Regex.IsMatch(dllName, pattern);
    }

    public static List<string> FilterDlls(IEnumerable<string> dllList)
    {
        List<string> filteredDlls = new List<string>();

        foreach (var dll in dllList)
        {
            if (IsWhitelisted(dll))
            {
                filteredDlls.Add(dll);
            }
        }

        return filteredDlls;
    }
}

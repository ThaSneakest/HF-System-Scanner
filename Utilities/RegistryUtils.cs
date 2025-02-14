using DevExpress.Utils;
using DevExpress.Utils.Drawing.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class RegistryUtils
{
    private static List<string> registryEntries = new List<string>();
    public const bool REBOOTED = false; // Example, adjust as per your logic
    public List<string> ARRAYREG { get; set; } = new List<string>();  // To store results
                                                                      // Placeholder for the Load list which stores loaded registry keys
    public static List<string> Load = new List<string>();
    private static List<string> arrReg = new List<string>();
    public static string valueName = "Default";
    private const int NERR_Success = 0;

    public static string HKEYTRANS(string key)
    {
        // Extract the root part of the key
        string hRoot = Regex.Replace(key, @"(.+?)\..+", "$1");

        // Call the _ROOT function (assumed to be another function returning a string)
        string root = ROOT(hRoot);

        // Escape backslashes for C# string literals
        root = root.Replace("\\", "\\\\");

        // Replace the root in the full key
        string fullKey = Regex.Replace(key, @"^(.+?)\\", root + "\\");

        // Assuming _STRTOUN is a method that converts the string in some way
        fullKey = STRTOUN(fullKey);

        return fullKey;
    }

    public static string ROOT(string rKey)
    {
        string hRoot = string.Empty;
        string userSid = string.Empty;

        switch (rKey.ToUpper())
        {
            case "HKEY_LOCAL_MACHINE":
            case "HKLM":
                hRoot = @"\registry\machine";
                break;

            case "HKEY_USERS":
            case "HKU":
                hRoot = @"\registry\user";
                break;

            case "HKEY_CURRENT_USER":
            case "HKCU":
                // Assuming _SECURITY__LOOKUPACCOUNTNAME is a function to get the user's SID
                string[] sid = SECURITY_LOOKUPACCOUNTNAME(Environment.MachineName + "\\" + Environment.UserName);
                if (sid != null && sid.Length > 0)
                {
                    userSid = sid[0];
                }
                hRoot = @"\registry\user\" + userSid;
                break;

            case "HKEY_CLASSES_ROOT":
            case "HKCR":
                hRoot = @"\registry\machine\software\classes";
                break;

            default:
                hRoot = string.Empty;
                break;
        }

        return hRoot;
    }
    private static string[] SECURITY_LOOKUPACCOUNTNAME(string accountName)
    {
        try
        {
            var user = new System.Security.Principal.NTAccount(accountName);
            var sid = (SecurityIdentifier)user.Translate(typeof(SecurityIdentifier));
            return new string[] { sid.ToString() };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resolving SID for {accountName}: {ex.Message}");
            return null;
        }
    }

    public static string StartKey(string key)
    {
        string hRoot = Regex.Replace(key, @"(.+?)\..+", "$1");
        hRoot = ROOT(hRoot);

        string startKey = string.Empty;
        if (Regex.IsMatch(key, @"\\.+\\"))
        {
            string subKey = Regex.Replace(key, @".+?\\(.+)\..+", "$1");
            startKey = hRoot + "\\" + subKey;
        }
        else
        {
            startKey = hRoot;
        }

        return STRTOUN(startKey);
    }

    public static string STRTOUN(string str)
    {
        StringBuilder retStr = new StringBuilder();
        int length = str.Length;

        for (int i = 0; i < length; i++)
        {
            // Get the character at position i (1-based index in AutoIt)
            char c = str[i];
            string hexChar = ((int)c).ToString("X4");  // Convert character to hex (4 digits)

            // Swap the byte pairs in the hex string
            hexChar = SwapHexBytePairs(hexChar);

            // Append the transformed hex string to the result
            retStr.Append(hexChar);
        }

        return retStr.ToString();
    }

    // Helper method to swap byte pairs in a hexadecimal string
    private static string SwapHexBytePairs(string hex)
    {
        if (hex.Length == 4)
        {
            // Swap the first and second byte pair
            return hex.Substring(2, 2) + hex.Substring(0, 2);
        }

        return hex;
    }


    public static void RegClose(object hkey)
    {
        // Simulate closing registry key
        ((RegistryKey)hkey).Close();
    }



    public static string RegReadString(string key)
    {
        // Example registry read logic here
        return string.Empty; // Replace with actual registry access
    }


    // Get registry handle
    public static int GetRegistryHandle(string fullKey, int access = 131097, int option = 1)
    {
        // Simulate registry handle retrieval (actual implementation depends on your needs)
        uint result = 3221225506; // Placeholder for actual handle retrieval logic
        return (int)result; // Explicit cast from uint to int
    }

    public static string TransformRegistryKey(string key)
    {
        return key.Replace("HKEY_LOCAL_MACHINE", "HKLM");
    }
    public void UDEBUG(string key)
    {
        RegistryKey hkey = Registry.LocalMachine.OpenSubKey(key, writable: true);
        if (hkey == null)
        {
            return;
        }

        int i = -1;
        while (true)
        {
            i++;
            string subKey = RegistrySubKeyHandler.GetRegistrySubKey(hkey, i);
            if (subKey == null)
                break;

            string sKey = $"{key}\\{subKey}\\DebugInformation";

            if (RegistryKeyHandler.RegistryKeyExists(sKey))
            {
                RegistryKey shkey = Registry.LocalMachine.OpenSubKey(sKey, writable: true);
                if (shkey == null) continue;

                int ii = -1;
                while (true)
                {
                    ii++;
                    string sSub = RegistrySubKeyHandler.GetRegistrySubKey(shkey, ii);
                    if (sSub == null)
                        break;
                    string keyPath = shkey.Name; // Gets the full registry key path
                    string data = RegistryValueHandler.ReadRegistryValue(keyPath, $"{sSub}\\DebugPath");
                    if (string.IsNullOrEmpty(data)) continue;

                    string file = data;
                    string company = string.Empty;

                    if (FileUtils.FileExists(file))
                    {
                        string cdate = DateTime.Now.ToString("yyyy-MM-dd");
                        data = $"{file} [{cdate}]";
                    }

                    // Add the entry to the registry array
                    ArrayUtils.AddToRegistryArray($"{key}\\{subKey}: -> {data}{company}");
                }
            }
        }

        hkey?.Close();
    }

    public static void UDEBUG0(string key)
    {
        using (RegistryKey hkey = Registry.LocalMachine.OpenSubKey(key, writable: true))
        {
            if (hkey == null)
            {
                return;
            }

            int i = 0;
            while (true)
            {
                string subKey = RegistrySubKeyHandler.GetRegistrySubKey(hkey, i);
                if (subKey == null)
                    break;
                // Get the full registry key path from the RegistryKey object (hkey)
                string keyPath = hkey.Name; // hkey.Name gives the registry path as a string
                string data = RegistryValueHandler.ReadRegistryValue(keyPath, subKey);
                if (!string.IsNullOrEmpty(data))
                {
                    string file = data;
                    string cdate = string.Empty;

                    if (File.Exists(file))
                    {
                        cdate = " [" + DateTime.Now.ToString("yyyy-MM-dd") + "]";
                        data = file;
                    }

                    // Add the entry to the registry array (this is a placeholder for actual storage)
                    ArrayUtils.AddToRegistryArray($"{key}\\{subKey}: -> {data}{cdate}");
                }

                i++;
            }
        }
    }

   

    // Method to retrieve User SID
    public static string GetUserSID(string username)
    {
        try
        {
            var user = new System.Security.Principal.NTAccount(username);
            var sid = (System.Security.Principal.SecurityIdentifier)user.Translate(typeof(System.Security.Principal.SecurityIdentifier));
            return sid.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving SID for {username}: {ex.Message}");
            return string.Empty;
        }
    }


    public static bool SetRegistryAccessAlt(string key, bool grantAccess)
    {
        try
        {
            // Perform the logic to set registry access
            Console.WriteLine($"Setting access for {key}, Grant Access: {grantAccess}");

            // Indicate success
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting registry access: {ex.Message}");

            // Indicate failure
            return false;
        }
    }


    public static void EnumSafe(string startKey)
    {
        var arrSafe = new List<string>();

        using (var hKey = RegistryKeyHandler.OpenRegistryKey(startKey))
        {
            if (hKey == null)
                return;

            // Enumerate all subkey names
            string[] subKeyNames = hKey.GetSubKeyNames();
            foreach (string keyName in subKeyNames)
            {
                string fullKeyPath = $"{startKey}\\{keyName}";

                // Open the registry subkey
                using (var subKey = RegistryKeyHandler.OpenRegistryKey(fullKeyPath))
                {
                    if (subKey != null)
                    {
                        // Get registry values as a list
                        var arrayName = RegistryValueHandler.GetRegistryValuesAsList(fullKeyPath);
                        if (arrayName != null && arrayName.Count > 0)
                        {
                            foreach (var entry in arrayName)
                            {
                                string atten = string.Empty;

                                // Check for specific patterns to add attention flag
                                if (Regex.IsMatch(fullKeyPath, @"(?i)(Minimal|Network)\\(PCProtect|pcwatch|cmwr|cmwf|MyOSProtect)"))
                                    atten = " <==== Update Required";

                                // Validate if the entry should continue processing
                                if (ShouldContinue(fullKeyPath, entry.Key, entry.Value))
                                    continue;

                                // Add formatted entry to the array
                                arrSafe.Add($"{fullKeyPath} => \"{entry.Key}\"=\"{entry.Value}\"{atten}");
                            }
                        }
                    }
                }

                // Recursively call EnumSafe for subkeys
                EnumSafe(fullKeyPath);
            }
        }
    }

    public static RegistryKey GetRegistryHive(string hive)
    {
        hive = hive.ToUpper();

        switch (hive)
        {
            case "HKEY_LOCAL_MACHINE":
                return Registry.LocalMachine;
            case "HKEY_CURRENT_USER":
                return Registry.CurrentUser;
            case "HKEY_USERS":
                return Registry.Users;
            case "HKEY_CLASSES_ROOT":
                return Registry.ClassesRoot;
            case "HKEY_CURRENT_CONFIG":
                return Registry.CurrentConfig;
            default:
                return null;
        }
    }


    public static bool ShouldContinue(string fullKeyPath, string entryName, string entryValue)
    {
        // Add all your complex match conditions here
        if (Regex.IsMatch(fullKeyPath, @"(?i)(Minimal|Network)\\(usbaudio|HdAudBus|HdAudAddService|uefi|SpbCx|iai2c|Ahcache|BasicDisplay|dmserver|SRService|System Bus Extender|Base|Boot Bus Extender)") &&
            Regex.IsMatch(entryValue, @"(?i)Service|Driver|Driver Group"))
        {
            return true;
        }


        if (Regex.IsMatch(fullKeyPath, "(?i)\\Network\\(AFD|BFE|Browser|Dhcp|DnsCache|...|termservice|WZCSVC)") &&
            Regex.IsMatch(entryValue, "(?i)Service|Driver|Driver Group"))
            return true;

        // Add other conditions as needed...

        return false;
    }

    


    public string SHELLTARGET(string key)
    {
        string data = string.Empty;

        // Define the registry key to be read
        string sKey = key;  // You can pass the key as a parameter or define it here.

        try
        {
            // Attempt to read different registry values in order
            data = RegistryValueHandler.ReadRegistryValue(sKey, "Target");
            if (data == null)
            {
                data = RegistryValueHandler.ReadRegistryValue(sKey, "TargetFolderPath");
            }
            if (data == null)
            {
                data = RegistryValueHandler.ReadRegistryValue(sKey, "TargetKnownFolder");
            }
            if (data == null)
            {
                data = RegistryValueHandler.ReadRegistryValue(sKey, "TargetSpecialFolder");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading registry key: {ex.Message}");
        }

        return data;
    }


    public static string HexToString(string hex)
    {
        if (!hex.StartsWith("0x"))
        {
            hex = "0x" + hex;
        }
        return BinaryToString(HexToBinary(hex));
    }

    public static IntPtr OpenRegistryKey(string keyPath, uint accessRights)
    {
        const int ERROR_SUCCESS = 0;
        IntPtr hKey = IntPtr.Zero;
        IntPtr rootKey = GetRootKeyFromPath(keyPath, out string subKey);

        if (rootKey == IntPtr.Zero || string.IsNullOrEmpty(subKey))
        {
            Console.WriteLine($"Invalid key path: {keyPath}");
            return IntPtr.Zero;
        }

        int result = Advapi32NativeMethods.RegOpenKeyExAlt1(rootKey, subKey, 0, accessRights, ref hKey);

        if (result != ERROR_SUCCESS)
        {
            Console.WriteLine($"Failed to open registry key '{keyPath}'. Error code: {result}");
            return IntPtr.Zero;
        }

        return hKey;
    }


    public static void CloseRegistryKey(IntPtr hKey)
    {
        if (hKey != IntPtr.Zero)
        {
            Advapi32NativeMethods.RegCloseKey(hKey);
            Console.WriteLine("Registry key handle closed successfully.");
        }
    }

    private static IntPtr GetRootKeyFromPath(string keyPath, out string subKey)
    {
        subKey = null;

        if (keyPath.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase))
        {
            subKey = keyPath.Substring(@"HKEY_LOCAL_MACHINE\".Length);
            return new IntPtr(unchecked((int)0x80000002)); // HKEY_LOCAL_MACHINE
        }
        else if (keyPath.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase))
        {
            subKey = keyPath.Substring(@"HKEY_CURRENT_USER\".Length);
            return new IntPtr(unchecked((int)0x80000001)); // HKEY_CURRENT_USER
        }
        else if (keyPath.StartsWith(@"HKEY_USERS\", StringComparison.OrdinalIgnoreCase))
        {
            subKey = keyPath.Substring(@"HKEY_USERS\".Length);
            return new IntPtr(unchecked((int)0x80000003)); // HKEY_USERS
        }
        else if (keyPath.StartsWith(@"HKEY_CLASSES_ROOT\", StringComparison.OrdinalIgnoreCase))
        {
            subKey = keyPath.Substring(@"HKEY_CLASSES_ROOT\".Length);
            return new IntPtr(unchecked((int)0x80000000)); // HKEY_CLASSES_ROOT
        }

        return IntPtr.Zero;
    }

    public static string StringToHex(string input)
    {
        return BitConverter.ToString(StringToBinary(input)).Replace("-", "");
    }

    public static object RegRead(string keyPath, string valueName, bool multiSzAsBinary = false)
    {
        IntPtr hKey = OpenRegistryKey(keyPath, 131097); // KEY_READ | KEY_WOW64_64KEY
        if (hKey == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            int type = 0;
            int dataSize = 0;

            int result = Advapi32NativeMethods.RegQueryValueEx(hKey, valueName, IntPtr.Zero, ref type, IntPtr.Zero, ref dataSize);
            if (result != 0)
            {
                throw new Exception($"RegQueryValueEx failed. Error code: {result}");
            }

            byte[] data = new byte[dataSize];
            result = Advapi32NativeMethods.RegQueryValueEx(hKey, valueName, IntPtr.Zero, ref type, data, ref dataSize);
            if (result != 0)
            {
                throw new Exception($"RegQueryValueEx failed. Error code: {result}");
            }

            return ProcessRegistryData(type, data, multiSzAsBinary);
        }
        finally
        {
            Advapi32NativeMethods.RegCloseKey(hKey);
        }
    }

    private static object ProcessRegistryData(int type, byte[] data, bool multiSzAsBinary)
    {
        switch (type)
        {
            case 1: // REG_SZ
            case 2: // REG_EXPAND_SZ
                return System.Text.Encoding.Unicode.GetString(data).TrimEnd('\0');
            case 7: // REG_MULTI_SZ
                if (multiSzAsBinary)
                {
                    return data;
                }
                else
                {
                    string multiSz = System.Text.Encoding.Unicode.GetString(data).TrimEnd('\0');
                    return multiSz.Replace("\0", Environment.NewLine);
                }
            case 3: // REG_BINARY
                return data;
            case 4: // REG_DWORD
                return BitConverter.ToUInt32(data, 0);
            case 11: // REG_QWORD
                return BitConverter.ToUInt64(data, 0);
            default:
                throw new NotSupportedException($"Registry type {type} is not supported.");
        }
    }

    private static byte[] StringToBinary(string input)
    {
        return System.Text.Encoding.Unicode.GetBytes(input);
    }

    private static string BinaryToString(byte[] data)
    {
        return System.Text.Encoding.Unicode.GetString(data);
    }

    private static byte[] HexToBinary(string hex)
    {
        hex = hex.Replace("0x", "").Replace("-", "");
        byte[] data = new byte[hex.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return data;
    }
    public static byte[] TrimBinaryData(byte[] data, byte[] pattern)
    {
        int patternLength = pattern.Length;
        int start = data.Length - patternLength;

        while (start >= 0)
        {
            bool match = true;
            for (int i = 0; i < patternLength; i++)
            {
                if (data[start + i] != pattern[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                start -= patternLength;
            }
            else
            {
                break;
            }
        }

        start = Math.Max(0, start + patternLength);
        byte[] trimmedData = new byte[start];
        Array.Copy(data, 0, trimmedData, 0, start);

        return trimmedData;
    }


    // List registry values and check for specific conditions
    public static bool ListRegistryLinks(RegistryKey hKey)
    {
        try
        {
            uint index = 0;

            while (true)
            {
                // Allocate memory for the name buffer
                StringBuilder valueName = new StringBuilder(256); // Adjust size as needed
                uint valueNameSize = (uint)valueName.Capacity;

                // Allocate memory for the data buffer
                byte[] valueData = new byte[1024]; // Adjust size as needed
                uint valueDataSize = (uint)valueData.Length;

                uint type = 0;

                // Query the value at the current index
                int ret = Advapi32NativeMethods.RegEnumValue(
                    hKey.Handle,
                    index,
                    valueName,
                    ref valueNameSize,
                    IntPtr.Zero,
                    ref type,
                    valueData,
                    ref valueDataSize);

                // If there are no more values, break the loop
                if (ret == NativeMethodConstants.ERROR_NO_MORE_ITEMS)
                {
                    break;
                }

                // Check for errors
                if (ret != NativeMethodConstants.ERROR_SUCCESS)
                {
                    Console.WriteLine($"Error enumerating registry values: {ret}");
                    break;
                }

                // If the type indicates a registry link (type 6), process it
                if (type == 6) // REG_LINK
                {
                    string keyName = valueName.ToString();
                    string target = Encoding.Unicode.GetString(valueData, 0, (int)valueDataSize).TrimEnd('\0');

                    Console.WriteLine($"RegLink Found. Source: {keyName} => Target: {target}");
                    return true;
                }

                index++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing registry links: {ex.Message}");
        }

        return false;
    }


    // Check if the registry exists
    public static bool RegistryExists(string key)
    {
        try
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
            {
                return registryKey != null;
            }
        }
        catch
        {
            return false;
        }
    }


    // Helper function to check for invalid characters
    private static bool _CHKINVAL(string name, out string invalidChars)
    {
        // Implement logic to check for invalid characters in the name
        invalidChars = null;
        return false; // Placeholder
    }


    private static bool CheckInvalidCharacters(string name, out string invalidChars)
    {
        // Check for invalid characters in the key name
        invalidChars = null;
        return false; // Placeholder
    }


    public static void RELOAD(string UNAME)
    {
        if (SystemConstants.BootMode != "Recovery") return;

        string[] ALLUSERS = RegistryUserHandler.GetAllUsers(); // Replace with actual method for fetching users
        string matchedUserPath = null;

        // Find the matching user path
        foreach (string userPath in ALLUSERS)
        {
            if (Regex.IsMatch(userPath, @"\\" + UNAME + @"\Z"))
            {
                matchedUserPath = userPath;
                break;
            }
        }

        // If no matching user path was found, exit
        if (matchedUserPath == null)
        {
            Console.WriteLine("No matching user path found.");
            return;
        }

        // Generate and execute the command
        string command = $@"reg load ""hku\{UNAME}"" ""{matchedUserPath}\ntuser.dat""";
        Console.WriteLine(command);

        try
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute command: {ex.Message}");
        }
    }

    public static void REUNLOAD(string uname, string bootMode)
    {
        if (bootMode != "Recovery")
            return;

        string command = $"reg unload \"hku\\{uname}\"";
        CommandHandler.RunCommand(command);
    }

    public static void REUNLOAD(string uname)
    {
        // Check if the boot mode is not "Recovery"
        string bootMode = "Recovery"; // Replace with the actual boot mode value
        if (bootMode != "Recovery") return;

        // Prepare the command to unload the registry key
        string command = $"reg unload \"hku\\{uname}\"";

        // Execute the command using Command Prompt
        CommandHandler.RunCommand(command);
    }



    private static string _SECURITY__LOOKUPACCOUNTSID(string user)
    {
        // Simulating account lookup for SID
        try
        {
            var securityIdentifier = new SecurityIdentifier(user);
            var account = securityIdentifier.Translate(typeof(NTAccount));
            return account.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error looking up account: {ex.Message}");
            return null;
        }
    }



    public static bool VAR(string key, string ch = "")
    {
        if (string.IsNullOrEmpty(key))
            return false;

        IntPtr hKey = new IntPtr(2147483650); // Default key handle
        if (!string.IsNullOrEmpty(ch))
        {
            hKey = (IntPtr)RegistryKeyHandler.GetRootKey(key); // Explicit cast
        }


        if (hKey == IntPtr.Zero)
            return false;

        // Removing the root part from the key (before the first backslash)
        string subKey = key.Substring(key.IndexOf("\\") + 1);

        IntPtr keyHandle = IntPtr.Zero; // Initialize keyHandle
        int result = (int)Advapi32NativeMethods.RegOpenKeyEx(hKey, subKey, 0, NativeMethodConstants.KEY_READ, ref keyHandle);



        if (result != 0)
            return result == NativeMethodConstants.ERROR_ACCESS_DENIED;

        // Close the handle if successful
       Kernel32NativeMethods.CloseHandle(keyHandle);

        return result == 0 || result == NativeMethodConstants.ERROR_ACCESS_DENIED;
    }

   

    
    // Method to check invalid characters and process accordingly
    public static Tuple<bool, string> CheckInvalidCharacters(string str)
    {
        bool chk = false;
        string retStr = string.Empty;

        str = str.Replace("0x", "");
        var arr = Regex.Matches(str, @"\w{4}")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();

        foreach (var item in arr)
        {
            string charStr = Regex.Replace(item, @"(\w{2})(\w{2})", "$2$1");
            charStr = charStr.Replace("00E0", "0000");

            if (Convert.ToInt32(charStr, 16) < 32)
            {
                charStr = "002A";
                chk = true;
            }

            retStr += (char)Convert.ToInt32(charStr, 16);
        }

        return Tuple.Create(chk, retStr);
    }

    // Process CLSID entries for Chrome and other related keys
    public static void ProcessClsid()
    {
        int k = 0;
        while (true)
        {
            // Use unchecked for overflow if needed
            uint userC = unchecked((uint)2147483651); // Using uint to avoid overflow
            if (string.IsNullOrEmpty(userC.ToString())) break;

            if (userC.ToString().Contains("_Classes"))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(userC.ToString()))
                {
                    if (key != null)
                    {
                        string[] subKeyNames = key.GetSubKeyNames();
                        foreach (string clsid in subKeyNames)
                        {
                            if (clsid == "CLSID")
                            {
                                ProcessClsid2(userC.ToString());
                            }
                            if (clsid == "ChromeHTML")
                            {
                                ChromeHandler.ProcessClsidChr(userC.ToString());
                            }
                        }
                    }
                }
            }

            k++;
        }
    }


    // Handle CLSID key processing
    public static void ProcessClsid2(string userC)
    {
        string keyPath = @"HKU\" + userC + @"\CLSID";

        using (RegistryKey baseKey = Registry.Users.OpenSubKey(userC))
        {
            if (baseKey != null)
            {
                using (RegistryKey hkey = baseKey.OpenSubKey("CLSID"))
                {
                    if (hkey != null)
                    {
                        foreach (string clsid in hkey.GetSubKeyNames())
                        {
                            string commandKeyPath = $@"CLSID\{clsid}\Shell\Open\Command";
                            using (RegistryKey commandKey = baseKey.OpenSubKey(commandKeyPath))
                            {
                                if (commandKey != null)
                                {
                                    // Retrieve the default value of the registry key
                                    string vdata = commandKey.GetValue("")?.ToString();
                                    if (!string.IsNullOrEmpty(vdata))
                                    {
                                        ProcessClsidFile(vdata);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public static string GetRegistryValue(string key, string valueName)
    {
        using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
        {
            if (registryKey == null) return null;
            return registryKey.GetValue(valueName)?.ToString();
        }
    }


    // Handle specific CLSID file processing
    public static void ProcessClsidFile(string file)
    {
        if (File.Exists(file))
        {
            // Process file based on path or conditions
        }
        else
        {
            // Handle missing file
        }
    }


    public static List<string> ReadIniSectionNames(string iniFile, bool readIni = false)
    {
        string readData = iniFile;
        if (!readIni)
        {
            using (StreamReader reader = new StreamReader(iniFile))
            {
                readData = reader.ReadToEnd();
            }
        }

        var sections = new List<string>();
        var matches = Regex.Matches(readData, @"\[([^\[]+?)\]");

        foreach (Match match in matches)
        {
            sections.Add(match.Groups[1].Value);
        }

        return sections;
    }

    public static string ReadIniSectionValue(string iniFile, string sectionName, string value, bool readIni = false)
    {
        string readData = iniFile;
        if (!readIni)
        {
            using (StreamReader reader = new StreamReader(iniFile))
            {
                readData = reader.ReadToEnd();
            }
        }

        var sections = ReadIniSectionNames(readData, true);

        foreach (var section in sections)
        {
            if (section != sectionName) continue;

            var sectionValues = Regex.Matches(readData, $@"\[{section}\]\s*([^\[]+)");
            foreach (Match secValue in sectionValues)
            {
                var valMatch = Regex.Match(secValue.Groups[1].Value, $@"{value}\s*=\s*(.+)");
                if (valMatch.Success)
                {
                    return valMatch.Groups[1].Value;
                }
            }
        }

        return null;
    }


    public static bool CheckKeyLocked(string key)
    {
        try
        {
            // Attempt to open the registry key with read-only access
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
            {
                if (registryKey != null)
                {
                    return false; // Key is not locked, we could access it
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // If we catch UnauthorizedAccessException, the key is locked
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking key permissions: {ex.Message}");
            return true; // If other errors occur, assume the key is locked
        }

        return false;
    }




    // Placeholder method to simulate getting subkey names from the registry key (mimicking __REGENUMKEY behavior)
    private string GetSubKeyName(RegistryKey key, int index)
    {
        try
        {
            return key.GetSubKeyNames()[index];
        }
        catch (IndexOutOfRangeException)
        {
            return null; // No more subkeys
        }
    }

    // Placeholder methods for registry and file operations (not implemented here)
    private void RegClose(RegistryKey hKey)
    {
        hKey?.Close();
    }

   

    // Placeholder method for registry reading (RegRead in AutoIt)
    private string RegRead(string key, string valueName)
    {
        using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(key))
        {
            if (regKey != null)
            {
                var value = regKey.GetValue(valueName);
                return value?.ToString() ?? string.Empty;
            }
        }
        return string.Empty;
    }

    

    // Method to read a registry value (mimics RegRead in AutoIt)
    private string RegRead1(string key)
    {
        // Attempt to read from the registry
        using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(key))
        {
            if (regKey != null)
            {
                // Read the value (empty string for the root key)
                return regKey.GetValue(string.Empty)?.ToString();
            }
        }
        return null; // Return null if not found or error occurs
    }

    public void ScanRegistry(string registryKeyPath)
    {
        //Logger.Instance.WriteToLog($"==================== {scanTaskLabel} ({isWhiteListed}) =================");
        //Logger.Instance.WriteToLog($"(Starting scan in registry path: {registryKeyPath})");

        RegistryKey registryKey = RegistryKeyHandler.OpenRegistryKey(registryKeyPath);
        if (registryKey == null)
        {
          //  Logger.Instance.WriteToLog($"Failed to open registry key: {registryKeyPath}");
            return;
        }

        List<string> taskLog = new List<string>();

        foreach (string subKeyName in registryKey.GetSubKeyNames())
        {
            string subKeyPath = $"{registryKeyPath}\\{subKeyName}";
            string command = RegistryValueHandler.ReadRegistryValue(subKeyPath, "");
            if (string.IsNullOrEmpty(command))
            {
                //Logger.Instance.WriteToLog($"Task: {subKeyName} - No Access");
                continue;
            }

            string path = RegistryValueHandler.ReadRegistryValue(subKeyPath, "Path");
            if (!string.IsNullOrEmpty(path))
            {
                string taskName = Path.GetFileName(path);
                string taskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "Tasks", path);

                if (File.Exists(taskPath))
                {
                    string taskContent = File.ReadAllText(taskPath);
                    ProcessTaskContent(taskLog[0], subKeyName, taskPath, taskContent);

                }
                else
                {
                //    Logger.Instance.WriteToLog($"Task: {subKeyName} - {path} -> Task file not found.");
                }
            }
        }

        taskLog.Sort();
        foreach (string logEntry in taskLog)
        {
          //  Logger.Instance.WriteToLog(logEntry);
        }

        registryKey.Close();
    }

    public static void ProcessTaskContent(string taskLog, string subKeyName, string taskPath, string taskContent)
    {
        // Example logic: Log or analyze task content
        Console.WriteLine($"Processing task: {subKeyName}");
        Console.WriteLine($"Task path: {taskPath}");
        Console.WriteLine($"Task content: {taskContent}");
    }

    /*/
    public void SEARCHREGBUTT()
    {
        // Get the search term from the TextBox
        string search = editTextBox.Text.Trim();

        // Perform regular expression replacements on the search string
        search = Regex.Replace(search, @"\s+$", "");
        search = Regex.Replace(search, @"(^\s+|\s+$)", "");
        if (string.IsNullOrEmpty(search))
        {
            MessageBox.Show("Please enter a valid search term.");
            return;
        }

        // Update UI controls
        label1.Text = $"Searching: {search}...";
        buttonSearchReg.Text = "Searching...";
        buttonSearchReg.Enabled = false;

        // Simulate progress bar for searching
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.Visible = true;

        // Simulate file creation
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SearchReg.txt");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            string currentDate = DateTime.Now.ToString();
            writer.WriteLine($"Farbar Recovery Scan Tool {VERSION}");
            writer.WriteLine($"{SCAN11} {Environment.UserName} ({currentDate})");
            writer.WriteLine($"{SCAN13} {AppDomain.CurrentDomain.BaseDirectory}");
            writer.WriteLine($"{SCAN15}: {BOOTM}");
            writer.WriteLine("================== {SEARCHREG}: \"{search}\" =============");

            // Perform the search
            string regRead = SEARCHREGE(search);
            if (search.Contains(";"))
            {
                string[] arr = search.Split(';');
                foreach (var item in arr)
                {
                    writer.WriteLine($"===================== Searching for \"{item}\" ============");
                    string regSearchResult = Regex.Match(regRead, $@"(?is)(\[hk(?:.(?!\[hk))+?{item}.+?)(?:\[\[\[\[)").Value;
                    if (!string.IsNullOrEmpty(regSearchResult))
                    {
                        writer.WriteLine(regSearchResult);
                    }
                }
            }
            else
            {
                writer.WriteLine(regRead);
            }

            writer.WriteLine($"====== {END} {OF} {SEARCHB} ======");
        }

        // Simulate the completion of the search
        progressBar.Visible = false;
        label1.Text = $"{SEARCHB} {DONE}.";
        buttonSearchReg.Text = SEARCHREG;
        MessageBox.Show($"{SEARCHB} {DONE}. \"SearchReg.txt\" {COMPLETED}");

        // Open the result file in Notepad
        System.Diagnostics.Process.Start("notepad.exe", filePath);

        // Reset UI
        label1.Text = "";
        buttonSearchReg.Enabled = true;
    }

    public string SEARCHREGE(string searchVal)
    {
        // If the boot mode is "recovery", return immediately
        if (SystemConstants.BootMode == "recovery")
        {
            return string.Empty;
        }

        // Initialize an empty list for storing registry keys
        List<string> arrReg = new List<string>();

        // Split the search value by ';' into an array
        string[] arrSearch = searchVal.Split(';');

        // Perform registry search for HKEY_LOCAL_MACHINE and HKEY_USERS
        SEARCHREGK("HKEY_LOCAL_MACHINE", arrSearch);
        SEARCHREGK("HKEY_USERS", arrSearch);

        // Open the file to store search results
        string searchFilePath = Path.Combine($@"{FolderConstants.HomeDrive}, frst\Search");
        using (StreamWriter writer = new StreamWriter(searchFilePath, append: true))
        {
            // If there are any results in arrReg, write them to the file
            if (arrReg.Count > 1)
            {
                arrReg = arrReg.Distinct().ToList();  // Remove duplicates
                foreach (var regKey in arrReg)
                {
                    writer.WriteLine(regKey);  // Write each registry key to the file
                }
            }
        }

        // Read the contents of the search file
        string regRead;
        using (StreamReader reader = new StreamReader(searchFilePath))
        {
            regRead = reader.ReadToEnd();
        }

        // Delete the search file after reading
        File.Delete(searchFilePath);

        return regRead;
    }

    /*/


    public static void SEARCHREGK(string startKey, string[] arr)
    {

        // Open the registry key
        RegistryKey hKey = Registry.LocalMachine.OpenSubKey(startKey);
        if (hKey == null) return;

        // Initialize the index
        int i = 0;

        // Loop through all subkeys
        while (true)
        {
            // Enumerate the subkeys
string key = hKey.GetSubKeyNames()[i];

            if (key == null) break;

            // Skip unwanted keys using regex
            if (Regex.IsMatch(startKey + "\\" + key, @"(?i)^HKEY_LOCAL_MACHINE\\.+\\ControlSet\d+|^HKEY_USERS\\([^\\]+_Classes|S-1-5-18)"))
            {
                i++;
                continue;
            }

            // Loop through the search array
            foreach (var searchTerm in arr)
            {
                if (key.Contains(searchTerm))
                {
                    arrReg.Add("[" + startKey + "\\" + key + "]" + "||||");
                }
            }

            // Search the values within the current key
            int z = 0;
            while (true)
            {
                string value = "";
                    //RegistryValueHandler.EnumerateRegistryValue(hKey, index);

                if (value == null) break;

                foreach (var searchTerm in arr)
                {
                    if (value.Contains(searchTerm))
                    {
                        string readVal = RegistryValueHandler.TryReadRegistryValue(hKey, key, value);
                        arrReg.Add("[" + startKey + "\\" + key + "]" + Environment.NewLine + "\"" + value + "\"=\"" + readVal + "\"" + "||||");
                    }
                    else
                    {
                        string readVal = RegistryValueHandler.TryReadRegistryValue(hKey, key, value);
                        if (!string.IsNullOrEmpty(readVal) && readVal.Contains(searchTerm))
                        {
                            arrReg.Add("[" + startKey + "\\" + key + "]" + Environment.NewLine + "\"" + value + "\"=\"" + readVal + "\"" + "||||");
                        }
                    }
                }
                z++;
            }

            // Recursively search subkeys
            SEARCHREGK(startKey + "\\" + key, arr);

            i++;
        }

        // Close the registry key
        hKey.Close();
    }

    public static string ReadRegistryValue(IntPtr hKey, string keyPath, string valueName)
    {
        using (RegistryKey key = RegistryKey.FromHandle(new Microsoft.Win32.SafeHandles.SafeRegistryHandle(hKey, ownsHandle: false)))
        {
            using (RegistryKey subKey = key.OpenSubKey(keyPath))
            {
                return subKey?.GetValue(valueName)?.ToString();
            }
        }
    }


    // Method to retrieve all results
    public List<string> GetResults()
    {
        return arrReg;
    }

    public static string SearchRegistry(string searchValue)
    {
        var result = string.Empty;

        // Search the registry's common root keys
        var rootKeys = new[] { Registry.CurrentUser, Registry.LocalMachine };

        foreach (var rootKey in rootKeys)
        {
            result += SearchRegistryKey(rootKey, searchValue);
        }

        return result;
    }

    private static string SearchRegistryKey(RegistryKey rootKey, string searchValue)
    {
        string result = string.Empty;

        try
        {
            foreach (var subKeyName in rootKey.GetSubKeyNames())
            {
                using (var subKey = rootKey.OpenSubKey(subKeyName))
                {
                    if (subKey == null) continue;

                    // Search for values matching the search term
                    foreach (var valueName in subKey.GetValueNames())
                    {
                        var value = subKey.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(value) && value.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            result += $"[HK{rootKey.Name}\\{subKeyName}] {valueName} = {value}{Environment.NewLine}";
                        }
                    }

                    // Recursively search subkeys
                    result += SearchRegistryKey(subKey, searchValue);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Handle access denied errors
            result += $"Access denied to: {rootKey.Name}{Environment.NewLine}";
        }

        return result;
    }

    // Placeholder method to check if a registry key exists (simulates VAR)
    private bool VAR(string key)
    {
        using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key))
        {
            return registryKey != null;
        }
    }

    public static void ScanReg(string userKey)
    {
        // Scan HKU\...\Run and RunOnce
        HandleRunKeys(userKey, "Run");
        HandleRunKeys(userKey, "RunOnce");

        // Scan HKU\...\Policies\Explorer\Run
        string policiesKey = $@"{userKey}\Software\Microsoft\Windows\CurrentVersion\Policies";
        string explorerRunKey = $@"{policiesKey}\Explorer\Run";
        Console.WriteLine($"Scanning Registry: HKU\\{explorerRunKey}");

        using (var explorerRunHandle = Registry.Users.OpenSubKey(explorerRunKey))
        {
            if (explorerRunHandle != null)
            {
                foreach (var valueName in explorerRunHandle.GetValueNames())
                {
                    string valueData = explorerRunHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "Explorer\\Run", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{explorerRunKey} not found.");
            }
        }

        // Scan HKU\...\Policies\System
        string systemKey = $@"{policiesKey}\System";
        Console.WriteLine($"Scanning Registry: HKU\\{systemKey}");

        using (var systemHandle = Registry.Users.OpenSubKey(systemKey))
        {
            if (systemHandle != null)
            {
                foreach (var valueName in systemHandle.GetValueNames())
                {
                    string valueData = systemHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "System", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{systemKey} not found.");
            }
        }

        // Handle additional keys
        HandleExplorerKeys(userKey);
        HandleWinlogonKeys(userKey);
        HandleCommandProcessor(userKey);
        HandleEnvironment(userKey);
    }

    public static void HandleRunKeys(string userKey, string runKeyName)
    {
        string runKeyPath = $@"{userKey}\Software\Microsoft\Windows\CurrentVersion\{runKeyName}";
        Console.WriteLine($"Scanning Registry: HKU\\{runKeyPath}");

        using (var runKeyHandle = Registry.Users.OpenSubKey(runKeyPath))
        {
            if (runKeyHandle != null)
            {
                foreach (var valueName in runKeyHandle.GetValueNames())
                {
                    string valueData = runKeyHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, runKeyName, valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{runKeyPath} not found.");
            }
        }
    }

    public static void HandleExplorerKeys(string userKey)
    {
        string explorerKey = $@"{userKey}\Software\Microsoft\Windows\CurrentVersion\Explorer";
        Console.WriteLine($"Scanning Registry: HKU\\{explorerKey}");

        using (var explorerKeyHandle = Registry.Users.OpenSubKey(explorerKey))
        {
            if (explorerKeyHandle != null)
            {
                foreach (var valueName in explorerKeyHandle.GetValueNames())
                {
                    string valueData = explorerKeyHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "Explorer", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{explorerKey} not found.");
            }
        }
    }

    public static void HandleWinlogonKeys(string userKey)
    {
        string winlogonKey = $@"{userKey}\Software\Microsoft\Windows NT\CurrentVersion\Winlogon";
        Console.WriteLine($"Scanning Registry: HKU\\{winlogonKey}");

        using (var winlogonHandle = Registry.Users.OpenSubKey(winlogonKey))
        {
            if (winlogonHandle != null)
            {
                foreach (var valueName in winlogonHandle.GetValueNames())
                {
                    string valueData = winlogonHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "Winlogon", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{winlogonKey} not found.");
            }
        }
    }

    public static void HandleCommandProcessor(string userKey)
    {
        string commandProcessorKey = $@"{userKey}\Software\Microsoft\Command Processor";
        Console.WriteLine($"Scanning Registry: HKU\\{commandProcessorKey}");

        using (var commandProcessorHandle = Registry.Users.OpenSubKey(commandProcessorKey))
        {
            if (commandProcessorHandle != null)
            {
                foreach (var valueName in commandProcessorHandle.GetValueNames())
                {
                    string valueData = commandProcessorHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "Command Processor", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{commandProcessorKey} not found.");
            }
        }
    }

    public static void HandleEnvironment(string userKey)
    {
        string environmentKey = $@"{userKey}\Environment";
        Console.WriteLine($"Scanning Registry: HKU\\{environmentKey}");

        using (var environmentHandle = Registry.Users.OpenSubKey(environmentKey))
        {
            if (environmentHandle != null)
            {
                foreach (var valueName in environmentHandle.GetValueNames())
                {
                    string valueData = environmentHandle.GetValue(valueName)?.ToString();
                    RegistryValueHandler.HandleRegistryValue(userKey, "Environment", valueName, valueData);
                }
            }
            else
            {
                Console.WriteLine($"Key HKU\\{environmentKey} not found.");
            }
        }
    }

    public static void AddToRegistryEntries(string entry)
    {
        registryEntries.Add(entry);
        Console.WriteLine(entry);
    }


    public static string RegRead(string key, ref List<string> arr, string prefix)
    {
        try
        {
            var regKey = RegistryKeyHandler.OpenRegistryKey(key);
            if (regKey != null)
            {
                var value = regKey.GetValue(valueName);
                return value != null ? value.ToString() : string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
        return string.Empty;
    }


    public static IntPtr _HKEY(string fullKey, uint access = NativeMethodConstants.KEY_ALL_ACCESS, int y = 1)
    {
        Structs.UNICODE_STRINGALT szName = new Structs.UNICODE_STRINGALT
        {
            Length = (ushort)(fullKey.Length * sizeof(char)),
            MaximumLength = (ushort)((fullKey.Length + 1) * sizeof(char)),
            Buffer = Marshal.StringToHGlobalUni(fullKey)
        };

        Structs.OBJECT_ATTRIBUTESALT1 objectAttributes = new Structs.OBJECT_ATTRIBUTESALT1
        {
            Length = Marshal.SizeOf(typeof(Structs.OBJECT_ATTRIBUTESALT1)),
            ObjectName = Marshal.AllocHGlobal(Marshal.SizeOf(szName)),
            Attributes = 64 | 256,
            SecurityDescriptor = IntPtr.Zero,
            SecurityQualityOfService = IntPtr.Zero
        };

        Marshal.StructureToPtr(szName, objectAttributes.ObjectName, false);

        IntPtr hKey;
        int ret = NtdllNativeMethods.NtOpenKeyALT1(out hKey, (int)access, objectAttributes);


        Marshal.FreeHGlobal(szName.Buffer);
        Marshal.FreeHGlobal(objectAttributes.ObjectName);

        if (ret == 0) // STATUS_SUCCESS
        {
            return hKey;
        }

        if (y == 0)
        {
            return IntPtr.Zero;
        }

        if (ret == 3221225506) // Specific error code
        {
            IntPtr unlockedKey = IntPtr.Zero; // Placeholder for unlocking logic
            if (unlockedKey != IntPtr.Zero)
            {
                // Placeholder for _UNLOCKALLREG
            }

            ret = NtdllNativeMethods.NtOpenKeyALT1(out hKey, (int)access, objectAttributes);
            if (ret == 0) // STATUS_SUCCESS
            {
                return hKey;
            }
        }

        return IntPtr.Zero;
    }







    public static void INVALSUBKEYS(string startKey, int mode)
    {
        // Example implementation
        // Perform operations on the registry or subkeys based on startKey and mode
        Console.WriteLine($"Invalidating subkeys for {startKey} with mode {mode}");
    }



    public static List<string[]> NetUserEnum()
    {
        IntPtr bufferPtr = IntPtr.Zero;
        uint entriesRead = 0;
        uint totalEntries = 0;
        uint resumeHandle = 0;

        List<string[]> userAccounts = new List<string[]>();

        // Call NetUserEnum to enumerate users
        int result = Netapi32NativeMethods.NetUserEnum(null, 1, 0, ref bufferPtr, 0, ref entriesRead, ref totalEntries, ref resumeHandle);

        if (result == NERR_Success)
        {
            // Cast the pointer to an array of USER_INFO_1
            IntPtr p = bufferPtr;
            for (int i = 0; i < entriesRead; i++)
            {
                Structs.USER_INFO_1 userInfo = (Structs.USER_INFO_1)Marshal.PtrToStructure(p, typeof(Structs.USER_INFO_1));
                p = (IntPtr)((long)p + Marshal.SizeOf(typeof(Structs.USER_INFO_1)));

                // Add user information to the list
                userAccounts.Add(new string[] { userInfo.usri1_name, userInfo.usri1_home_dir });
            }
        }

        // Free the buffer allocated by the API
        if (bufferPtr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(bufferPtr);
        }

        return userAccounts;
    }




    public static string GetSecDes(string key, int securityInfo)
    {
        IntPtr hKey = IntPtr.Zero;

        // Open the registry key
        uint result = Advapi32NativeMethods.RegOpenKeyEx(NativeMethodConstants.HKEY_LOCAL_MACHINE, key, 0, 0x20019, ref hKey);  // Open with KEY_QUERY_VALUE & KEY_READ access

        if (result != 0)
        {
            return null;  // Failed to open the registry key
        }

        // Get the size of the security descriptor
        int length = 0;
        Advapi32NativeMethods.RegGetKeySecurity(hKey, securityInfo, null, ref length);  // First call to get the size

        // Allocate buffer for the security descriptor
        byte[] securityDescriptor = new byte[length];

        int result1 = Advapi32NativeMethods.RegGetKeySecurity(hKey, securityInfo, securityDescriptor, ref length);


        // Close the registry key handle
        Advapi32NativeMethods.RegCloseKey(hKey);

        if (result1 != 0)
        {
            return null;  // Failed to retrieve the security descriptor
        }

        return securityDescriptor.ToString();  // Return the security descriptor
    }

    public static void LoadHive(string hiveFilePath, string subKeyName)
    {
        IntPtr hKey = (IntPtr)0x80000002; // HKEY_LOCAL_MACHINE

        int result = Advapi32NativeMethods.RegLoadKey(hKey, subKeyName, hiveFilePath);
        if (result == NativeMethodConstants.ERROR_SUCCESS)
        {
            Console.WriteLine($"Hive loaded successfully: {hiveFilePath}");
        }
        else
        {
            Console.WriteLine($"Failed to load hive: {hiveFilePath}. Error code: {result}");
            throw new InvalidOperationException($"RegLoadKey failed with error code {result}");
        }
    }

    public static string NormalizeRegistryKey(string key)
    {
        if (key.StartsWith("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
        {
            return key.Replace("HKEY_CLASSES_ROOT", "CLASSES_ROOT");
        }
        else if (key.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
        {
            return key.Replace("HKEY_CURRENT_USER", "CURRENT_USER");
        }
        else if (key.StartsWith("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
        {
            return key.Replace("HKEY_LOCAL_MACHINE", "MACHINE");
        }
        else if (key.StartsWith("HKEY_USERS", StringComparison.OrdinalIgnoreCase))
        {
            return key.Replace("HKEY_USERS", "USERS");
        }
        else
        {
            return key; // Return the key unchanged if no match
        }
    }

    

    public static string GetSidFromName(string accountName)
    {
        if (string.IsNullOrEmpty(accountName))
        {
            throw new ArgumentNullException(nameof(accountName), "Account name cannot be null or empty.");
        }

        try
        {
            // Translate the account name to a SecurityIdentifier
            NTAccount account = new NTAccount(accountName);
            SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

            // Return the SID in string format
            return sid.Value;
        }
        catch (IdentityNotMappedException)
        {
            Console.WriteLine($"The account name '{accountName}' could not be mapped to a SID.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving SID for '{accountName}': {ex.Message}");
            return null;
        }
    }



   

    public static void UnloadHive(string subKeyName)
    {
        IntPtr hKey = (IntPtr)0x80000002; // HKEY_LOCAL_MACHINE

        int result = Advapi32NativeMethods.RegUnLoadKey(hKey, subKeyName);
        if (result == NativeMethodConstants.ERROR_SUCCESS)
        {
            Console.WriteLine($"Hive unloaded successfully: {subKeyName}");
        }
        else
        {
            Console.WriteLine($"Failed to unload hive: {subKeyName}. Error code: {result}");
            throw new InvalidOperationException($"RegUnLoadKey failed with error code {result}");
        }
    }



}


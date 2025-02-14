using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner
{
    public class Program
    {

        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        private static string FrstLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FRST.txt");
        private static string SystemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
        private static string ProgramFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private static string FrstDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FRST");

        //private static List<string>
            //AllUsers = GetAllUserProfiles(); // Assuming this method retrieves user profile directories

        private const string RESTRICT = "Restricted";
        private const string UPD1 = "Update";

        //private static string[]
            //UserRegistryKeys = GetUserRegistryKeys(); // Replace with method to get user registry keys

        private static List<string> registryEntries = new List<string>();
        private const string UpdateIndicator = "UPDATED"; // Replace "UPDATED" with your desired message or value.

        private static string
            BootMode = "Normal"; // Example value, replace it with actual logic to determine boot mode.

        static string HFIXLOG = "Fixlog.txt"; // Set your log file path
        static string FIX = "Folder";
        static string NFOUND = "Not Found";
        static string FIL0 = "File not found";
        static string NOACC = "No Access";
        static string SCANB = "Scanning";
        static string NOFIXENTRY = "No fix entry found";
        static string PAD = " ";
        static string VERSION = "1.0";
        static string COMPANYNAME = "Company";
        static string END = "End";
        static string OF = "Output";
        static List<string> ARRCLSID = new List<string>(); // To store CLSID values
        static string REGIST8 = "Not found"; // Default for unknown file paths
        static string HKEY = @"HKCR\CLSID\"; // The registry hive

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]


        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainApp());
        }
    }
}
/*/
var myArray = new List<int> { 1, 2, 3, 4, 5 };
ArrayUtils.ArrayAdd(ref myArray, 6);
ArrayUtils.ArrayReverse(ref myArray);
Console.WriteLine(string.Join(", ", myArray)); // Output: 6, 5, 4, 3, 2, 1

var array = new string[3, 3]
{
{ "A", "B", "C" },
{ "D", "E", "F" },
{ "G", "H", "I" }
};

int result = ArrayUtils.ArraySearch(array, "E", 0, 0, false, 0, true);
Console.WriteLine($"Result: {result}");

var array1D = new int[5, 1] { { 5 }, { 2 }, { 9 }, { 1 }, { 3 } };
ArrayUtils.ArraySort(ref array1D, descending: true);
Console.WriteLine("Sorted 1D Array (Descending):");
for (int i = 0; i < array1D.GetLength(0); i++)
    Console.WriteLine(array1D[i, 0]);

var array2D = new int[3, 3]
{
{ 3, 2, 1 },
{ 6, 5, 4 },
{ 9, 8, 7 }
};
ArrayUtils.ArraySort(ref array2D, subItem: 1);
Console.WriteLine("Sorted 2D Array by SubItem:");
for (int i = 0; i < array2D.GetLength(0); i++)
{
    for (int j = 0; j < array2D.GetLength(1); j++)
        Console.Write(array2D[i, j] + " ");
    Console.WriteLine();
}

object[] inputArray = { 3, "B", 2, "A", 5, "C", 1 };

Console.WriteLine("Before Sort:");
Console.WriteLine(string.Join(", ", inputArray));

ArrayUtils.QuickSort1D(inputArray, 0, inputArray.Length - 1);

Console.WriteLine("After Sort:");
Console.WriteLine(string.Join(", ", inputArray));

var tableArray = new object[,]
{
{ 3, "B", 5 },
{ 1, "A", 2 },
{ 4, "C", 6 }
};

Console.WriteLine("Before Sort:");
Print2DArray(tableArray);

ArrayUtils.QuickSort2D(ref tableArray, 0, tableArray.GetLength(0) - 1, 0);

Console.WriteLine("\nAfter Sort (Ascending by Column 0):");
Print2DArray(tableArray);

var data = new int[] { 3, 5, 1, 9, 6, 7, 2, 4, 8 };

Console.WriteLine("Before Sort:");
Console.WriteLine(string.Join(", ", data));

ArrayUtils.DualPivotQuickSort(data, 0, data.Length - 1);

Console.WriteLine("After Sort:");
Console.WriteLine(string.Join(", ", data));

var data2D = new string[,]
  {
{ "A1", "B1", "C1" },
{ "A2", "B2", "C2" },
{ "A3", "B3", "C3" }
  };

var data1D = new string[] { "X1", "X2", "X3", "X4" };

// Convert 2D array to string
Console.WriteLine("2D Array:");
string result2D = ArrayUtils.ArrayToString(data2D, "|", 0, 2, "\n", 0, 2);
Console.WriteLine(result2D);

// Convert 1D array to string
Console.WriteLine("\n1D Array:");
string result1D = ArrayUtils.ArrayToString(data1D, "|", 1, 3);
Console.WriteLine(result1D);

var inputData = new int[] { 1, 2, 2, 3, 4, 4, 5, 6, 6 };

Console.WriteLine("Original Array:");
Console.WriteLine(string.Join(", ", inputData));

var uniqueValues = ArrayUtils.ArrayUnique(inputData, includeCount: true);
Console.WriteLine("\nUnique Values (with Count):");
Console.WriteLine(string.Join(", ", uniqueValues));

try
{
    // Example: List all files in the specified directory
    string directoryPath = @"C:\Example\";
    string[] files = FileUtils.FileListToArray(directoryPath, "*.txt", flag: 0, returnPath: true);

    Console.WriteLine("Files Found:");
    foreach (string file in files)
    {
        Console.WriteLine(file);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    string directoryPath = @"C:\Example\";

    // Example: List all files and directories recursively
    var fileList = FileUtils.FileListToArrayRecursive(directoryPath, "*.txt", 0, 1, 1, 1);

    Console.WriteLine("Matching Files and Directories:");
    foreach (string entry in fileList)
    {
        Console.WriteLine(entry);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Source lists
var source1 = new List<string> { "file1.txt", "file3.txt", "file2.txt" };
var source2 = new List<string> { "file4.txt", "file6.txt", "file5.txt" };

// Target list
var target = new List<string>();

// Merge and sort the lists
ArrayUtils.AddFileLists(ref target, source1, source2, sort: true);

Console.WriteLine("Merged and Sorted File List:");
foreach (var file in target)
{
    Console.WriteLine(file);
}

// 1D List Example
var list1D = new List<string>();
ListUtils.AddToList(ref list1D, "Value1");
ListUtils.AddToList(ref list1D, "Value2");
Console.WriteLine("1D List:");
foreach (var item in list1D)
{
    Console.WriteLine(item);
}

// 2D List Example (using Tuple)
var list2D = new List<Tuple<string, int>>();
ListUtils.AddToList(ref list2D, "Key1", 10);
ListUtils.AddToList(ref list2D, "Key2", 20);
Console.WriteLine("\n2D List:");
foreach (var item in list2D)
{
    Console.WriteLine($"Key: {item.Item1}, Value: {item.Item2}");
}

string mask = string.Empty;
List<string> list = new List<string> { "file1.txt;*.log;test?.csv" };

if (ListToMask(ref mask, list))
{
    Console.WriteLine("Generated Regex Mask:");
    Console.WriteLine(mask);
}
else
{
    Console.WriteLine("Invalid list format.");
}

// 1D array example
string[,] oneDArray = new string[,] { { "Line1" }, { "Line2" }, { "Line3" } };

// 2D array example
string[,] twoDArray = new string[,] {
{ "Row1Col1", "Row1Col2", "Row1Col3" },
{ "Row2Col1", "Row2Col2", "Row2Col3" },
{ "Row3Col1", "Row3Col2", "Row3Col3" }
};

// Write 1D array to file
FileUtils.FileWriteFromArray("1DOutput.txt", oneDArray);

// Write 2D array to file with a custom delimiter
FileUtils.FileWriteFromArray("2DOutput.txt", twoDArray, delimiter: ",");

string filePath = "example.txt";
int lineNumber = 3;
string text = "Inserted Text";

// Example 1: Overwrite a specific line
FileUtils.FileWriteToLine(filePath, lineNumber, text, overwrite: true);

// Example 2: Append to a specific line
FileUtils.FileWriteToLine(filePath, lineNumber, text, overwrite: false);

// Example 3: Fill missing lines if needed
FileUtils.FileWriteToLine(filePath, 10, "New Line at 10", overwrite: true, fill: true);

string basePath = @"C:\Example\SubDir";
string relativePath = @"..\AnotherDir\File.txt";

string fullPath = PathUtils.GetFullPath(relativePath, basePath);
Console.WriteLine("Resolved Full Path:");
Console.WriteLine(fullPath);

string messageBuffer = null;

uint messageId = 2; // Example message ID (can vary)
uint formatResult = WinApiUtils.WinApiFormatMessage(
    FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
    IntPtr.Zero,
    messageId,
    0, // Default language
    ref messageBuffer,
    256, // Buffer size
    IntPtr.Zero
);

if (formatResult > 0)
{
    Console.WriteLine("Formatted Message:");
    Console.WriteLine(messageBuffer);
}
else
{
    Console.WriteLine("Failed to format message.");
}

uint errorCode = 2; // ERROR_FILE_NOT_FOUND
string errorMessage = WinApiUtils.GetErrorMessage(errorCode);

if (!string.IsNullOrEmpty(errorMessage))
{
    Console.WriteLine("Error Message:");
    Console.WriteLine(errorMessage);
}
else
{
    Console.WriteLine("Failed to retrieve the error message.");
}

uint lastError = WinApiUtils.WinApiGetLastError();

Console.WriteLine("Last Error Code:");
Console.WriteLine(lastError);

if (CryptUtils.CryptStartup())
{
    Console.WriteLine("Cryptographic context initialized successfully.");
}
else
{
    Console.WriteLine("Failed to initialize cryptographic context.");
}

CryptUtils.CryptShutdown();
Console.WriteLine("Cryptographic context shut down.");

try
{
    // Example: FatalExitWithMessage
    FatalExitWithMessage(1, "This is a fatal error example.");

    // Example: HIWORD and LOWORD
    int value = 0x12345678;
    Console.WriteLine($"HIWORD: {BitwiseHelper.HIWORD(value)}, LOWORD: {BitwiseHelper.LOWORD(value)}");

    // Example: GuidFromString
    Guid guid = GuidFromString("{D63E0CE2-A0A2-11D0-9C02-00C04FC99C8E}");
    Console.WriteLine($"Generated GUID: {guid}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    // Example: MAKELONG
    int low = 0x1234, high = 0x5678;
    int combined = BitHelper.MAKELONG(low, high);
    Console.WriteLine($"MAKELONG: 0x{combined:X8}");

    // Example: MultiByteToWideChar
    byte[] multiByteText = System.Text.Encoding.UTF8.GetBytes("Hello");
    string wideText = Utility.MultiByteToWideChar(multiByteText);
    Console.WriteLine($"Wide Text: {wideText}");

    // Example: CoInitialize
    ComHandler.CoInitialize();

    // Example: Heap Allocation
    IntPtr memory = HeapUtil.HeapAlloc(100);
    Console.WriteLine($"Allocated Memory: {memory}");

    UIntPtr size = HeapUtil.HeapSize(memory);
    Console.WriteLine($"Memory Size: {size}");

    HeapUtil.HeapFree(ref memory);
    Console.WriteLine($"Memory Freed: {memory}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    // Example: Get String Length
    string example = "Hello World";
    IntPtr examplePtr = Marshal.StringToHGlobalUni(example);
    int length = Utility.GetStringLengthW(examplePtr);
    Console.WriteLine($"String Length: {length}");
    Marshal.FreeHGlobal(examplePtr);

    // Example: Empty Recycle Bin
    ShellRecycleBinHandler.EmptyRecycleBin();

    // Example: Query Recycle Bin
    var recycleBinInfo = ShellRecycleBinHandler.QueryRecycleBin();
    Console.WriteLine($"Recycle Bin Size: {recycleBinInfo.size}, Items: {recycleBinInfo.numItems}");

    // Example: Create File
    IntPtr fileHandle = NativeMethods.CreateFile("example.txt", 2, 0, 0, 0);
    Console.WriteLine($"File Handle: {fileHandle}");

    // Example: Create Symbolic Link
    FileUtils.CreateSymbolicLink("example_symlink", "example.txt");
    Console.WriteLine("Symbolic link created.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    // Example: Get Current Process Handle
    IntPtr currentProcess = ProcessUtils.GetCurrentProcessHandle();
    Console.WriteLine($"Current Process Handle: {currentProcess}");

    // Close the current process handle
    NativeMethods.CloseHandle(currentProcess);

    Console.WriteLine("Current process handle closed successfully.");

    // Example: Find First File
    var findData = new FileUtils.WIN32_FIND_DATA();
    IntPtr fileHandle = FileUtils.FindFirstFile("C:\\*.*", out findData);
    Console.WriteLine($"First File Found: {findData.cFileName}");

    // Close the search handle
    NativeMethods.FindClose(fileHandle);

    // Example: Get Profiles Directory
    string profilesDir = ProfilesDirectoryHandler.GetProfilesDirectory();
    Console.WriteLine($"Profiles Directory: {profilesDir}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

uint processId = (uint)Process.GetCurrentProcess().Id; // Get the current process ID.
uint parentProcessId = ProcessUtils.GetParentProcess(processId);

Console.WriteLine($"Current Process ID: {processId}");
Console.WriteLine($"Parent Process ID: {parentProcessId}");

try
{
    IntPtr processHandle = ProcessUtils.OpenProcess(0x001F0FFF, false, processId);
    Console.WriteLine("Process handle obtained successfully.");
    // Remember to close the handle.
    NativeMethods.CloseHandle(processHandle); // Replace with your CloseHandle logic if necessary.
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    IntPtr buffer;
    uint size = GetFileVersionInfo(@"C:\Windows\System32\notepad.exe", out buffer);

    Console.WriteLine($"Version Info Size: {size}");
    Marshal.FreeHGlobal(buffer);

    string indirectString = LoadIndirectString("@%SystemRoot%\\system32\\shell32.dll,-1");
    Console.WriteLine($"Indirect String: {indirectString}");

    string loadedString = LoadString(IntPtr.Zero, 1); // Replace with a valid HINSTANCE and resource ID
    Console.WriteLine($"Loaded String: {loadedString}");

    IntPtr library = LoadLibraryEx("user32.dll");
    Console.WriteLine("Library loaded successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    // Example usage of VerQueryValue
    IntPtr versionInfoData = IntPtr.Zero; // Load version info data (not shown here)
    string[][] info = VerQueryValue(versionInfoData);
    foreach (var entry in info)
    {
        Console.WriteLine($"Translation: {entry[0]}");
        for (int i = 1; i < entry.Length; i++)
        {
            Console.WriteLine($"Value {i}: {entry[i]}");
        }
    }

    // Example usage of ExpandEnvironmentStrings
    string expanded = FileUtils.ExpandEnvironmentStrings("%SystemRoot%\\System32");
    Console.WriteLine($"Expanded Path: {expanded}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

try
{
    string serviceName = "YourServiceName";

    // Query Service Status
    int[] status = ServiceHandler.QueryServiceStatus(serviceName);
    Console.WriteLine("Service Status:");
    foreach (var stat in status)
    {
        Console.WriteLine(stat);
    }

    // Start Service
    ServiceHandler.StartService(serviceName);
    Console.WriteLine("Service started successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Example 1: Get Locale Info
try
{
    string localeInfo = Utility.GetLocaleInfo(0x0409, 0x0002); // Locale: English (United States), Type: Language name
    Console.WriteLine($"Locale Info: {localeInfo}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Example 2: Add to a Date
DateTime today = DateTime.Now;
DateTime newDate = Utility.AddDate("d", 10, today); // Add 10 days
Console.WriteLine($"Date after 10 days: {newDate}");

// Example 3: Calculate Date Difference
DateTime startDate = new DateTime(2023, 1, 1);
DateTime endDate = new DateTime(2023, 12, 31);
int daysDiff = Utility.DateDifference("d", startDate, endDate);
Console.WriteLine($"Days difference: {daysDiff}");

// Example 1: Leap Year Check
Console.WriteLine($"2024 is a leap year: {Utility.IsLeapYear(2024)}");

// Example 2: Date Validation
string date = "2024/02/29";
Console.WriteLine($"Is valid date: {Utility.IsValidDate(date)}");

// Example 3: Split Date and Time
string datetime = "2024/02/29 12:34:56";
Utility.SplitDateTime(datetime, out int[] dateParts, out int[] timeParts);
Console.WriteLine($"Date: {string.Join("/", dateParts)}");
Console.WriteLine($"Time: {string.Join(":", timeParts)}");

// Example 4: Date to Day Value
double dayValue = Utility.DateToDayValue(2024, 2, 29);
Console.WriteLine($"Julian Day Value: {dayValue}");

// Example 5: Day Value to Date
string convertedDate = Utility.DayValueToDate(dayValue);
Console.WriteLine($"Converted Date: {convertedDate}");

Console.WriteLine($"Current Date and Time: {Utility.GetNowDateTime()}");
Console.WriteLine($"Current Date: {Utility.GetNowDate()}");

// Convert ticks to time
Utility.TicksToTime(123456789, out int hours, out int minutes, out int seconds);
Console.WriteLine($"Time: {hours}h {minutes}m {seconds}s");

// Convert time to ticks
long ticks = Utility.TimeToTicks(12, 34, 56);
Console.WriteLine($"Ticks: {ticks}");

// Days in February of a leap year
var daysInMonth = Utility.DaysInMonth(2024);
Console.WriteLine($"Days in February 2024: {daysInMonth[1]}");

// Encode FileTime
var fileTime = Utility.EncodeFileTime(12, 31, 2024, 23, 59, 59);
Console.WriteLine($"FileTime: {fileTime}");

// Encode SystemTime
var systemTime = Utility.EncodeSystemTime(1, 1, 2025, 0, 0, 0);
Console.WriteLine($"SystemTime: {systemTime}");

string zipFilePath = @"C:\example.zip";
string fileName = "example.txt";
string destinationPath = @"C:\output\example.txt";

if (ZipUtils.SaveFileToDestinationOnce(zipFilePath, fileName, destinationPath))
{
    Console.WriteLine("File successfully extracted and saved.");
}
else
{
    Console.WriteLine("Failed to extract and save the file.");
}

SecurityUtils.ScanInstalledPrograms();



}

public static void WLBALLOON()
{
// Define the registry key and value name
string registryKey = @"HKLM\" + GetSoftware() + @"\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\wlballoon";
string valueName = "DllName";
string valueType = "REG_EXPAND_SZ";
string valueData = "wlnotify.dll";

// Call the method to restore the value in the registry
RESTOREVAL(registryKey, valueName, valueType, valueData);
}

public static string GetSoftware()
{
// Return the SOFTWARE key based on your environment or implementation
return "Software";  // Replace with actual value if needed
}

public static void RESTOREVAL(string registryKey, string valueName, string valueType, string valueData)
{
try
{
    // Open the registry key in the correct hive (HKEY_LOCAL_MACHINE)
    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey, writable: true))
    {
        if (key != null)
        {
            // Set the registry value
            key.SetValue(valueName, valueData, RegistryValueKind.ExpandString);
            Console.WriteLine("Registry value set successfully.");
        }
        else
        {
            Console.WriteLine("Failed to open registry key.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error setting registry value: " + ex.Message);
}
}

public static string WL(int input)
{
string IT;

if (input == 1)
{
    IT = "WLISTED"; // Replace with actual value for WLISTED
}
else
{
    IT = "ALL"; // Replace with actual value for ALL
}

return IT;
}

public static string GetPublisherDisplayName(string input, bool isPublisher = false)
{
if (isPublisher)
{
    input = System.Text.RegularExpressions.Regex.Replace(input, "(?i)(.+/Resources/)(.+)", "$1PublisherDisplayName}");
}

try
{
    return LoadIndirectString(input);
}
catch
{
    return string.Empty;
}
}

public static string GetPublisherNameFromXml(string xmlContent)
{
string publisher = "";

if (System.Text.RegularExpressions.Regex.IsMatch(xmlContent, "(?i)<PublisherDisplayName>[^<]*<"))
{
    publisher = System.Text.RegularExpressions.Regex.Replace(xmlContent, "(?is).+<PublisherDisplayName>([^<]*)</PublisherDisplayName.+", "$1");
    if (publisher.Contains("ms-resource:"))
    {
        publisher = System.Text.RegularExpressions.Regex.Replace(xmlContent, "(?is).+Publisher=.+?O=(.+?), .+", "$1");
    }
}

return publisher;
}

public static void DetectBootMode()
{
string systemDrive = Environment.GetEnvironmentVariable("SystemDrive");
if ((systemDrive == "X:" && (Directory.Exists("x:\\windows") || Directory.Exists("x:\\i386"))) || Environment.CommandLine.Contains("simulateRE"))
{
    // Recovery Mode
    Console.WriteLine("Boot Mode: Recovery");
}
else
{
    string bootMode = "Normal";
    string value = RegistryValueHandler.GetValue(@"HKLM\system\currentcontrolset\control\safeboot\option", "OptionValue", null)?.ToString();

    if (!string.IsNullOrEmpty(value))
    {
        switch (value)
        {
            case "1":
                bootMode = "Safe Mode (minimal)";
                break;
            case "2":
                bootMode = "Safe Mode (with Networking)";
                break;
        }
    }

    Console.WriteLine($"Boot Mode: {bootMode}");
}
}

public static void GenerateShortcutScanReport()
{
Console.WriteLine("Scanning shortcuts...");

string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Shortcut.txt");
using (StreamWriter writer = new StreamWriter(reportPath))
{
    string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    writer.WriteLine($"Shortcut Scan Report - {currentDate}");
    writer.WriteLine("====================================");
    writer.WriteLine("Scanning all user shortcuts...");

    // Simulate scanning and writing results
    writer.WriteLine("Shortcut 1: Example.lnk");
    writer.WriteLine("Shortcut 2: Test.lnk");
    writer.WriteLine("====================================");
    writer.WriteLine("End of Shortcut Scan Report");
}

Console.WriteLine($"Shortcut scan report generated at: {reportPath}");
}

private static string LoadIndirectString(string input)
{
// Simulate loading a localized string from the system
return $"Localized String: {input}";
}


private static void Print2DArray<T>(T[,] array)
{
for (int i = 0; i < array.GetLength(0); i++)
{
    for (int j = 0; j < array.GetLength(1); j++)
    {
        Console.Write(array[i, j] + " ");
    }
    Console.WriteLine();
}
}

public static void GenerateAdditionReport()
{
string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
string additionFilePath = Path.Combine(scriptDir, "Addition.txt");

using (StreamWriter additionFile = new StreamWriter(additionFilePath, true, Encoding.UTF8))
{
    WriteSectionHeader(additionFile, "User Account Control");
    additionFile.WriteLine("(Security Center 1)");

    EnumerateNetUsers(additionFile);

    string domain = GetDomain();
    if (!string.IsNullOrEmpty(domain))
    {
        additionFile.WriteLine($"Updated Domain: {domain}");
    }

    AddSecurityCenterInfo(additionFile);
    WriteCustomCLSIDSection(additionFile);

    // Example context menu handlers
    WriteContextMenuHandlers(additionFile);
    WriteFolderExtensions(additionFile);

    // Placeholder for additional logging
    WriteLoadedModules(additionFile);
    WriteAlternateDataStreams(additionFile);

    // Example configuration and system settings
    WriteSystemRestoreInfo(additionFile);
    WriteFirewallStatus(additionFile);

    // Example device enumeration
    WriteDevices(additionFile);
}
}

private static void WriteSectionHeader(StreamWriter writer, string sectionName)
{
writer.WriteLine();
writer.WriteLine("==================== {0}: =============================", sectionName);
writer.WriteLine();
}

private static void EnumerateNetUsers(StreamWriter writer)
{
// Placeholder for network user enumeration
writer.WriteLine("Enumerating network users...");
}

private static string GetDomain()
{
// Placeholder for domain retrieval logic
return "ExampleDomain";
}

private static void AddSecurityCenterInfo(StreamWriter writer)
{
writer.WriteLine("Adding Security Center Info...");
}

private static void WriteCustomCLSIDSection(StreamWriter writer)
{
writer.WriteLine("Writing Custom CLSID Information...");
}

private static void WriteContextMenuHandlers(StreamWriter writer)
{
writer.WriteLine("Writing Context Menu Handlers...");
// Example of specific context menu handler logic
}

private static void WriteFolderExtensions(StreamWriter writer)
{
writer.WriteLine("Writing Folder Extensions...");
}

private static void WriteLoadedModules(StreamWriter writer)
{
writer.WriteLine("Writing Loaded Modules...");
}

private static void WriteAlternateDataStreams(StreamWriter writer)
{
writer.WriteLine("Writing Alternate Data Streams...");
}

private static void WriteSystemRestoreInfo(StreamWriter writer)
{
writer.WriteLine("Writing System Restore Information...");
}

private static void WriteFirewallStatus(StreamWriter writer)
{
writer.WriteLine("Writing Firewall Status...");
}

private static void WriteDevices(StreamWriter writer)
{
writer.WriteLine("Writing Device Information...");
}

// Add other method placeholders as necessary for functionality

public static void AAAARPM()
{
// Get the list of .exe files in the FRST directory
var badFiles = Directory.EnumerateFiles(FrstDirectory, "*.exe", SearchOption.TopDirectoryOnly).ToList();
if (badFiles.Count == 0) return;

// Open the FRST log for writing
using (var frstLog = new StreamWriter(FrstLogPath, append: true))
{
    frstLog.WriteLine(Environment.NewLine + "========================================================" + Environment.NewLine);

    foreach (var badFile in badFiles)
    {
        // Get the list of processes
        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
                // Check if the bad file is the main module of the process
                string processPath = process.MainModule?.FileName;
                if (string.Equals(badFile, processPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the process should be terminated
                    if (Procrit(process))
                    {
                        process.Kill();
                        frstLog.WriteLine($"{badFile} => Process terminated");
                    }
                }
            }
            catch
            {
                // Ignore processes that cannot be accessed
            }
        }

        // Remove special attributes from the file
        var attributes = File.GetAttributes(badFile);
        if ((attributes & (FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0)
        {
            File.SetAttributes(badFile, attributes & ~(FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Hidden));
        }

        // Move the bad file to a new destination
        string destination = GetDestination(badFile, 1);
        try
        {
            File.Move(badFile, destination);
            frstLog.WriteLine($"{badFile} => Moved");
        }
        catch
        {
            // Try to grant permissions and move again
            GrantPermissions(badFile);
            try
            {
                File.Move(badFile, destination);
                frstLog.WriteLine($"{badFile} => Moved after permissions change");
            }
            catch
            {
                frstLog.WriteLine($"\"{badFile}\" => Move failed");
            }
        }
    }
}
}

private static bool Procrit(Process process)
{
// Placeholder logic to determine if the process is critical
return true;
}

private static string GetDestination(string filePath, int option)
{
// Placeholder logic to determine the destination
return Path.Combine(FrstDirectory, "Quarantine", Path.GetFileName(filePath));
}

private static void GrantPermissions(string filePath)
{
// Placeholder logic to grant necessary permissions to the file
}

public static void AAAARPOL()
{
// Close and reopen the log file
if (File.Exists(FrstLogPath))
{
    File.Delete(FrstLogPath);
}
using (var frstLog = new StreamWriter(FrstLogPath, append: true))
{
    AAAARPOL1(Path.Combine(SystemDir, @"GroupPolicy\Machine\Registry.pol"));
    AAAARPOL1(Path.Combine(SystemDir, @"GroupPolicy\User\Registry.pol"));
    AAAARPOL1(Path.Combine(ProgramFilesDir, @"Mozilla Firefox\distribution\policies.json"));
    AAAARPOL2(Path.Combine(SystemDir, @"GroupPolicyUsers"));
    AAAARPOL3(Path.Combine(SystemDir, @"GroupPolicy\Machine\Scripts"));
    AAAARPOL3(Path.Combine(SystemDir, @"GroupPolicy\User\Scripts"));
    AAAARPOL4(Path.Combine(SystemDir, @"GroupPolicyUsers"));
    AAAARPOL5();
    AAAARPOL6(@"Mozilla\Firefox");
    AAAARPOL6(@"Google");
    AAAARPOL6(@"Microsoft\Edge");
    AAAARPOL6(@"BraveSoftware\Brave");
    AAAARPOL6(@"Vivaldi");
    AAAARPOL6(@"YandexBrowser");
}
}

public static void AAAARPOL1(string filePath)
{
string prefix = "GroupPolicy";

if (filePath.Contains(@"\User\"))
    prefix = "GroupPolicy\\User";
if (filePath.Contains(@"\Mozilla Firefox\"))
    prefix = "GroupPolicy-Firefox";

if (File.Exists(filePath))
{
    if (filePath.Contains(@"\Mozilla Firefox\"))
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
        return;
    }

    string fileContent = File.ReadAllText(filePath);

    if (fileContent.Length < 30)
        return;

    string hexString = StringToHex(fileContent);
    hexString = Regex.Replace(hexString, @"([^x][^x])00", "$1");
    string decodedContent = HexToString(hexString);

    if (decodedContent.Contains("Chrome"))
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} - Chrome <==== {UPD1}{Environment.NewLine}");
    }
    else if (decodedContent.Contains("Firefox"))
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} - Firefox <==== {UPD1}{Environment.NewLine}");
    }
    else if (decodedContent.Contains("Edge"))
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} - Edge <==== {UPD1}{Environment.NewLine}");
    }
    else if (decodedContent.Contains("Windows Defender\\Exclusions"))
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} - Windows Defender <==== {UPD1}{Environment.NewLine}");
    }
    else
    {
        File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} ? <==== {UPD1}{Environment.NewLine}");
    }
}
}


public static void AAAARPOL2(string folderPath)
{
if (!Directory.Exists(folderPath))
    return;

// Recursively find all "Registry.pol" files
var files = Directory.EnumerateFiles(folderPath, "Registry.pol", SearchOption.AllDirectories);
string prefix = "GroupPolicyUsers\\";

foreach (var filePath in files)
{
    if (Path.GetFileName(filePath).Equals("Registry.pol", StringComparison.OrdinalIgnoreCase))
    {
        string fileContent = File.ReadAllText(filePath);
        if (fileContent.Length < 30)
            continue;

        // Extract the user SID
        string userSid = Regex.Replace(filePath, @"(?i).+GroupPolicyUsers\\(.+?)\\Registry\.pol", "$1");

        // Convert file content to hexadecimal and back
        string hexContent = StringToHex(fileContent);
        hexContent = Regex.Replace(hexContent, @"([^x][^x])00", "$1");
        string decodedContent = HexToString(hexContent);

        if (decodedContent.Contains("Chrome"))
        {
            File.AppendAllText("FRST.txt", $"{prefix}{userSid}: {RESTRICT} - Chrome <==== {UPD1}{Environment.NewLine}");
        }
        else if (decodedContent.Contains("Firefox"))
        {
            File.AppendAllText("FRST.txt", $"{prefix}{userSid}: {RESTRICT} - Firefox <==== {UPD1}{Environment.NewLine}");
        }
        else if (decodedContent.Contains("Edge"))
        {
            File.AppendAllText("FRST.txt", $"{prefix}{userSid}: {RESTRICT} - Edge <==== {UPD1}{Environment.NewLine}");
        }
        else
        {
            File.AppendAllText("FRST.txt", $"{prefix}{userSid}: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
        }
    }
}
}


public static void AAAARPOL3(string folderPath)
{
// Check if either "scripts.ini" or "psscripts.ini" exists in the folder
if (!File.Exists(Path.Combine(folderPath, "scripts.ini")) &&
    !File.Exists(Path.Combine(folderPath, "psscripts.ini")))
{
    return;
}

string prefix = "GroupPolicyScripts";

// Check if the folder path contains "\User\" and log appropriately
if (folderPath.IndexOf(@"\User\", StringComparison.OrdinalIgnoreCase) >= 0)
{
    File.AppendAllText("FRST.txt", $"{prefix}\\User: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
}
else
{
    File.AppendAllText("FRST.txt", $"{prefix}: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
}
}


public static void AAAARPOL4(string folderPath)
{
// Check if either "scripts.ini" or "psscripts.ini" exists in the folder
if (!File.Exists(Path.Combine(folderPath, "scripts.ini")) &&
    !File.Exists(Path.Combine(folderPath, "psscripts.ini")))
{
    return;
}

// Retrieve a list of all files or directories that match the "Scripts" pattern
var entries = Directory.EnumerateFiles(folderPath, "*Scripts*", SearchOption.AllDirectories);
string prefix = "GroupPolicyUsers\\";

foreach (var entry in entries)
{
    if (entry.IndexOf("Scripts", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        // Extract the user SID from the file path
        string userSid = Regex.Replace(entry, @"(?i).+GroupPolicyUsers\\(.+?)\\Scripts", "$1");

        // Append the log entry to the "FRST.txt" file
        File.AppendAllText("FRST.txt", $"{prefix}{userSid}\\Scripts: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
    }
}
}


public static void AAAARPOL5()
{
using (var frstLog = new StreamWriter(FrstLogPath, append: true))
{
    foreach (var userProfile in AllUsers)
    {
        string userPolicyPath = Path.Combine(userProfile, "NTUSER.pol");

        if (!File.Exists(userPolicyPath))
        {
            continue;
        }

        string fileContent = File.ReadAllText(userPolicyPath);

        if (fileContent.Length < 30)
        {
            continue;
        }

        frstLog.WriteLine($"Policies: {userPolicyPath}: {RESTRICT} <==== {UPD1}");
    }
}
}

public static void AAAARPOL6(string subKey)
{
string baseKey = $@"HKLM\SOFTWARE\Policies\{subKey}";

if (TryReadRegistryKey(baseKey))
{
    AAAPOL00(baseKey, true); // Changed from '1' to 'true'
    return;
}

foreach (var userKey in UserRegistryKeys)
{
    string userPolicyKey = $@"HKU\{userKey}\SOFTWARE\Policies\{subKey}";

    if (TryReadRegistryKey(userPolicyKey))
    {
        AAAPOL00(userPolicyKey, true); // Changed from '1' to 'true'
        return;
    }
}
}


private static List<string> GetAllUserProfiles()
{
// Retrieve all user profile paths (e.g., from "C:\Users")
string usersDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string parentDirectory = Directory.GetParent(usersDirectory).FullName;

if (Directory.Exists(parentDirectory))
{
    return new List<string>(Directory.GetDirectories(parentDirectory));
}

return new List<string>();
}

private static bool TryReadRegistryKey(string keyPath)
{
try
{
    using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
    using (var subKey = key.OpenSubKey(keyPath, writable: false))
    {
        return subKey != null;
    }
}
catch
{
    return false;
}
}

public static void AAAPOL00(string key, bool ie)
{
if (ie)
{
    File.AppendAllText("FRST.txt", $"{key}: {RESTRICT} <==== {UPD1}{Environment.NewLine}");
}
else
{
    registryEntries.Add($"{key}: {RESTRICT} <==== {UPD1}");
}
}


private static string[] GetUserRegistryKeys()
{
// Implement logic to retrieve user registry keys (e.g., SID paths under HKU)
return new string[] { "S-1-5-21-...-1001", "S-1-5-21-...-1002" }; // Example keys
}

public static void AAAAPOL0(string key, int ie)
{
// Check if the base key contains any values
var registryValue = TryReadRegistryValue(key, GetFirstRegistryValueName(key));
if (!string.IsNullOrEmpty(registryValue))
{
    AAAPOL00(key, ie != 0); // Convert int to bool
    return;
}

using (var hkey = OpenRegistryKey(key))
{
    if (hkey == null)
        return;

    int subKeyIndex = 0;
    while (true)
    {
        var subKeyName = RegistrySubKeyHandler.EnumerateSubKey(hkey, subKeyIndex++);
        if (subKeyName == null)
            break;

        string subKeyPath = $@"{key}\{subKeyName}";
        registryValue = TryReadRegistryValue(subKeyPath, GetFirstRegistryValueName(subKeyPath));
        if (!string.IsNullOrEmpty(registryValue))
        {
            AAAPOL00(key, ie != 0); // Convert int to bool
            return;
        }

        using (var hkey1 = OpenRegistryKey(subKeyPath))
        {
            if (hkey1 == null)
                continue;

            int subSubKeyIndex = 0;
            while (true)
            {
                var subSubKeyName = RegistrySubKeyHandler.EnumerateSubKey(hkey1, subSubKeyIndex++);
                if (subSubKeyName == null)
                    break;

                string subSubKeyPath = $@"{subKeyPath}\{subSubKeyName}";
                registryValue = TryReadRegistryValue(subSubKeyPath, GetFirstRegistryValueName(subSubKeyPath));
                if (!string.IsNullOrEmpty(registryValue))
                {
                    AAAPOL00(key, ie != 0); // Convert int to bool
                    return;
                }
            }
        }
    }
}
}



private static RegistryKey OpenRegistryKey(string keyPath)
{
try
{
    return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                      .OpenSubKey(keyPath, writable: false);
}
catch
{
    return null;
}
}


private static string GetFirstRegistryValueName(string keyPath)
{
try
{
    using (var key = OpenRegistryKey(keyPath))
    {
        if (key != null)
        {
            var valueNames = key.GetValueNames();
            return valueNames.Length > 0 ? valueNames[0] : null;
        }
    }
}
catch { }

return null;
}

private static string TryReadRegistryValue(string keyPath, string valueName)
{
try
{
    using (var key = OpenRegistryKey(keyPath))
    {
        if (key != null && !string.IsNullOrEmpty(valueName))
        {
            return key.GetValue(valueName)?.ToString();
        }
    }
}
catch
{
    // Handle/log exception if needed
}

return null;
}


private static bool TryReadRegistryValueExists(string keyPath, string valueName)
{
try
{
    using (var key = OpenRegistryKey(keyPath))
    {
        if (key != null && !string.IsNullOrEmpty(valueName))
        {
            return key.GetValue(valueName) != null;
        }
    }
}
catch
{
    // Handle/log exception if needed
}

return false;
}


private static string StringToHex(string input)
{
return BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(input)).Replace("-", "");
}

private static string HexToString(string hex)
{
byte[] bytes = new byte[hex.Length / 2];
for (int i = 0; i < bytes.Length; i++)
    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

return System.Text.Encoding.UTF8.GetString(bytes);
}

private static double GetOsVersion()
{
var version = Environment.OSVersion.Version;
return version.Major + version.Minor / 10.0; // For example, Windows 10.0 => 10.0
}


public static void HandleStartupFolders()
{
string startupDirKey;
string startupDir = null;
string commonStartupDirKey;
string commonStartupDir = null;

if (BootMode != "Recovery")
{
    startupDirKey = GetOsVersion() > 5.2
        ? @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"
        : @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell";

    startupDir = TryReadRegistryValue(startupDirKey, "startup");

    if (!string.IsNullOrEmpty(startupDir))
    {
        if (GetOsVersion() > 5.2 &&
            !Regex.IsMatch(startupDir, @"(?i)AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup$|^\\\\[^\\]+\\users\\[^\\]+\\Start Menu\\Programs\\Startup"))
        {
            AddToRegistryLog($"StartupDir: {startupDir} <==== {UpdateIndicator}");
        }

        HandleStartupFolder(ExpandEnvironmentVariable(startupDir));
    }
}
else
{
    foreach (var userFolder in AllUsers)
    {
        if (Regex.IsMatch(userFolder, @"(?i)\\(All Users|default|public|ProgramData)$"))
            continue;

        string userName = Regex.Replace(userFolder, @".+\\(.+)", "$1");

        RunCommand($"reg load \"hku\\{userName}\" \"{userFolder}\\ntuser.dat\"");

        string userStartupDir = TryReadRegistryValue($@"HKU\{userName}\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", "startup");

        if (!string.IsNullOrEmpty(userStartupDir) &&
            !Regex.IsMatch(userStartupDir, @"(?i)AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup$|^\\\\[^\\]+\\users\\[^\\]+\\Start Menu\\Programs\\Startup"))
        {
            AddToRegistryLog($"StartupDir[{userName}]: {userStartupDir} <==== {UpdateIndicator}");
        }

        RunCommand($"reg unload \"hku\\{userName}\"");
    }
}

commonStartupDirKey = GetOsVersion() > 5.2
    ? $@"HKLM\{SoftwarePath}\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"
    : $@"HKLM\{SoftwarePath}\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

commonStartupDir = TryReadRegistryValue(commonStartupDirKey, "Common Startup");

if (!string.IsNullOrEmpty(commonStartupDir) &&
    GetOsVersion() > 5.2 &&
    !Regex.IsMatch(commonStartupDir, @"(?i)ProgramData(|%)\\Microsoft\\Windows\\Start Menu\\Programs\\Startup$"))
{
    AddToRegistryLog($"StartupCommonDir: {commonStartupDir} <==== {UpdateIndicator}");
}

if (!string.IsNullOrEmpty(commonStartupDir))
{
    HandleStartupFolder(ExpandEnvironmentVariable(commonStartupDir));
}
}



private static string SoftwarePath
{
get
{
    return Environment.Is64BitOperatingSystem ? @"SOFTWARE\WOW6432Node" : @"SOFTWARE";
}
}


private static void AddToRegistryLog(string message)
{
// Append the message to your registry log file or collection
Console.WriteLine(message);
}

private static void HandleStartupFolder(string path)
{
// Process startup folder content
Console.WriteLine($"Processing startup folder: {path}");
}

private static string ExpandEnvironmentVariable(string path)
{
if (string.IsNullOrEmpty(path))
    return path;

// Match the first environment variable in the path
var match = System.Text.RegularExpressions.Regex.Match(path, @"%([^\\]+)%.*");
if (match.Success)
{
    string envVar = match.Groups[1].Value;
    string envValue = Environment.GetEnvironmentVariable(envVar) ?? string.Empty;

    // Replace backslashes with double backslashes
    envValue = envValue.Replace(@"\", @"\\");

    // Replace the first occurrence of the environment variable placeholder
    return System.Text.RegularExpressions.Regex.Replace(path, @"%[^%]+%", envValue);
}

return path;
}

private static void StartupFix(string fix, string bootMode, double osVersion, string fixLogPath)
{
string user = null;
string key;
string value;
string regType;

if (System.Text.RegularExpressions.Regex.IsMatch(fix, @"StartupDir(|\[.+\]):"))
{
    // Extract user from the fix string
    user = System.Text.RegularExpressions.Regex.Replace(fix, @"StartupDir\[(.+?)\].+", "$1");

    if (osVersion > 5.2)
    {
        key = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders";
        if (bootMode.Equals("recovery", StringComparison.OrdinalIgnoreCase))
            key = $@"HKU\{user}\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders";

        value = @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
        regType = "REG_EXPAND_SZ";
    }
    else
    {
        key = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell";
        value = @"C:\Documents and Settings\Winxp\Start Menu\Programs\Startup";
        regType = "REG_SZ";
    }

    if (bootMode.Equals("recovery", StringComparison.OrdinalIgnoreCase))
        ReloadUserRegistry(user);

    if (TryWriteRegistryValue(key, "Startup", regType, value))
    {
        WriteToFixLog(fixLogPath, $"{fix} => Restored");
    }
    else
    {
        WriteToFixLog(fixLogPath, $"{fix} => Not Restored");
    }

    if (bootMode.Equals("recovery", StringComparison.OrdinalIgnoreCase))
        UnloadUserRegistry(user);
}
else
{
    if (osVersion > 5.2)
    {
        key = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders";
        regType = "REG_EXPAND_SZ";
    }
    else
    {
        key = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
        regType = "REG_SZ";
    }

    if (TryWriteRegistryValue(key, "Common Startup", regType, @"%ProgramData%\Microsoft\Windows\Start Menu\Programs\Startup"))
    {
        WriteToFixLog(fixLogPath, $"{fix} => Restored");
    }
    else
    {
        WriteToFixLog(fixLogPath, $"{fix} => Not Restored");
    }
}
}

// Helper Methods
private static bool TryWriteRegistryValue(string keyPath, string valueName, string valueType, string valueData)
{
try
{
    using (var key = OpenRegistryKey(keyPath, writable: true))
    {
        if (key == null)
            return false;

        if (valueType.Equals("REG_EXPAND_SZ", StringComparison.OrdinalIgnoreCase))
        {
            key.SetValue(valueName, valueData, Microsoft.Win32.RegistryValueKind.ExpandString);
        }
        else if (valueType.Equals("REG_SZ", StringComparison.OrdinalIgnoreCase))
        {
            key.SetValue(valueName, valueData, Microsoft.Win32.RegistryValueKind.String);
        }

        return true;
    }
}
catch
{
    return false;
}
}

private static void WriteToFixLog(string logPath, string message)
{
try
{
    System.IO.File.AppendAllText(logPath, message + Environment.NewLine);
}
catch
{
    // Handle or log file writing errors
}
}

private static void ReloadUserRegistry(string user)
{
RunCommand($"reg load \"hku\\{user}\" \"C:\\Users\\{user}\\ntuser.dat\"");
}

private static void UnloadUserRegistry(string user)
{
RunCommand($"reg unload \"hku\\{user}\"");
}

private static void RunCommand(string command)
{
try
{
    var process = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();
    process.WaitForExit();
}
catch
{
    // Handle command execution errors
}
}

private static Microsoft.Win32.RegistryKey OpenRegistryKey(string path, bool writable = false)
{
try
{
    return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, writable);
}
catch
{
    return null;
}
}

public static void WriteRegistryToLog(string frstLogPath, string tempDir, string[] registryEntries, bool isCheckBox1Checked, string regB, string register9, string register10)
{
string reg101Path = Path.Combine(tempDir, "reg101");

// Write initial log headers
using (StreamWriter frstLog = new StreamWriter(frstLogPath, append: true))
{
    frstLog.WriteLine();
    frstLog.WriteLine($"==================== {regB} ({WhitelistLog(isCheckBox1Checked)}) ===================");
    frstLog.WriteLine();
}

// Write registry entries to temporary file
using (StreamWriter reg101 = new StreamWriter(reg101Path))
{
    reg101.WriteLine($"({register9} {register10})");
    reg101.WriteLine();

    foreach (var entry in registryEntries)
    {
        reg101.WriteLine(entry);
    }
}

// Read from the temporary file
string regExpr;
using (StreamReader reg101Reader = new StreamReader(reg101Path))
{
    regExpr = reg101Reader.ReadToEnd();
}

// Replace http with hxxp to prevent accidental link clicks
regExpr = Regex.Replace(regExpr, "(?i)http(s|):", "hxxp$1:");

// If checkbox is checked, apply a whitelist filter
if (isCheckBox1Checked)
{
    ApplyWhitelistToRegistry();
}

// Write processed registry data to the final log
using (StreamWriter frstLog = new StreamWriter(frstLogPath, append: true))
{
    frstLog.Write(regExpr);
}

// Delete the temporary file
if (File.Exists(reg101Path))
{
    File.Delete(reg101Path);
}
}

// Helper method to handle the whitelist logic
private static void ApplyWhitelistToRegistry()
{
// Implement the whitelist logic here
}

// Helper method to generate whitelist log output
private static string WhitelistLog(bool isChecked)
{
return isChecked ? "Enabled" : "Disabled";
}

public static void FixBcd(string command)
{
string tempDir = Path.GetTempPath();  // Get the temporary directory path
string bcdFilePath = Path.Combine(tempDir, "editbcd");

// Delete the file if it exists
if (File.Exists(bcdFilePath))
{
    File.Delete(bcdFilePath);
}

// Log the header
File.AppendAllText("HFIXLOG.txt", "\r\n=========================  bcdedit ========================" + "\r\n\r\n");

// Run the command and redirect output to the bcd file
var processStartInfo = new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c {command} > \"{bcdFilePath}\" 2>&1",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};

// Start the process and wait for it to complete
using (var process = Process.Start(processStartInfo))
{
    process.WaitForExit();
}

// Read the output and append it to the log file
string output = File.ReadAllText(bcdFilePath);
File.AppendAllText("HFIXLOG.txt", "\r\n" + output);

// Log the footer
File.AppendAllText("HFIXLOG.txt", "\r\n========= " + "END" + " " + "OF" + " bcdedit =========" + "\r\n\r\n");

// Delete the temporary bcd file
File.Delete(bcdFilePath);
}
public static string FixBr()
{
Random random = new Random();
int mr = random.Next(10, 19); // Random number between 10 and 18
string ran = "\\";

for (int i = 0; i < mr; i++)
{
    ran += (char)random.Next(97, 123); // Random character between 'a' and 'z'
}

return ran + ".txt";
}

public static void FixBt()
{
string fixList = FixBr();
string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fixList);

// Create and write to the file
File.WriteAllText(filePath, "̩");

// Open the file using Notepad
System.Diagnostics.Process.Start("notepad.exe", filePath);
}

public static void FixButt()
{
string fixList = "\\fixlist.txt";
string scriptDir = AppDomain.CurrentDomain.BaseDirectory;

// Check if the fixlist.txt file exists
string fixFilePath = scriptDir + fixList;
if (File.Exists(fixFilePath + ".txt"))
{
    File.Move(fixFilePath + ".txt", fixFilePath);
}

if (!File.Exists(fixFilePath))
{
    // Assume ClipGet() retrieves clipboard contents, replace with actual method
    string clipboardContent = GetClipboardContent(); // Implement this method
    if (Regex.IsMatch(clipboardContent, "(?is)Start::.+End::"))
    {
        fixList = FixBr();
        string filePath = Path.Combine(scriptDir, fixList);
        File.WriteAllText(filePath, clipboardContent);
    }
    else
    {
        Console.WriteLine("No valid fix list found in clipboard.");
        return;
    }
}

// Reading the fixlist
string[] lines = File.ReadAllLines(fixFilePath);
string sf = string.Join("\n", lines);

if (sf.Contains("HKU") || sf.Contains("Emptytemp:"))
{
    // Implement GETFILELIST2() and LOAD()
}

// Continue processing the file
ProcessFileContent(sf);

// Example of logging and handling file content
string logFilePath = Path.Combine(scriptDir, "Fixlog.txt");
using (StreamWriter logFile = new StreamWriter(logFilePath, true))
{
    logFile.WriteLine("Fixlog generated at: " + DateTime.Now);
    logFile.WriteLine("Processed fixlist: " + fixList);
}
}

public static void ProcessFileContent(string content)
{
// Implement this method to handle the file content processing
}

public static string GetClipboardContent()
{
// Implement the method to get clipboard content in C#
return "Start::Some fix data::End::"; // Dummy return for illustration
}

public static void FixCatalog5(string key, string val, string lp)
{
string ret = WriteRegistryValue(key + val, "LibraryPath", lp);
string logMessage = string.Empty;

if (ret == "1")
{
    logMessage = $"Winsock: Catalog5 {val}\\LibraryPath => Restored ({lp})";
}
else
{
    logMessage = $"{key}{val}\\LibraryPath => Error saving value";
}

File.AppendAllText("Fixlog.txt", logMessage + Environment.NewLine);
}

public static string WriteRegistryValue(string key, string valueName, string valueData)
{
try
{
    using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true))
    {
        if (registryKey != null)
        {
            registryKey.SetValue(valueName, valueData, RegistryValueKind.String);
            return "1"; // Successfully written
        }
        else
        {
            return "0"; // Error: key not found
        }
    }
}
catch (Exception ex)
{
    // Log the exception or handle it accordingly
    return "0";
}
}

// Import the DllCall for flushing the DNS cache
[DllImport("dnsapi.dll", CharSet = CharSet.Auto)]
public static extern bool DnsFlushResolverCache();

public static bool FlushDNS()
{
try
{
    bool result = DnsFlushResolverCache();
    return result; // If the function executes successfully, return the result (true or false)
}
catch (Exception ex)
{
    // Handle the exception or log it accordingly
    Console.WriteLine("Error flushing DNS cache: " + ex.Message);
    return false;
}
}

static void FOLDER()
{
File.AppendAllText(HFIXLOG, Environment.NewLine + "========================= " + FIX + " ========================" + Environment.NewLine + Environment.NewLine);

string folder = Regex.Replace(FIX, "(?i)Folder:\\s*(.+)", "$1");
folder = Regex.Replace(folder, "^\"|\\s+$|\"$", "");
folder = Regex.Replace(folder, "\\$", "");

if (!Directory.Exists(folder))
{
    File.AppendAllText(HFIXLOG, NFOUND + "." + Environment.NewLine);
    FOLDER0();
    return;
}

if ((File.GetAttributes(folder) & FileAttributes.Directory) == 0)
{
    File.AppendAllText(HFIXLOG, folder + " = " + FIL0 + Environment.NewLine);
    FOLDER0();
    return;
}

if (CreateFile(folder))
{
    File.AppendAllText(HFIXLOG, NOACC + Environment.NewLine);
    FOLDER0();
    return;
}

string[] folderArray = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

foreach (var filePath in folderArray)
{
    Console.WriteLine(SCANB + " " + folder + " : " + filePath);
    string fileAttributes = File.GetAttributes(filePath).ToString();
    if (IsReparsePoint(filePath)) fileAttributes += "L";
    string attributesFormatted = string.Format("{0:D5}", fileAttributes);
    string attributesString = Regex.Replace(attributesFormatted, "0", "_");
    long fileSize = new FileInfo(filePath).Length;
    string sizeFormatted = string.Format("{0:D9}", fileSize);
    string creationDate = File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
    string modifiedDate = File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");

    string version = GetFileVersion(filePath);
    version = Regex.Replace(version, "\\s+", " ");
    string company = "";

    string hash = "00000000000000000000000000000000";

    if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
    {
        hash = GetMD5Hash(filePath);
        if (string.IsNullOrEmpty(hash)) hash = "00000000000000000000000000000000";
        if (Regex.IsMatch(filePath, "(?i)\\.(dll|exe|sys|mui)$"))
        {
            COMP(filePath);
        }
        else
        {
            company = " (" + version + ")";
        }
    }
    else
    {
        company = " (" + version + ")";
    }

    File.AppendAllText(HFIXLOG, creationDate + " - " + modifiedDate + " - " + sizeFormatted + " " + attributesString + " [" + hash + "]" + company + " " + filePath + Environment.NewLine);
}

FOLDER0();
}

static bool CreateFile(string path)
{
// Implement logic to create file or check file access
return false;
}

static bool IsReparsePoint(string path)
{
// Implement logic to check if a file is a reparse point
return false;
}

static string GetFileVersion(string path)
{
// Implement logic to get the file version (example using FileVersionInfo)
return "1.0.0.0";
}

static string GetMD5Hash(string filePath)
{
// Implement logic to calculate MD5 hash of the file
return "dummyhash";
}

static void COMP(string filePath)
{
// Implement your logic for COMP function
}

static void FOLDER0()
{
// Append the log with the necessary details
File.AppendAllText(HFIXLOG, Environment.NewLine + "====== " + END + " " + OF + " Folder: ======" + Environment.NewLine + Environment.NewLine);
}

static void FOLDEREX(string hive, string key)
{
// Open the registry key
RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key);

if (registryKey == null)
{
    Console.WriteLine("Registry key not found.");
    return;
}

string[] subKeys = registryKey.GetSubKeyNames();
string clsid, valn, path, file = "", company = "", cdate = "";

foreach (string subKey in subKeys)
{
    clsid = subKey; // In C#, each subkey is like a CLSID
    valn = GetRegKeyValue(Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("CLSID\\" + clsid), "");
    path = GetRegKeyValue(Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("CLSID\\" + clsid + "\\InprocServer32"), "");

    if (!string.IsNullOrEmpty(path))
    {
        file = path;  // Assigning a valid value to file
        AAAAFP();

        if (!File.Exists(file))
        {
            file = path + " -> " + REGIST8;
        }
        else
        {
            cdate = " [" + cdate + "]";
        }
    }

    // Add to ARRCLSID if conditions match
    if (!Regex.IsMatch(file, @"(?i)WINDOWS\\system32\\shell32\.dll") && !Regex.IsMatch(company, @"(?i)Microsoft Corporation"))
    {
        ARRCLSID.Add($"{hive}: [{valn}] -> {clsid} => {file} {cdate} {company}");
    }
}
}

// Helper function to get the value from a registry key
static string GetRegKeyValue(RegistryKey key, string subKey)
{
if (key == null)
    return null;

return key.GetValue(subKey)?.ToString();
}

// Helper method for AAAAFP, assuming this is some function that does something with the file
static void AAAAFP()
{
// Placeholder for the original AAAFP functionality
}

static void FOLDEREXFIX(string fix)
{
string user = "";
string clsid = Regex.Replace(fix, @"(?i).+?: .+ -> (.+) =>.*", "$1");
string key = @"HKLM\SOFTWARE\Classes\Drive\shellex\FolderExtensions\" + clsid;
string key1 = @"HKLM\Software\Classes\CLSID\" + clsid;

if (Regex.IsMatch(fix, @"FolderExtensions_"))
{
    user = Regex.Replace(fix, @"FolderExtensions_([^:]+):.+", "$1");
    key = @"HKU\" + user + @"\SOFTWARE\Classes\Drive\shellex\FolderExtensions\" + clsid;
    key1 = @"HKU\" + user + @"\SOFTWARE\Classes\CLSID\" + clsid;
}

DELKEY(key);
if (IsKeyExist(key1)) DELKEY(key1);
}

static void DELKEY(string key)
{
try
{
    RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true);
    if (registryKey != null)
    {
        registryKey.DeleteSubKeyTree(key, throwOnMissingSubKey: false);
        Console.WriteLine($"Deleted registry key: {key}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error deleting registry key {key}: {ex.Message}");
}
}

static bool IsKeyExist(string key)
{
try
{
    RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key);
    return registryKey != null;
}
catch
{
    return false;
}
}

static void GETFILELIST(string folder)
{
if (!Directory.Exists(folder)) return;

List<string> fileArray = Directory.GetFiles(folder).ToList();
if (fileArray.Count == 0) return;

int daysOld = 31;
// Assuming CheckBox14 is some external condition, we use 91 if it's true.
bool checkBox14Checked = true;  // Placeholder condition
if (checkBox14Checked)
    daysOld = 91;

foreach (var filePath in fileArray)
{
    // Assuming CheckBox12 is an external condition.
    bool checkBox12Checked = true;  // Placeholder condition
    string scanMessage = "Scanning: " + filePath;
    Console.WriteLine(scanMessage);

    if (checkBox12Checked)
    {
        // Placeholder for _ADS_LIST_NTQUERY and _REPARSEPOINT logic.
        List<string> ads = GetFileAds(filePath);
        foreach (var ad in ads)
        {
            if (!Regex.IsMatch(ad, @"(?i):(\${3D0CE612-FDEE-43f7-8ACA-957BEC0CCBA0}\..*|SmartScreen|Zone.Identifier|encryptable|favicon|ms-properties|OECustomProperty|Win32App.*)$"))
            {
                Console.WriteLine($"{filePath} {ad}");
            }
        }
    }

    if (!IsReparsePoint(filePath) && CreateFile(filePath))
    {
        if (!Regex.IsMatch(filePath, @"(?i):\\(Recovery|hiberfil.sys|System (Recovery|Repair|Volume Information)|WINDOWS\\CSC|OSRSS|WINDOWS\\system32\\WebThreatDefSvc)$"))
        {
            string dateModified = File.GetLastWriteTime(filePath).ToString();
            Console.WriteLine($"Locked: {dateModified} {filePath}");
        }
    }

    if (checkBox14Checked)
    {
        if (Regex.IsMatch(filePath, @"(?i)\.(exe|dll)$") && !File.GetAttributes(filePath).HasFlag(FileAttributes.Directory) && FileUtils.CheckFileSignature(filePath) != 11)
        {
            string dateCreated = File.GetCreationTime(filePath).ToString();
            string dateModified = File.GetLastWriteTime(filePath).ToString();
            string attributes = GetFileAttributes(filePath);
            long size = new FileInfo(filePath).Length;
            string version = GetFileVersion(filePath);

            Console.WriteLine($"{dateCreated} - {dateModified} - {size} {attributes} {version} {filePath}");
        }
    }

    // Placeholder for _DATEDIFF and _NOWCALC logic.
    DateTime fileDate = File.GetLastWriteTime(filePath);
    int dateDiff = (DateTime.Now - fileDate).Days;
    if (dateDiff < daysOld || fileDate.Year > DateTime.Now.Year)
    {
        // Apply conditions for exclusion based on regex matching
        if (!Regex.IsMatch(filePath, @"(?i).*\.(exe|dll|sys)$"))
        {
            string dateCreated = File.GetCreationTime(filePath).ToString();
            string dateModified = File.GetLastWriteTime(filePath).ToString();
            long size = new FileInfo(filePath).Length;
            string attributes = GetFileAttributes(filePath);
            string version = GetFileVersion(filePath);

            Console.WriteLine($"{dateCreated} - {dateModified} - {size} {attributes} {version} {filePath}");
        }
    }
}
}

static List<string> GetFileAds(string filePath)
{
// Placeholder logic for retrieving ADS
return new List<string> { "Ad1", "Ad2" };  // Example ADS
}


static string GetFileAttributes(string path)
{
// Placeholder for fetching file attributes
return "A";
}
const uint GENERIC_READ = 0x80000000;
const uint OPEN_EXISTING = 3;
const uint FILE_SHARE_READ = 0x00000001;
const uint FILE_SHARE_WRITE = 0x00000002;

[DllImport("kernel32.dll", SetLastError = true)]
static extern IntPtr CreateFile(
string lpFileName,
uint dwDesiredAccess,
uint dwShareMode,
IntPtr lpSecurityAttributes,
uint dwCreationDisposition,
uint dwFlagsAndAttributes,
IntPtr hTemplateFile);

[DllImport("kernel32.dll", SetLastError = true)]
static extern bool ReadFile(
IntPtr hFile,
byte[] lpBuffer,
uint nNumberOfBytesToRead,
out uint lpNumberOfBytesRead,
IntPtr lpOverlapped);

[DllImport("kernel32.dll", SetLastError = true)]
static extern bool SetFilePointerEx(
IntPtr hFile,
long liDistanceToMove,
IntPtr lpNewFilePointer,
uint dwMoveMethod);

[DllImport("kernel32.dll", SetLastError = true)]
static extern bool CloseHandle(IntPtr hObject);

public static string GetMBR(string sFilename)
{
uint nBytesReceived = 0;
IntPtr hFile = CreateFile(
    sFilename,
    GENERIC_READ,
    FILE_SHARE_READ | FILE_SHARE_WRITE,
    IntPtr.Zero,
    OPEN_EXISTING,
    0,
    IntPtr.Zero);

if (hFile == IntPtr.Zero || hFile.ToInt64() == -1)
{
    int error = Marshal.GetLastWin32Error();
    string errorMsg = $"Could not open file {sFilename}\nError: {error}\nHandle: {hFile}";
    Console.WriteLine($"Error: _WinAPI_CreateFile\n{errorMsg}");
    return null;
}

try
{
    if (!SetFilePointerEx(hFile, 0, IntPtr.Zero, 0))
    {
        int error = Marshal.GetLastWin32Error();
        Console.WriteLine($"Error: _WinAPI_SetFilePointer\nCould not move pointer. (Error {error})");
        return null;
    }

    byte[] buffer = new byte[512];
    if (!ReadFile(hFile, buffer, (uint)buffer.Length, out nBytesReceived, IntPtr.Zero))
    {
        int error = Marshal.GetLastWin32Error();
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "readmbr"), $"Error reading MBR: (Error {error})\n");
        return null;
    }

    if (nBytesReceived < 512)
    {
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "readmbr"), $"Attempted reading MBR returned {nBytesReceived} bytes.\n");
        return null;
    }

    return BitConverter.ToString(buffer).Replace("-", "");
}
finally
{
    CloseHandle(hFile);
}

}

public static List<string> GetProfiles()
{
List<string> arrProfiles = new List<string>();
string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
string profilesDirectory = null;

if (GetOSVersion() < 6)
{
    profilesDirectory = (string)RegistryValueHandler.GetValue($@"HKEY_LOCAL_MACHINE\{keyPath}", "ProfilesDirectory", null);
    if (profilesDirectory != null && profilesDirectory.Contains("%"))
    {
        profilesDirectory = ExpandEnvironmentVariables(profilesDirectory);
    }
}

// Open the registry key
using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
{
    if (hKey == null) return arrProfiles;

    // Get all subkey names
    string[] subKeyNames = hKey.GetSubKeyNames();
    foreach (var subKeyName in subKeyNames)
    {
        using (RegistryKey subKey = hKey.OpenSubKey(subKeyName))
        {
            if (subKey == null) continue;

            string profileImagePath = (string)subKey.GetValue("ProfileImagePath");
            if (!string.IsNullOrEmpty(profileImagePath))
            {
                if (profileImagePath.Contains("%"))
                {
                    profileImagePath = ExpandEnvironmentVariables(profileImagePath);
                }
                if (GetOSVersion() < 6 && profilesDirectory != null)
                {
                    profileImagePath = System.IO.Path.Combine(profilesDirectory, profileImagePath);
                }
                arrProfiles.Add(profileImagePath);
            }
        }
    }
}

// Remove duplicates
arrProfiles = RemoveDuplicates(arrProfiles);
return arrProfiles;
}

// Helper Methods
private static int GetOSVersion()
{
// Simulate OS version retrieval
// Return the version number (e.g., 6 for Windows Vista and later)
return Environment.OSVersion.Version.Major;
}

private static string ExpandEnvironmentVariables(string value)
{
return Environment.ExpandEnvironmentVariables(value);
}

private static List<string> RemoveDuplicates(List<string> list)
{
HashSet<string> uniqueSet = new HashSet<string>(list);
return new List<string>(uniqueSet);
}
public static List<string> GetGPExtensions()
{
string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\GPExtensions";
List<string> arrayReg = new List<string>();

using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
{
    if (hKey == null) return arrayReg;

    foreach (string subKeyName in hKey.GetSubKeyNames())
    {
        string path = null;
        string file = null;
        string cDate = null;
        string company = null;

        using (RegistryKey subKey = hKey.OpenSubKey(subKeyName))
        {
            if (subKey == null) continue;

            path = subKey.GetValue("DllName") as string;

            if (!string.IsNullOrEmpty(path))
            {
                file = path;
                // Call AAAAFP() equivalent here if necessary
            }
            else
            {
                if (IsCheckboxChecked(1) &&
                    Regex.IsMatch(subKeyName, @"(?i)^{35378EAC-683F-11D2-A89A-00C04FBBCFA2}$"))
                {
                    continue;
                }
            }

            if (!string.IsNullOrEmpty(cDate))
            {
                cDate = $" [{cDate}]";
                path = file;
            }

            if (IsCheckboxChecked(1))
            {
                if (company?.Contains("Microsoft Corp") == true &&
                    Regex.IsMatch(path, @"(?i):\\WINDOWS\\System32\\(gpprefcl|wlgpclnt|AppManagementConfiguration|auditcse|fdeploy|dskquota|gptext|gpscript|TsUsbRedirectionGroupPolicyExtension|iedkcs32|tsworkspace|WorkFoldersGPExt|dmenrollengine|srchadmin|scecli|gpprnext|hvsigpext|dot3gpclnt|pwlauncher|cscobj|appmgmts|domgmt|polstore|dggpext|RdpGroupPolicyExtension)\.dll"))
                {
                    continue;
                }
            }

            arrayReg.Add($"HKLM\\Software\\...\\Winlogon\\GPExtensions: [{subKeyName}] -> {path}{cDate}{company}");
        }
    }
}

return arrayReg;
}

// Helper Methods
private static bool IsCheckboxChecked(int checkboxId)
{
// Simulate GUI checkbox status
return true;
}
public static void GPExtensionsFix(string fix)
{
// Extract the subkey name using regex
string sKey = Regex.Replace(fix, @".+?: \[(.+?)\].+", "$1");

// Construct the full registry key path
string fullKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\GPExtensions\{sKey}";

// Delete the registry key
try
{
    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\GPExtensions", writable: true))
    {
        if (key != null)
        {
            key.DeleteSubKeyTree(sKey, throwOnMissingSubKey: false);
            Console.WriteLine($"Successfully deleted key: {fullKeyPath}");
        }
        else
        {
            Console.WriteLine($"Key does not exist: {fullKeyPath}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error deleting key: {fullKeyPath}\n{ex.Message}");
}
}
public static void GPOL(string fix)
{
string filePath = null;

if (Regex.IsMatch(fix, @"GroupPolicy(|Scripts|\\User|-Firefox):", RegexOptions.IgnoreCase))
{
    if (Regex.IsMatch(fix, @"GroupPolicy(|Scripts):", RegexOptions.IgnoreCase))
    {
        filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"GroupPolicy\Machine");
    }
    else if (Regex.IsMatch(fix, @"(?i)(GroupPolicy|GroupPolicyScripts)\\User:"))
    {
        filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"GroupPolicy\User");
    }
    else if (Regex.IsMatch(fix, @"fox:", RegexOptions.IgnoreCase))
    {
        filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\distribution\policies.json");
        MoveFile(filePath);
        return;
    }
}
else
{
    string userSid = Regex.Replace(fix, @"(?i)GroupPolicyUsers\\(.+?): .+", "$1");
    filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), $@"GroupPolicyUsers\{userSid}");
}

if (!File.Exists(filePath))
{
    NotFound(filePath);
}
else
{
    MoveDirectory(filePath);
    File.WriteAllText(Path.Combine(@"C:\FRST\re"), "P");
}

string gptIniPath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "GPT.ini");
if (File.Exists(gptIniPath1))
{
    MoveFile(gptIniPath1);
}

string gptIniPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"GroupPolicy\GPT.ini");
if (File.Exists(gptIniPath2))
{
    MoveFile(gptIniPath2);
}
}

private static void MoveFile(string filePath)
{
if (File.Exists(filePath))
{
    string destination = Path.Combine(@"C:\DestinationPath", Path.GetFileName(filePath));
    Console.WriteLine($"Moving file from {filePath} to {destination}");
    try
    {
        // Implement actual file move logic here
        File.Move(filePath, destination);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error moving file: {ex.Message}");
    }
}
else
{
    Console.WriteLine($"File not found: {filePath}");
}
}

private static void MoveDirectory(string directoryPath)
{
Console.WriteLine($"Move directory: {directoryPath}");
// Implement directory move logic
}

private static void NotFound(string path)
{
Console.WriteLine($"File or directory not found: {path}");
}
public static void NTPOL(string fix)
{
// Extract the file path using regex
string sPath = Regex.Replace(fix, @"(?i)Policies: (.+NTUSER\.POL):.+", "$1");

// Move the file
MoveFile(sPath);
}
public static void GPPolicy(string hive1)
{
List<string> arrayReg = new List<string>();
string lrChar = " *‮*";
Match match = Regex.Match(lrChar, @"\*(.)\*");
if (match.Success)
{
    lrChar = match.Groups[1].Value;
}

string keyPath = $@"{hive1}\SOFTWARE\Policies\Microsoft\Windows\safer\codeidentifiers";

using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
{
    if (hKey == null) return;

    int i = 0;
    while (true)
    {
        string subKey1 = RegistrySubKeyHandler.EnumerateSubKey(hKey, i);
        if (string.IsNullOrEmpty(subKey1)) break;

        using (RegistryKey hKey1 = hKey.OpenSubKey($@"{subKey1}\Paths"))
        {
            if (hKey1 == null) continue;

            int s = 0;
            while (true)
            {
                string subKey2 = RegistrySubKeyHandler.EnumerateSubKey(hKey1, s);
                if (string.IsNullOrEmpty(subKey2)) break;

                object saferFlags = hKey1.OpenSubKey(subKey2)?.GetValue("SaferFlags");
                if (saferFlags == null || (int)saferFlags == 0)
                {
                    string paths = hKey1.OpenSubKey(subKey2)?.GetValue("ItemData") as string;

                    if (!string.IsNullOrEmpty(paths) && !paths.Contains("Shell Folders\\Cache%OLK*"))
                    {
                        paths = Regex.Replace(paths, lrChar, "");
                        arrayReg.Add($"{hive1} Group Policy restriction on software: {paths} <==== {DateTime.Now}");
                    }
                }

                s++;
            }
        }

        i++;
    }
}

foreach (var entry in arrayReg)
{
    Console.WriteLine(entry);
}
}
public static void GPPolicyFix(string fix)
{
string hive, key;
bool isHKLM = fix.Contains("HKLM ");

if (isHKLM)
{
    hive = "HKLM";
}
else
{
    string user = Regex.Replace(fix, @"(?i)HKU\\(.+?) Group Policy.+", "$1");
    hive = $@"HKU\{user}";
}

key = $@"{hive}\SOFTWARE\Policies\Microsoft\Windows\safer\codeidentifiers";

string secDes = GetSecurityDescriptor(key, 4);
if (secDes.Contains("(D;") ||
    (!Regex.IsMatch(secDes, @"A;.*?(GA|FA);;;BA") && !secDes.Contains("(A;;FA;;;WD)")))
{
    GrantAccess(key, 4, 1);
}

int ret = DeleteRegistryKey(key);

switch (ret)
{
    case 1:
        int ret1 = WriteRegistryKey(key, "authenticodeenabled", RegistryValueKind.DWord, 0);
        if (ret1 == 1)
        {
            FileWrite("HFixLog.txt", $"{fix} => Restored");
        }
        else
        {
            Deleted(fix);
        }
        break;

    case 0:
        NotFound(fix);
        break;

    case 2:
        NotDeleted(fix);
        break;
}
}

// Helper Methods

private static string GetSecurityDescriptor(string keyPath, int accessLevel)
{
// Simulate retrieving security descriptor
return "SampleSecurityDescriptor";
}

private static void GrantAccess(string keyPath, int accessLevel, int flag)
{
// Simulate granting access to the registry key
Console.WriteLine($"Granted access to {keyPath} with level {accessLevel} and flag {flag}");
}

private static int DeleteRegistryKey(string keyPath)
{
try
{
    using (RegistryKey baseKey = GetBaseKey(keyPath, out string subKeyPath))
    {
        baseKey?.DeleteSubKeyTree(subKeyPath, throwOnMissingSubKey: false);
        return 1; // Success
    }
}
catch (UnauthorizedAccessException)
{
    return 2; // Access denied
}
catch
{
    return 0; // Key not found
}
}

private static int WriteRegistryKey(string keyPath, string valueName, RegistryValueKind valueKind, object value)
{
try
{
    using (RegistryKey baseKey = GetBaseKey(keyPath, out string subKeyPath))
    {
        using (RegistryKey subKey = baseKey?.CreateSubKey(subKeyPath))
        {
            subKey?.SetValue(valueName, value, valueKind);
            return 1; // Success
        }
    }
}
catch
{
    return 0; // Failed to write
}
}

private static RegistryKey GetBaseKey(string keyPath, out string subKeyPath)
{
if (keyPath.StartsWith("HKLM"))
{
    subKeyPath = keyPath.Substring(5); // Remove "HKLM\"
    return Microsoft.Win32.Registry.LocalMachine;
}
if (keyPath.StartsWith("HKU"))
{
    subKeyPath = keyPath.Substring(4); // Remove "HKU\"
    return Microsoft.Win32.Registry.Users;
}

subKeyPath = null;
return null;
}

private static void FileWrite(string filePath, string content)
{
Console.WriteLine($"Writing to {filePath}: {content}");
// Simulate writing to a file
}

private static void Deleted(string fix)
{
Console.WriteLine($"Deleted: {fix}");
}

private static void NotDeleted(string fix)
{
Console.WriteLine($"Not deleted: {fix}");

}
public static void Handler(string handlerFilter)
{
string key = $@"HKCR\PROTOCOLS\{handlerFilter}";
Console.WriteLine($"Scanning Internet: {key}");

using (RegistryKey hKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"PROTOCOLS\{handlerFilter}"))
{
    if (hKey == null) return;

    string tempFile = Path.Combine(Path.GetTempPath(), "hndlr0");
    using (StreamWriter writer = new StreamWriter(tempFile))
    {
        int i = 0;
        while (true)
        {
            string subKey = RegistrySubKeyHandler.EnumerateSubKey(hKey, i);
            if (string.IsNullOrEmpty(subKey)) break;

            using (RegistryKey subKeyHandle = hKey.OpenSubKey(subKey))
            {
                if (subKeyHandle == null || subKeyHandle.SubKeyCount == 0)
                {
                    string clsid = subKeyHandle?.GetValue("clsid") as string ?? "REGIST7";
                    string filePath = null;
                    string company = null;
                    string creationDate = null;

                    if (!string.IsNullOrEmpty(clsid))
                    {
                        filePath = RegistryValueHandler.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{clsid}\InprocServer32", "", null) as string;

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            if (filePath.Contains("mscoree.dll"))
                            {
                                MscoreeHandler($"HKCR\\CLSID\\{clsid}\\InprocServer32", filePath);
                            }

                            if (!File.Exists(filePath))
                            {
                                filePath += " REGIST8";
                            }
                            else
                            {
                                creationDate = $" [{File.GetCreationTime(filePath):yyyy-MM-dd}]";
                            }
                        }
                    }

                    writer.WriteLine($"{handlerFilter}: {subKey} - {clsid} - {filePath}{creationDate}{company}");
                }
                else
                {
                    HandlerSub(key, subKey, handlerFilter, writer);
                }
            }

            i++;
        }
    }

    string regexContent = File.ReadAllText(tempFile);

    if (IsCheckboxChecked(11))
    {
        regexContent = ApplyFilters(regexContent);
    }

    File.WriteAllText("HAddition.txt", regexContent);
    File.Delete(tempFile);
}
}

public static void HandlerFix(string fix)
{
string sKey = Regex.Replace(fix, @"Handler: ([^\{]+?) - .+", "$1");
string key = $@"HKLM\Software\Classes\PROTOCOLS\Handler\{sKey}";

DeleteRegistryKey(key);

if (Regex.IsMatch(fix, @"\{.+\}"))
{
    string clsid = Regex.Replace(fix, @"[^\{]+(\{.+\}).*", "$1");
    key = $@"HKLM\Software\Classes\CLSID\{clsid}";

    if (RegistryKeyExists(key))
    {
        DeleteRegistryKey(key);
    }
}
}

private static bool RegistryKeyExists(string keyPath)
{
using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
{
    return key != null;
}
}

private static void MscoreeHandler(string regPath, string filePath)
{
Console.WriteLine($"Handling MSCOREE: {regPath}, {filePath}");
}

private static string ApplyFilters(string content)
{
content = Regex.Replace(content, @"(?i)Handler: (about|javascript|mailto|res|vbscript) .+\\system32\\mshtml.dll \[.+\] \(Microsoft.+\)\v{2}", "");
// Add additional filters here...
return content;
}
public static void HandlerSub(string key, string subKey, string handlerFilter, StreamWriter writer)
{
using (RegistryKey hKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{key}\{subKey}"))
{
    if (hKey == null) return;

    int j = 0;
    while (true)
    {
        string subKey1 = RegistrySubKeyHandler.EnumerateSubKey(hKey, j);
        if (string.IsNullOrEmpty(subKey1)) break;

        string clsid = hKey.OpenSubKey(subKey1)?.GetValue("clsid") as string;

        if (string.IsNullOrEmpty(clsid))
        {
            clsid = "REGIST7"; // Default value if clsid is missing
            writer.WriteLine($"{handlerFilter}: {subKey} - {clsid}");
        }
        else
        {
            string filePath = RegistryValueHandler.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{clsid}\InprocServer32", "", null) as string;

            if (!string.IsNullOrEmpty(filePath) && filePath.Contains("mscoree.dll"))
            {
                MscoreeHandler($@"HKCR\CLSID\{clsid}\InprocServer32", filePath);
            }

            string finalFilePath = filePath ?? "REGIST8";
            if (!string.IsNullOrEmpty(finalFilePath) && File.Exists(finalFilePath))
            {
                // If file exists, use its actual path
                finalFilePath = filePath;
            }

            writer.WriteLine($"{handlerFilter}: {subKey}\\{subKey1} - {clsid} - {finalFilePath} [Date] [Company]");
        }

        j++;
    }
}
}
private static List<string> arrayReg = new List<string>();

public static void HKLM()
{
// Example placeholder constants
string software = @"SOFTWARE";
string bootMode = "Normal"; // Replace with actual boot mode retrieval logic
string userInitDefaultPath = @"C:\Windows\system32\userinit.exe";
int osNum = 6; // Replace with actual OS version logic
string updateInfo = "<==== UpdateInfo"; // Placeholder for the update information

// Group Policy Handling
GPPolicy("HKLM");
if (bootMode != "Recovery")
{
    // Placeholder for HKU UserReg handling
    foreach (string userReg in GetUserRegistryKeys())
    {
        GPPolicy($"HKU\\{userReg}");
    }
}

string winlogonKeyPath = $@"HKLM\{software}\Microsoft\Windows NT\CurrentVersion\Winlogon";
Console.WriteLine($"Processing Winlogon Key: {winlogonKeyPath}");

// Handle Userinit
string userInitValue = ReadRegistry(winlogonKeyPath, "Userinit");
HandleRegistryEntry(winlogonKeyPath, "Userinit", userInitValue, userInitDefaultPath, updateInfo);

// Handle Shell
string shellValue = ReadRegistry(winlogonKeyPath, "Shell");
HandleRegistryEntry(winlogonKeyPath, "Shell", shellValue, "explorer.exe", updateInfo);

// Additional Winlogon Keys
ProcessWinlogonSubKeys(winlogonKeyPath, new[] { "System", "UIHost", "Taskman" });
ProcessWinlogonLegalNotice(winlogonKeyPath, "LegalNoticeCaption");
ProcessWinlogonLegalNotice(winlogonKeyPath, "LegalNoticeText");

// Handle Notify Key
if (osNum < 6)
{
    string notifyKeyPath = $@"{winlogonKeyPath}\Notify";
    ProcessWinlogonNotifyKeys(notifyKeyPath);
}

// Handle CLSID Keys
ProcessCLSIDKeys(software, updateInfo);

// Example additional logic for specific keys
string specificKey = $@"HKLM\{software}\Classes\CLSID\{{7986d495-ce42-4926-8afc-26dfa299cadb}}\InprocServer32";
string specificValue = ReadRegistry(specificKey, "");
if (!string.IsNullOrEmpty(specificValue) && !specificValue.Contains("authui.dll"))
{
    AddToArrayReg($"HKLM\\...26dfa299cadb\\InprocServer32: [Authentication UI Logon UI] {specificValue} {updateInfo}");
}
}

private static string ReadRegistry(string keyPath, string valueName)
{
try
{
    return (string)RegistryValueHandler.GetValue(keyPath, valueName, null);
}
catch
{
    return null;
}
}

private static void HandleRegistryEntry(string keyPath, string valueName, string value, string defaultValue, string updateInfo)
{
if (string.IsNullOrEmpty(value))
{
    AddToArrayReg($"{keyPath}: [{valueName}] {defaultValue} {updateInfo}");
}
else if (value.IndexOf(defaultValue, StringComparison.OrdinalIgnoreCase) >= 0)
{
    AddToArrayReg($"{keyPath}: [{valueName}] {value} <=== {updateInfo}");
}
}

private static void ProcessWinlogonSubKeys(string keyPath, string[] subKeys)
{
foreach (string subKey in subKeys)
{
    string value = ReadRegistry(keyPath, subKey);
    if (!string.IsNullOrEmpty(value))
    {
        Console.WriteLine($"Processed Winlogon SubKey: {subKey}");
    }
}
}

private static void ProcessWinlogonLegalNotice(string keyPath, string valueName)
{
string value = ReadRegistry(keyPath, valueName);
if (!string.IsNullOrEmpty(value))
{
    AddToArrayReg($"{keyPath}: [{valueName}] {value}");
}
}

private static void ProcessWinlogonNotifyKeys(string notifyKeyPath)
{
using (RegistryKey notifyKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(notifyKeyPath))
{
    if (notifyKey == null) return;

    foreach (string subKey in notifyKey.GetSubKeyNames())
    {
        string dllName = ReadRegistry($@"{notifyKeyPath}\{subKey}", "DLLName");
        if (string.IsNullOrEmpty(dllName))
        {
            AddToArrayReg($"Winlogon\\Notify\\{subKey}: ");
        }
        else if (!File.Exists(dllName))
        {
            AddToArrayReg($"Winlogon\\Notify\\{subKey}: {dllName} [X]");
        }
        else
        {
            string version = FileVersionInfo.GetVersionInfo(dllName)?.CompanyName ?? "Unknown";
            AddToArrayReg($"Winlogon\\Notify\\{subKey}: {dllName} [Date] ({version})");
        }
    }
}
}

private static void ProcessCLSIDKeys(string software, string updateInfo)
{
string clsidKeyPath = $@"HKLM\{software}\Classes\CLSID\{{F3130CDB-AA52-4C3A-AB32-85FFC23AF9C1}}\InprocServer32";
string clsidValue = ReadRegistry(clsidKeyPath, "");
if (!string.IsNullOrEmpty(clsidValue) &&
    !clsidValue.ToLowerInvariant().Contains("wbem\\wbemess.dll".ToLowerInvariant()))
{
    AddToArrayReg($"HKLM\\...\\InprocServer32: [Default-wbemess] {clsidValue} <==== {updateInfo}");
}
}

private static void AddToArrayReg(string entry)
{
arrayReg.Add(entry);
Console.WriteLine($"Added to arrayReg: {entry}");
}

public static void HKU()
{
string label = "Registry Scan: Starting HKU processing";
Console.WriteLine(label);

string bootMode = "Normal"; // Replace with the actual boot mode logic
string[] allUsers = GetAllUsers(); // Replace with actual logic to get all user profiles

if (bootMode == "Recovery")
{
    foreach (string userPath in allUsers)
    {
        string user = ExtractUserFromPath(userPath);
        LoadRegistryHive(user, userPath);
        ProcessHKU(user);
        UnloadRegistryHive(user);
    }
}
else
{
    int index = 0;
    while (true)
    {
        string user = EnumerateRegistryKey(Microsoft.Win32.Registry.Users, index);
        if (string.IsNullOrEmpty(user)) break;

        if (!Regex.IsMatch(user, @"(?i)(\.default|_Classes)"))
        {
            ProcessHKU(user);
        }

        index++;
    }
}
}

private static string[] GetAllUsers()
{
// Placeholder: Replace with logic to get all user profiles
return new[] { @"C:\Users\User1", @"C:\Users\User2" };
}

private static string ExtractUserFromPath(string path)
{
// Extract the user from the path using regex logic
return Path.GetFileName(path);
}

private static void LoadRegistryHive(string user, string userPath)
{
string command = $@"reg load HKU\{user} ""{userPath}\ntuser.dat""";
ExecuteCommand(command);
}

private static void UnloadRegistryHive(string user)
{
string command = $@"reg unload HKU\{user}";
ExecuteCommand(command);
}

private static void ExecuteCommand(string command)
{
try
{
    Process process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            CreateNoWindow = true,
            UseShellExecute = false
        }
    };

    process.Start();
    process.WaitForExit();
}
catch (Exception ex)
{
    Console.WriteLine($"Error executing command '{command}': {ex.Message}");
}
}

private static string EnumerateRegistryKey(RegistryKey rootKey, int index)
{
try
{
    return rootKey.GetSubKeyNames()[index];
}
catch
{
    return null;
}
}

private static void ProcessHKU(string user)
{
Console.WriteLine($"Processing HKU for user: {user}");
// Add logic to process HKU for the user
}

public static void HKURun(string user, string run)
{
string fullKey = $@"HKU\{user}\Software\Microsoft\Windows\CurrentVersion\{run}";
using (RegistryKey hKey = Microsoft.Win32.Registry.Users.OpenSubKey(fullKey))
{
    if (hKey == null) return;

    string[][] arrayName = ListValues(hKey);
    if (arrayName == null || arrayName.Length == 0) return;

    foreach (var value in arrayName)
    {
        string valName = value[0];
        string filePath = value[1];
        string attention = string.Empty;

        if (Regex.IsMatch(valName, @"\w+\d$") &&
            Regex.IsMatch(filePath, $@"(?i)\\AppData\\Local\\{valName}\\{valName}\.exe"))
        {
            attention = " <==== UPDATED";
        }
        else if (filePath.Contains(valName) &&
                 Regex.IsMatch(filePath, @"(?i)\\AppData") &&
                 Regex.IsMatch(filePath, @"(?i)(REGSVR32|rundll32)"))
        {
            attention = " <==== UPDATED";
        }
        else if (filePath.Contains(@"\Temp\") &&
                 !Regex.IsMatch(filePath, @"(?i)\\(spool|razer)"))
        {
            attention = " <==== UPDATED";
        }
        else if (filePath.Contains(@"\ctfmon.exe") &&
                 !Regex.IsMatch(filePath, @"(?i):\\Windows\\System32"))
        {
            attention = " <==== UPDATED";
        }
        else if (valName == "CMD" && Regex.IsMatch(filePath, @"(?i)HTTP:"))
        {
            attention = " <==== UPDATED";
        }
        else if (Regex.IsMatch(valName, @"(?i)^(Cortana|AntiMalwareServiceExecutable|WmiPrvSE|MpCmdRun|SecurityHealthSystray|audiodg|WindowsUpdate|spoolsv|uerinit|explorer|iexplore|System|regdrv|taskhost|Isass|smss|csrss|wininit|services|lsass|lsm|winlogon|.*svchost.*|dwm|msdtc|VSSVC|alg|sihost|dllhost|cmd|msedge|conhost|fontdrvhost|SystemSettingsBroker|SystemSettings|SearchIndexer)(|\.exe)$"))
        {
            attention = " <==== UPDATED";
        }
        // Add more conditions here based on the original script.

        if (File.Exists(filePath))
        {
            AddToArrayReg($"HKU\\{user}\\...\\{run}: [{valName}] => {filePath} {attention}");
        }
        else
        {
            string noFile = " (NOT FOUND)";
            AddToArrayReg($"HKU\\{user}\\...\\{run}: [{valName}] => {filePath}{noFile}{attention}");
        }
    }
}

// Check for invalid subkeys
if (run == "Run")
{
    string invalidKeyName = FindInvalidSubkey(fullKey);
    if (!string.IsNullOrEmpty(invalidKeyName))
    {
        AddToArrayReg($"InvalidSubkeyName: [HKU\\{user}\\Software\\Microsoft\\Windows\\CurrentVersion\\Run\\{invalidKeyName}] <==== UPDATED");
    }
}
}

private static string[][] ListValues(RegistryKey key)
{
List<string[]> values = new List<string[]>();
foreach (string valueName in key.GetValueNames())
{
    string valueData = key.GetValue(valueName)?.ToString();
    if (!string.IsNullOrEmpty(valueData))
    {
        values.Add(new[] { valueName, valueData });
    }
}
return values.ToArray();
}

private static string FindInvalidSubkey(string fullKey)
{
// Placeholder: Logic to find invalid subkeys based on fullKey
return string.Empty;
}

public static List<string> HKUUsers()
{
List<string> hkuUsers = new List<string>();
try
{
    using (RegistryKey hku = Microsoft.Win32.Registry.Users)
    {
        foreach (string subKeyName in hku.GetSubKeyNames())
        {
            // Exclude keys matching the regex pattern
            if (!System.Text.RegularExpressions.Regex.IsMatch(subKeyName, @"(?i)(^S-1-5-18|_Classes)"))
            {
                hkuUsers.Add(subKeyName);
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error enumerating HKU keys: {ex.Message}");
}

return hkuUsers;
}

public static void SDELETE()
{
// Display a message box (equivalent to AutoIt's MsgBox)
string message = $"FRST {DELETED}\n\n{REBOOT2}\n{REBOOT3}";
System.Windows.Forms.MessageBox.Show(message, "FRST", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

// Generate a random number
Random random = new Random();
int rand = random.Next(100, 1000);

// Move the current script to a temporary location
string scriptDir = AppDomain.CurrentDomain.BaseDirectory;
string scriptName = AppDomain.CurrentDomain.FriendlyName;
string tempDir = Path.GetTempPath();
string tempFile = Path.Combine(tempDir, $"FRST{rand}.TEMP");

FileUtils.MoveFile(Path.Combine(scriptDir, scriptName), tempFile, true);

// Check if the FRST directory exists and write to the registry if it does
string frstPath = Path.Combine(C, "frst");
if (Directory.Exists(frstPath))
{
    RegistryValueHandler.SetRegistryValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce", "*Removed", $"cmd /c rd /q /s \"{frstPath}\"", RegistryValueKind.String);
}

// Write the command to delete the temporary file to the registry
RegistryValueHandler.SetRegistryValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce", "*DelTemp", $"cmd /c DEL /F /Q /A \"{tempFile}\"", RegistryValueKind.String);

// Shutdown the system
Process.Start("shutdown", "/r /t 0");

// Exit the application
Environment.Exit(0);
}

// Dummy placeholders for variables in the AutoIt script
private static readonly string DELETED = "deleted"; // Replace with the appropriate value
private static readonly string REBOOT2 = "reboot message 2"; // Replace with the appropriate value
private static readonly string REBOOT3 = "reboot message 3"; // Replace with the appropriate value
private static readonly string C = @"C:\"; // Replace with the appropriate drive

public static bool ListToMask(ref string mask, List<string> list)
{
if (list == null || list.Count == 0)
{
    mask = string.Empty;
    return false; // Indicate failure due to empty or null list
}

try
{
    // Escape special characters and join with "|"
    mask = string.Join("|", list.Select(item => Regex.Escape(item)));
    return true; // Indicate success
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating regex mask: {ex.Message}");
    mask = string.Empty;
    return false; // Indicate failure
}
}

public static void FatalExitWithMessage(int exitCode, string message)
{
try
{
    // Log the error message to a file
    string logFilePath = "error.log"; // You can specify the desired log file path
    File.AppendAllText(logFilePath, $"{DateTime.Now}: Fatal Error - {message}{Environment.NewLine}");

    // Optionally display the error message
    Console.Error.WriteLine($"Fatal Error: {message}");

    // Exit the application with the specified exit code
    Environment.Exit(exitCode);
}
catch (Exception ex)
{
    // Handle any exceptions during the logging or exiting process
    Console.Error.WriteLine($"Unexpected error in FatalExitWithMessage: {ex.Message}");
    Environment.Exit(exitCode);
}
}

public static Guid GuidFromString(string guidString)
{
Guid guid = Guid.Empty;

// Convert the string to a GUID using the WinAPI CLSIDFromString function.
int result = CLSIDFromString(guidString, ref guid);
if (result != 0)
{
    throw new ArgumentException($"Failed to convert string to GUID: {guidString}", nameof(guidString));
}

return guid;
}

/// <summary>
/// P/Invoke declaration for CLSIDFromString from the OLE32 library.
/// </summary>
/// <param name="guidString">The string representation of the GUID.</param>
/// <param name="guid">The output GUID object.</param>
/// <returns>An HRESULT indicating success or failure.</returns>
[DllImport("ole32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern int CLSIDFromString([MarshalAs(UnmanagedType.LPWStr)] string guidString, ref Guid guid);

[DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern uint GetFileVersionInfoSize(string lptstrFilename, out uint lpdwHandle);

// Import GetFileVersionInfo from version.dll
[DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern bool GetFileVersionInfo(string lptstrFilename, uint dwHandle, uint dwLen, IntPtr lpData);

public static uint GetFileVersionInfo(string filePath, out IntPtr buffer)
{
buffer = IntPtr.Zero;

// Get the size of the version information
uint handle;
uint size = GetFileVersionInfoSize(filePath, out handle);
if (size == 0)
{
    Console.WriteLine($"Error getting file version info size. Error code: {Marshal.GetLastWin32Error()}");
    return 0;
}

// Allocate memory for the version info
buffer = Marshal.AllocHGlobal((int)size);

// Retrieve the version information
if (!GetFileVersionInfo(filePath, handle, size, buffer))
{
    Console.WriteLine($"Error getting file version info. Error code: {Marshal.GetLastWin32Error()}");
    Marshal.FreeHGlobal(buffer);
    buffer = IntPtr.Zero;
    return 0;
}

return size;
}

[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern int LoadString(
IntPtr hInstance,
uint uID,
StringBuilder lpBuffer,
int nBufferMax);

// Wrapper method for LoadString
public static string LoadString(IntPtr hInstance, uint resourceID)
{
const int bufferSize = 1024; // Maximum buffer size
StringBuilder buffer = new StringBuilder(bufferSize);

// Call LoadString
int result = LoadString(hInstance, resourceID, buffer, bufferSize);
if (result > 0)
{
    return buffer.ToString();
}
else
{
    int errorCode = Marshal.GetLastWin32Error();
    Console.WriteLine($"Failed to load string resource. Error code: {errorCode}");
    return null;
}
}

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

// Import FreeLibrary from kernel32.dll
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool FreeLibrary(IntPtr hModule);

// Flags for LoadLibraryEx
public const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;
public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
public const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

// Wrapper for LoadLibraryEx
public static IntPtr LoadLibraryEx(string libraryPath, uint flags = 0)
{
IntPtr libraryHandle = LoadLibraryEx(libraryPath, IntPtr.Zero, flags);

if (libraryHandle == IntPtr.Zero)
{
    int errorCode = Marshal.GetLastWin32Error();
    Console.WriteLine($"Failed to load library: {libraryPath}. Error code: {errorCode}");
}

return libraryHandle;
}

// Wrapper to free a loaded library
public static void FreeLibraryHandle(IntPtr libraryHandle)
{
if (libraryHandle != IntPtr.Zero)
{
    if (!FreeLibrary(libraryHandle))
    {
        int errorCode = Marshal.GetLastWin32Error();
        Console.WriteLine($"Failed to free library handle. Error code: {errorCode}");
    }
    else
    {
        Console.WriteLine("Library handle freed successfully.");
    }
}
}

[DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern bool VerQueryValue(
IntPtr pBlock,
string lpSubBlock,
out IntPtr lplpBuffer,
out uint puLen
);

public static string[][] VerQueryValue(IntPtr versionInfoData)
{
if (versionInfoData == IntPtr.Zero)
    throw new ArgumentNullException(nameof(versionInfoData), "Version info data cannot be null.");

// Query \VarFileInfo\Translation for language and codepage
IntPtr translationPtr;
uint translationSize;
if (!VerQueryValue(versionInfoData, @"\VarFileInfo\Translation", out translationPtr, out translationSize))
{
    Console.WriteLine("Failed to query translation block.");
    return Array.Empty<string[]>();
}

int translationCount = (int)translationSize / 4; // Each entry is 4 bytes (2 for language, 2 for codepage)
var translations = new List<string[]>();

for (int i = 0; i < translationCount; i++)
{
    ushort langId = (ushort)Marshal.ReadInt16(translationPtr, i * 4);
    ushort codePage = (ushort)Marshal.ReadInt16(translationPtr, i * 4 + 2);

    string translation = $"{langId:X4}{codePage:X4}";
    Console.WriteLine($"Found Translation: {translation}");

    // Query string values for this language and codepage
    var stringValues = new List<string> { translation };
    foreach (var key in new[] { "ProductName", "FileDescription", "CompanyName", "FileVersion" })
    {
        string subBlock = $@"\StringFileInfo\{translation}\{key}";
        if (VerQueryValue(versionInfoData, subBlock, out IntPtr valuePtr, out uint valueLen) && valueLen > 0)
        {
            string value = Marshal.PtrToStringUni(valuePtr);
            stringValues.Add(value);
        }
        else
        {
            stringValues.Add(null); // Add null if the key is not available
        }
    }

    translations.Add(stringValues.ToArray());
}

return translations.ToArray();
}
}
}
/*/
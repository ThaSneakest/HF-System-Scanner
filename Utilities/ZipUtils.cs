using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Scripting;

namespace Wildlands_System_Scanner
{
    public class ZipUtils
    {
        public static void ZIP(string fix, ref string resultLog)
        {
            resultLog += "================== Zip: ===================\n";
            string pathsAll = Regex.Replace(fix, "(?i)zip:\\s*(.+)", "$1");
            string[] paths = pathsAll.Split(';');

            if (paths.Length == 0)
            {
                resultLog += "Error reading paths\n";
                return;
            }

            string zipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), DateTime.Now.ToString("MMddyyyy_HHmmss") + ".zip");
            string zipTemp = Path.Combine(Path.GetTempPath(), "zip" + DateTime.Now.ToString("MMddyyyy_HHmmss"));
            Directory.CreateDirectory(zipTemp);

            string pTemp = Path.Combine("C:\\FRST", "tempzip");
            File.Create(pTemp).Dispose();

            if (!ZIP_CREATE(zipPath, true))
            {
                resultLog += "Error creating zip folder\n\n";
                return;
            }

            foreach (var path in paths)
            {
                string cleanedPath = path.Trim();

                if (File.Exists(cleanedPath))
                {
                    string fileName = Regex.Replace(cleanedPath, ".+\\\\(.+)", "|$1|");
                    if ((new FileInfo(cleanedPath).Attributes & FileAttributes.Directory) == 0) // If not directory
                    {
                        if (!FileFix.CopyFile(cleanedPath, zipTemp))
                        {
                            resultLog += $"{cleanedPath} -> NCOPY\n";
                        }
                        else
                        {
                            resultLog += $"{cleanedPath} -> COP {zipPath}\n";
                        }
                    }
                    else // Directory
                    {
                        if (!DirectoryFix.CopyDirectory(cleanedPath, zipTemp))
                        {
                            resultLog += $"{cleanedPath} -> NCOPY\n";
                        }
                    }
                }
                else
                {
                    resultLog += $"{cleanedPath} -> NOT FOUND\n";
                    if (paths.Length == 2)
                    {
                        File.Delete(zipPath);
                    }
                }
            }

            File.Delete(pTemp);
            Directory.Delete(zipTemp, true);
            resultLog += "=========== Zip: END ==========\n";
        }

        // Simulates the creation of a ZIP file. This function will return true if successful, false otherwise.
        public static bool ZIP_CREATE(string zipPath, bool overwrite)
        {
            try
            {
                if (File.Exists(zipPath) && !overwrite)
                    return false;

                using (FileStream fs = new FileStream(zipPath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    // Create an empty archive (if needed).
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Simulates adding an item to the ZIP file.
        public static bool _ZIP_ADDITEM(string zipPath, string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(zipPath, FileMode.OpenOrCreate))
                using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Verifies the digital signature of a file and retrieves signer information.
        /// </summary>
        public static string[] GetSignatureInfo(string filePath, int codePage = 0)
        {
            string[] emptyAnswer = { "", "", "" };
            string[] certInfo = { "", "", "" };

            IntPtr filePathPtr = MultiByteToWideChar(filePath, codePage, true);
            if (filePathPtr == IntPtr.Zero)
                return emptyAnswer;

            var result = Crypt32NativeMethods.CryptQueryObject(1, filePath, 1024, 2, 0, out _, out _, out _, out var hStore, out var hMsg);
            if (!result || hMsg == IntPtr.Zero)
                return emptyAnswer;

            var signerInfoSize = GetSignerInfoSize(hMsg);
            if (signerInfoSize == 0)
                return emptyAnswer;

            var signerInfo = GetSignerInfo(hMsg);
            if (signerInfo == null)
                return emptyAnswer;

            var signerInfoStruct = Marshal.PtrToStructure<Structs.CMSG_SIGNER_INFO>(Marshal.UnsafeAddrOfPinnedArrayElement(signerInfo, 0));
            var certInfoStruct = new Structs.CERT_INFO
            {
                Issuer_cbData = signerInfoStruct.Issuer_cbData,
                Issuer_pbData = signerInfoStruct.Issuer_pbData,
                SerialNumber_cbData = signerInfoStruct.SerialNumber_cbData,
                SerialNumber_pbData = signerInfoStruct.SerialNumber_pbData
            };

            var certContext = Crypt32NativeMethods.CertFindCertificateInStore(hStore, 1 | 65536, 0, 720896, certInfoStruct);
            if (certContext == IntPtr.Zero)
                return emptyAnswer;

            string signerName = GetCertName(certContext);
            if (string.IsNullOrEmpty(signerName))
                return emptyAnswer;

            certInfo[1] = signerName;
            return certInfo;
        }

        /// <summary>
        /// Adds an item to a ZIP file.
        /// </summary>
        public static void AddItemToZip(string zipFilePath, string itemPath, string destinationDir = "")
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException("ZIP file not found.", zipFilePath);

            if (!File.Exists(itemPath))
                throw new FileNotFoundException("Item file not found.", itemPath);

            string entryName = string.IsNullOrEmpty(destinationDir)
                ? Path.GetFileName(itemPath)
                : Path.Combine(destinationDir, Path.GetFileName(itemPath)).Replace('\\', '/');

            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
            {
                if (archive.GetEntry(entryName) != null)
                    throw new InvalidOperationException($"Entry '{entryName}' already exists in the ZIP file.");

                archive.CreateEntryFromFile(itemPath, entryName);
            }
        }

        /// <summary>
        /// Retrieves the signer name from a certificate context.
        /// </summary>
        private static string GetCertName(IntPtr certContext)
        {
            int size = Crypt32NativeMethods.CertGetNameString(certContext, 4, 0, IntPtr.Zero, null, 0);
            if (size == 0)
                return null;

            var buffer = new char[size];
            Crypt32NativeMethods.CertGetNameString(certContext, 4, 0, IntPtr.Zero, buffer, size);
            return new string(buffer).TrimEnd('\0');
        }

        /// <summary>
        /// Gets the size of the signer information.
        /// </summary>
        private static int GetSignerInfoSize(IntPtr hMsg)
        {
            uint size = 0;
            if (!Crypt32NativeMethods.CryptMsgGetParam(hMsg, 6, 0, IntPtr.Zero, ref size))
            {
                throw new InvalidOperationException("Failed to get the size of the signer information.");
            }
            return (int)size;
        }


        /// <summary>
        /// Retrieves signer information from the message.
        /// </summary>
        private static byte[] GetSignerInfo(IntPtr hMsg)
        {
            uint signerInfoSize = 0;
            if (!Crypt32NativeMethods.CryptMsgGetParam(hMsg, 6, 0, IntPtr.Zero, ref signerInfoSize))
                return null;

            var signerInfo = new byte[signerInfoSize];
            if (!Crypt32NativeMethods.CryptMsgGetParam(hMsg, 6, 0, signerInfo, ref signerInfoSize))
                return null;

            return signerInfo;
        }

        /// <summary>
        /// Converts a string to a wide character pointer.
        /// </summary>
        private static IntPtr MultiByteToWideChar(string input, int codePage = 0, bool addNullTerminator = true)
        {
            if (string.IsNullOrEmpty(input))
                return IntPtr.Zero;

            int size = (input.Length + (addNullTerminator ? 1 : 0)) * 2;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            for (int i = 0; i < input.Length; i++)
            {
                Marshal.WriteInt16(ptr, i * 2, input[i]);
            }

            if (addNullTerminator)
            {
                Marshal.WriteInt16(ptr, input.Length * 2, 0);
            }

            return ptr;
        }
        public static bool SaveFileToDestinationOnce(string zipFilePath, string fileName, string destinationPath)
        {
            try
            {
                byte[] fileData = SaveFileToBinary(zipFilePath, fileName);

                if (fileData != null && fileData.Length > 0)
                {
                    File.WriteAllBytes(destinationPath, fileData);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return false;
            }
        }

        public static byte[] SaveFileToBinary(string zipFilePath, string fileName)
        {
            try
            {
                using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry(fileName);
                    if (entry == null)
                    {
                        throw new FileNotFoundException($"File '{fileName}' not found in the ZIP archive.");
                    }

                    using (var entryStream = entry.Open())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file from ZIP: {ex.Message}");
                return null;
            }
        }

        public static Stream SaveFileToStream(string zipFilePath, string fileName)
        {
            try
            {
                using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry(fileName);
                    if (entry == null)
                    {
                        throw new FileNotFoundException($"File '{fileName}' not found in the ZIP archive.");
                    }

                    return entry.Open();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving stream from ZIP: {ex.Message}");
                return null;
            }
        }
        public static string ZIP_CREATE(string sFilename, int iOverwrite = 0)
        {
            if (File.Exists(sFilename) && iOverwrite == 0)
                return null; // File exists and overwrite not allowed

            try
            {
                using (var fileStream = File.Create(sFilename))
                {
                    byte[] zipHeader = new byte[]
                    {
                    0x50, 0x4B, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00
                    };
                    fileStream.Write(zipHeader, 0, zipHeader.Length);
                }
                return sFilename;
            }
            catch (Exception)
            {
                return null; // Handle file error
            }
        }

        public static bool ZIP_ITEMEXISTS(string sZipFile, string sItem)
        {
            if (!File.Exists(sZipFile))
                return false;

            try
            {
                using (var archive = ZipFile.OpenRead(sZipFile))
                {
                    var entry = archive.GetEntry(sItem);
                    return entry != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string ZIP_ADDPATH(string sZipFile, string sPath)
        {
            if (!File.Exists(sZipFile))
                return null;

            sPath = ZIP_PATHSTRIPSLASH(sPath);
            if (string.IsNullOrEmpty(sPath))
                return null;

            try
            {
                using (var archive = ZipFile.Open(sZipFile, ZipArchiveMode.Update))
                {
                    var directories = sPath.Split('\\');
                    foreach (var dir in directories)
                    {
                        var entry = archive.GetEntry(dir);
                        if (entry == null)
                        {
                            // Add directory if it doesn't exist
                            archive.CreateEntry(dir + "/");
                        }
                    }
                }
                return sPath;
            }
            catch
            {
                return null;
            }
        }

        public static string ZIP_CREATETEMPDIR()
        {
            string tempName;
            do
            {
                tempName = "";
                var random = new Random();
                while (tempName.Length < 7)
                {
                    tempName += (char)random.Next(97, 123); // Generate random lowercase letters
                }
                tempName = Path.Combine(Path.GetTempPath(), "~" + tempName + ".tmp");
            } while (Directory.Exists(tempName) || File.Exists(tempName));

            try
            {
                Directory.CreateDirectory(tempName);
            }
            catch
            {
                return null; // Equivalent to SetError(1, 0, 0)
            }
            return tempName;
        }

        public static string ZIP_CREATETEMPNAME()
        {
            Guid guid = Guid.NewGuid();
            return Regex.Replace(guid.ToString(), "[}{-]", "");
        }

        public static bool ZIP_DLLCHK()
        {
            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string zipDllPath = Path.Combine(systemDir, "zipfldr.dll");
            if (!File.Exists(zipDllPath))
                return false; // Equivalent to SetError(1, 0, 0)

            // Check registry key
            string regKey = @"HKEY_CLASSES_ROOT\CLSID\{E88DCCE0-B7B3-11d1-A9F0-00AA0060FA31}";
            string regValue = Microsoft.Win32.Registry.GetValue(regKey, "", null) as string;
            if (string.IsNullOrEmpty(regValue))
                return false; // Equivalent to SetError(2, 0, 0)

            return true;
        }

        public static dynamic ZIP_GETNAMESPACE(string zipFile, string path = "")
        {
            if (!ZIP_DLLCHK())
                return null; // Equivalent to SetError(@error, 0, 0)

            if (!ISFULLPATH(zipFile))
                return null; // Equivalent to SetError(3, 0, 0)

            dynamic shellApp = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            dynamic namespaceFolder = shellApp.NameSpace(zipFile);
            if (namespaceFolder == null)
                return null; // Equivalent to SetError(4, 0, 0)

            if (!string.IsNullOrEmpty(path))
            {
                string[] pathParts = path.Split('\\');
                foreach (var part in pathParts)
                {
                    dynamic folderItem = namespaceFolder.ParseName(part);
                    if (folderItem == null)
                        return null; // Equivalent to SetError(5, 0, 0)

                    namespaceFolder = folderItem.GetFolder;
                    if (namespaceFolder == null)
                        return null; // Equivalent to SetError(6, 0, 0)
                }
            }

            return namespaceFolder;
        }

        public static bool ZIP_INTERNALDELETE(string zipFile, string fileName)
        {
            if (!ZIP_DLLCHK())
                return false; // Equivalent to SetError(@error, 0, 0)

            if (!ISFULLPATH(zipFile))
                return false; // Equivalent to SetError(3, 0, 0)

            string path = ZIP_PATHPATHONLY(fileName);
            fileName = ZIP_PATHNAMEONLY(fileName);

            dynamic namespaceFolder = ZIP_GETNAMESPACE(zipFile, path);
            if (namespaceFolder == null)
                return false; // Equivalent to SetError(4, 0, 0)

            dynamic folderItem = namespaceFolder.ParseName(fileName);
            if (folderItem == null)
                return false; // Equivalent to SetError(5, 0, 0)

            string tempDir = ZIP_CREATETEMPDIR();
            if (tempDir == null)
                return false; // Equivalent to SetError(6, 0, 0)

            dynamic shellApp = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            dynamic tempNamespace = shellApp.NameSpace(tempDir);
            tempNamespace.MoveHere(folderItem);

            Directory.Delete(tempDir, true);

            folderItem = namespaceFolder.ParseName(fileName);
            return folderItem == null; // Equivalent to SetError(7, 0, 0)
        }

        public static string ZIP_PATHNAMEONLY(string path)
        {
            return Regex.Replace(path, @".*\\", "");
        }

        public static string ZIP_PATHPATHONLY(string path)
        {
            return Regex.Replace(path, @"^(.*)\\.*?$", "$1");
        }

        public static string ZIP_PATHSTRIPSLASH(string input)
        {
            return Regex.Replace(input, @"(^\\+|\\+$)", "");
        }

        private static bool ISFULLPATH(string path)
        {
            return Path.IsPathRooted(path);
        }

        // Cleans up references if BNORELEASE is false
        public static void Clean(ref object param1, ref object param2, bool noRelease = false)
        {
            if (noRelease)
                return;

            param1 = null;
            param2 = null;
        }

        // Calls DllGetClassObject to get a class object interface
        public static object DllGetClassObject(string dllPath, string clsid, string iid, string interfaceTag = "")
        {
            var classId = GuidFromString(clsid);
            var interfaceId = GuidFromString(iid);
            IntPtr classObjectPointer = IntPtr.Zero;

            int result = DllCall(dllPath, "DllGetClassObject", ref classId, ref interfaceId, out classObjectPointer);
            if (result != 0 || classObjectPointer == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to get class object. Error: {result}");

            var obj = Marshal.GetTypedObjectForIUnknown(classObjectPointer, Type.GetTypeFromProgID(interfaceTag));
            return obj;
        }

        // Parses a display name using SHParseDisplayName
        public static IntPtr ParseDisplayName(string path)
        {
            IntPtr parsedPointer = IntPtr.Zero;
            uint attributes = 0;

            int result = Shell32NativeMethods.SHParseDisplayName(path, IntPtr.Zero, out parsedPointer, 0, ref attributes);
            if (result != 0)
                throw new InvalidOperationException($"Failed to parse display name. Error: {result}");

            return parsedPointer;
        }

        // Frees memory allocated with CoTaskMemAlloc
        public static void CoTaskMemFree(IntPtr memory)
        {
            if (memory != IntPtr.Zero)
            {
                Ole32NativeMethods.CoTaskMemFreeNative(memory);
            }
        }

        // Converts a GUID string into a GUID struct
        public static Guid GuidFromString(string guidString)
        {
            Guid guid;
            int result = Ole32NativeMethods.CLSIDFromString(guidString, out guid);
            if (result != 0)
                throw new InvalidOperationException($"Failed to create GUID from string. Error: {result}");

            return guid;
        }

        // PInvoke declarations
      

        // Helper for calling native DLL functions
        private static int DllCall(string dllPath, string functionName, ref Guid classId, ref Guid interfaceId, out IntPtr outputPointer)
        {
            IntPtr dllHandle = Kernel32NativeMethods.LoadLibrary(dllPath);
            if (dllHandle == IntPtr.Zero)
                throw new DllNotFoundException($"Failed to load DLL: {dllPath}");

            IntPtr functionPointer = Kernel32NativeMethods.GetProcAddress(dllHandle, functionName);
            if (functionPointer == IntPtr.Zero)
            {
                Kernel32NativeMethods.FreeLibrary(dllHandle);
                throw new EntryPointNotFoundException($"Function {functionName} not found in {dllPath}");
            }

            // Create a delegate for the native function
            var functionDelegate = Marshal.GetDelegateForFunctionPointer<Kernel32NativeMethods.NativeDllGetClassObjectDelegate>(functionPointer);
            int result = functionDelegate(ref classId, ref interfaceId, out outputPointer);

            Kernel32NativeMethods.FreeLibrary(dllHandle);
            return result;
        }

        public static string UnzipFile(string zipFilePath, string fileNameToExtract)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                var entry = archive.GetEntry(fileNameToExtract);
                if (entry != null)
                {
                    using (var reader = new StreamReader(entry.Open()))
                    {
                        return reader.ReadToEnd(); // Read and return the file content as a string
                    }
                }
            }
            throw new FileNotFoundException($"The file {fileNameToExtract} was not found in the archive.");
        }

        public static string UnzipAndSaveFileOnce(string zipFilePath, string fileName, string outputDirectory)
        {
            if (!File.Exists(zipFilePath))
            {
                Console.WriteLine($"Zip file not found: {zipFilePath}");
                return null;
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.GetDirectoryName(zipFilePath) ?? string.Empty;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                ZipArchiveEntry entry = archive.GetEntry(fileName);
                if (entry != null)
                {
                    string destinationPath = Path.Combine(outputDirectory, entry.FullName);
                    if (!File.Exists(destinationPath))
                    {
                        entry.ExtractToFile(destinationPath);
                        Console.WriteLine($"Extracted: {destinationPath}");
                        return destinationPath;
                    }
                    else
                    {
                        Console.WriteLine($"File already exists: {destinationPath}");
                        return destinationPath;
                    }
                }
                else
                {
                    Console.WriteLine($"File '{fileName}' not found in the zip archive.");
                    return null;
                }
            }
        }

    }
}

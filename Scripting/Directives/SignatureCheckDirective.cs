using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SignatureCheckDirective
    {
        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, [In] ref WINTRUST_DATA pWVTData);

        private const uint TRUST_E_NOSIGNATURE = 0x800B0100;
        private const uint TRUST_E_EXPLICIT_DISTRUST = 0x800B0111;
        private const uint TRUST_E_SUBJECT_NOT_TRUSTED = 0x800B0004;
        private const uint TRUST_E_PROVIDER_UNKNOWN = 0x800B0001;
        private const uint TRUST_E_ACTION_UNKNOWN = 0x800B0002;
        private const uint TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800B0003;
        private const uint ERROR_SUCCESS = 0x0;

        private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            public IntPtr pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public uint dwUIChoice;
            public uint fdwRevocationChecks;
            public uint dwUnionChoice;
            public IntPtr pFile;
            public uint dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public uint dwProvFlags;
            public uint dwUIContext;
        }

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("SignatureCheck::"))
                    {
                        string targetFilePath = line.Substring("SignatureCheck::".Length).Trim();
                        if (!string.IsNullOrEmpty(targetFilePath))
                        {
                            Console.WriteLine($"SignatureCheck:: directive found. Checking signature for: {targetFilePath}");
                            CheckSignature(targetFilePath);
                        }
                        else
                        {
                            Console.WriteLine("SignatureCheck:: directive found, but no file path was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void CheckSignature(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)),
                pcwszFilePath = Marshal.StringToCoTaskMemUni(filePath),
                hFile = IntPtr.Zero,
                pgKnownSubject = IntPtr.Zero
            };

            WINTRUST_DATA trustData = new WINTRUST_DATA
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA)),
                pPolicyCallbackData = IntPtr.Zero,
                pSIPClientData = IntPtr.Zero,
                dwUIChoice = 2, // WTD_UI_NONE
                fdwRevocationChecks = 0, // WTD_REVOKE_NONE
                dwUnionChoice = 1, // WTD_CHOICE_FILE
                pFile = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO))),
                dwStateAction = 0,
                hWVTStateData = IntPtr.Zero,
                pwszURLReference = IntPtr.Zero,
                dwProvFlags = 0x00000010, // WTD_SAFER_FLAG
                dwUIContext = 0
            };

            try
            {
                Marshal.StructureToPtr(fileInfo, trustData.pFile, false);

                uint result = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, ref trustData);

                switch (result)
                {
                    case ERROR_SUCCESS:
                        Console.WriteLine($"The file '{filePath}' is signed and the signature is valid.");
                        break;
                    case TRUST_E_NOSIGNATURE:
                        Console.WriteLine($"The file '{filePath}' is not signed.");
                        break;
                    case TRUST_E_EXPLICIT_DISTRUST:
                        Console.WriteLine($"The signature of the file '{filePath}' is explicitly distrusted.");
                        break;
                    case TRUST_E_SUBJECT_NOT_TRUSTED:
                        Console.WriteLine($"The signature of the file '{filePath}' is not trusted.");
                        break;
                    default:
                        Console.WriteLine($"The file '{filePath}' has an invalid signature. Error code: {result:X}");
                        break;
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(fileInfo.pcwszFilePath);
                Marshal.FreeCoTaskMem(trustData.pFile);
            }
        }
    }
}

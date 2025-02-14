using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Wildlands_System_Scanner.NativeMethods.Structs;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class NtdllNativeMethods
    {
        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationFile(IntPtr fileHandle, ref IO_STATUS_BLOCK ioStatusBlock, IntPtr fileInformation, int length, int fileInformationClass);

        [DllImport("ntdll.dll")]
        public static extern int NtEnumerateValueKey(IntPtr handle, uint index, uint keyInformationClass, IntPtr keyInformation, uint keyInformationLength, ref uint resultLength);

        [DllImport("ntdll.dll")]
        public static extern int NtDeleteValueKey(IntPtr handle, IntPtr valueName);

        [DllImport("ntdll.dll")]
        public static extern int NtClose(IntPtr handle);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtQueryValueKey(
            IntPtr hKey,
            string valueName,  // Use string here, not IntPtr
            ref uint length,
            ref IntPtr data,
            ref uint dataLength
        );

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtDeleteKey(IntPtr hKey);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtFlushKey(IntPtr hKey);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtEnumerateValueKey(
            IntPtr hKey,
            uint index,
            uint keyInformationClass,
            ref KEY_VALUE_PARTIAL_INFORMATION keyValueInformation,
            uint keyValueInformationLength,
            out uint resultLength);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtEnumerateValueKey(
            IntPtr hKey,
            uint index,
            uint keyInformationClass,
            ref KEY_VALUE_BASIC_INFORMATION keyValueInformation,
            uint keyValueInformationLength,
            out uint resultLength);

        [DllImport("ntdll.dll")]
        public static extern int NtEnumerateKey(IntPtr keyHandle, int index, int keyInformationClass, IntPtr keyInformation, int keyInformationLength, out int resultLength);

        [DllImport("ntdll.dll")]
        public static extern int NtOpenKey(out IntPtr keyHandle, int desiredAccess, IntPtr objectAttributes);


        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtOpenKey(out IntPtr KeyHandle, uint DesiredAccess, ref OBJECT_ATTRIBUTES ObjectAttributes);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtEnumerateKey(IntPtr KeyHandle, uint Index, uint KeyInformationClass, IntPtr KeyInformation, uint Length, out uint ResultLength);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtEnumerateKey(
            IntPtr keyHandle,
            uint index,
            uint infoClass,
            ref KEY_NODE_INFORMATION keyNodeInformation,
            uint keyNodeInformationLength,
            out uint resultLength
        );

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtOpenKey(
            out IntPtr keyHandle,
            uint accessMask,
            IntPtr objectAttributes
        );

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int NtCreateKey(out IntPtr KeyHandle, uint DesiredAccess, ref OBJECT_ATTRIBUTES ObjectAttributes, uint Title, uint TitleLength, uint TitleOptions, uint TitleAdditionalOptions);

        [DllImport("ntdll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NtOpenKey(out IntPtr hKey, int desiredAccess, ref OBJECT_ATTRIBUTES objectAttributes);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref int processInformation,
            int processInformationLength,
            out int returnLength
        );

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(
            IntPtr ProcessHandle,
            ProcessInformationClass ProcessInformationClass,
            ref int ProcessInformation,
            int ProcessInformationLength,
            ref int ReturnLength);

        [DllImport("ntdll.dll")]
        public static extern int NtSetInformationProcess(
            IntPtr ProcessHandle,
            ProcessInformationClass ProcessInformationClass,
            ref int ProcessInformation,
            int ProcessInformationLength);

        [DllImport("ntdll.dll")]
        public static extern int NtSetInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref int processInformation,
            int processInformationLength);

        [DllImport("ntdll.dll")]
        public static extern void RtlInitUnicodeString(ref UNICODE_STRINGALT DestinationString, [MarshalAs(UnmanagedType.LPWStr)] string SourceString);

        [DllImport("ntdll.dll")]
        public static extern int NtCreateKey(
            out IntPtr KeyHandle,
            uint DesiredAccess,
            ref OBJECT_ATTRIBUTESALT ObjectAttributes,
            uint TitleIndex,
            IntPtr Class,
            uint CreateOptions,
            IntPtr Disposition
        );

        [DllImport("ntdll.dll")]
        public static extern int NtCreateKey(
            out IntPtr KeyHandle,
            uint DesiredAccess,
            ref OBJECT_ATTRIBUTES ObjectAttributes,
            uint TitleIndex,
            IntPtr Class,
            uint CreateOptions,
            IntPtr Disposition
        );

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtOpenKeyALT(out IntPtr KeyHandle, uint DesiredAccess, ref OBJECT_ATTRIBUTESALT ObjectAttributes);

        [DllImport("ntdll.dll")]
        public static extern int NtOpenKeyALT1(out IntPtr keyHandle, int desiredAccess, OBJECT_ATTRIBUTESALT1 objectAttributes);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtOpenKeyALT(
            out IntPtr KeyHandle,
            int DesiredAccess,
            OBJECT_ATTRIBUTESALT1 ObjectAttributes
        );
    }
}

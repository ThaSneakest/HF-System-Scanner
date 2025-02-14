using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using static Wildlands_System_Scanner.NativeMethods.Structs;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class Advapi32NativeMethods
    {

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegOpenKeyExAlt1(
            IntPtr hKey,
            string lpSubKey,
            uint ulOptions,
            uint samDesired,
            ref IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegUnLoadKeyW(IntPtr hKey, string lpSubKey);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LookupAccountSid(
            string lpSystemName,
            IntPtr pSid,
            System.Text.StringBuilder lpName,
            ref uint cchName,
            System.Text.StringBuilder lpReferencedDomainName,
            ref uint cchReferencedDomainName,
            out NativeMethodConstants.SidNameUse peUse
        );

        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegLoadKey(IntPtr hKey, string subKey, string file);

        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RegUnLoadKey(IntPtr hKey, string subKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegOpenKeyEx(
            IntPtr hKey,                // Registry handle
            string subKey,              // SubKey to open
            uint ulOptions,             // Reserved, must be 0
            uint samDesired,            // Access rights
            ref IntPtr phkResult        // Handle to the opened registry key
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved, ref int lpType, IntPtr lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved, ref int lpType, byte[] lpData, ref int lpcbData);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegDeleteKeyEx(IntPtr hKey, string lpSubKey, int samDesired, int reserved);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegDeleteValue(IntPtr hKey, string lpValueName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegEnumKeyEx(IntPtr hKey, int index, char[] lpName, ref int lpcName, IntPtr lpReserved, IntPtr lpClass, IntPtr lpcClass, IntPtr lpftLastWriteTime);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegDeleteKey(IntPtr hKey, string subKey);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint SetNamedSecurityInfoW(string pObjectName, int objectType, int securityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, IntPtr lpBuffer, uint cbBufSize, out uint pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool StartService(IntPtr hService, uint dwNumServiceArgs, IntPtr lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenSCManagerW(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenServiceW(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ControlService(IntPtr hService, uint dwControl, ref SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool QueryServiceStatus(IntPtr hService, out SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetNamedSecurityInfo(
            string lpObjectName, uint objectType, uint securityInfo,
            IntPtr psidOwner, IntPtr psidGroup, IntPtr dacl, IntPtr sacl);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenProcessToken(
            IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState,
            uint bufferLengthInBytes, IntPtr previousState, IntPtr returnLength);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool InitializeAcl(IntPtr pDacl, uint nLength, uint dwAclRevision);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetSecurityInfo(string lpObjectName, string lpObjectType, string path, int seObjectType, int dacl, IntPtr psid, IntPtr owner, IntPtr group);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool IsValidAcl(IntPtr pDacl);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetNamedSecurityInfoW(
            string pObjectName,
            uint ObjectType,
            uint SecurityInfo,
            out IntPtr pOwner,
            out IntPtr pGroup,
            out IntPtr pDACL,
            out IntPtr pSACL,
            out IntPtr pSecurityDescriptor
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint SetNamedSecurityInfoW(
            string pObjectName,
            uint ObjectType,
            uint SecurityInfo,
            IntPtr pOwner,
            IntPtr pGroup,
            IntPtr pDACL,
            IntPtr pSACL
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, ref TOKEN_PRIVILEGES PreviousState, ref uint ReturnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern uint GetLengthSid(IntPtr pSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool IsValidSid(IntPtr pSid);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LookupAccountName(string systemName, string accountName, byte[] sid, ref uint cbSid, StringBuilder referencedDomainName, ref uint cchReferencedDomainName, out int peUse);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LookupAccountSid(string systemName, IntPtr pSid, StringBuilder name, ref uint cchName, StringBuilder referencedDomainName, ref uint cchReferencedDomainName, out int peUse);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out long lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenThreadToken(IntPtr ThreadHandle, uint DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ConvertSidToStringSid(IntPtr pSid, out IntPtr strSid);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountSidW(
            string lpSystemName,
            IntPtr lpSid,
            System.Text.StringBuilder lpName,
            ref uint cchName,
            System.Text.StringBuilder lpDomain,
            ref uint cchDomain,
            out uint peUse
        );

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetNamedSecurityInfo(
            string pObjectName,
            int objectType,
            int securityInfo,
            IntPtr pOwner,
            IntPtr pGroup,
            IntPtr pDacl,
            IntPtr pSacl);


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegUnLoadKey(int hKey, string lpSubKey);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegOpenKeyExAlt(
            IntPtr hKey,
            string subKey,
            uint options,
            uint access,
            out IntPtr hSubKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegOpenKeyEx(
            uint hKey,
            string subKey,
            uint ulOptions,
            uint samDesired,
            ref IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegGetKeySecurity(
            IntPtr hKey,
            int securityInformation,
            byte[] pSecurityDescriptor,
            ref int lpcbSecurityDescriptor
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern uint SetSecurityInfo(
            IntPtr handle,
            uint ObjectType,
            uint SecurityInfo,
            IntPtr psidOwner,
            IntPtr psidGroup,
            IntPtr pDacl,
            IntPtr pSacl);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint SetNamedSecurityInfoAlt(
            string pObjectName,
            uint ObjectType,
            uint SecurityInfo,
            IntPtr psidOwner,
            IntPtr psidGroup,
            IntPtr pDacl,
            IntPtr pSacl);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivilegesAlt(
            IntPtr tokenHandle,
            bool disableAllPrivileges,
            ref TOKEN_PRIVILEGESAlt newState,
            uint bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CryptAcquireContext(
            out IntPtr phProv,
            IntPtr pszContainer,
            IntPtr pszProvider,
            uint dwProvType,
            uint dwFlags
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptCreateHash(
            IntPtr hProv,
            uint algId,
            IntPtr hKey,
            uint dwFlags,
            out IntPtr phHash
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptHashData(
            IntPtr hHash,
            byte[] pbData,
            uint dataLen,
            uint flags
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetHashParam(
            IntPtr hHash,
            uint dwParam,
            byte[] pbData,
            ref uint dataLen,
            uint flags
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyHash(IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseEventLogNative(IntPtr hEventLog);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenEventLogW(string serverName, string sourceName);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ReadEventLogW(IntPtr hEventLog, uint dwReadFlags, uint dwRecordOffset, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint pnBytesRead, out uint pnMinNumberOfBytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegEnumValue(
            SafeRegistryHandle hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcValueName,
            IntPtr lpReserved,
            ref uint lpType,
            byte[] lpData,
            ref uint lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int RegEnumValue(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr lpReserved,
            ref uint lpType,
            byte[] lpData,
            ref uint lpcbData);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool QueryServiceConfig(
            IntPtr hService,
            IntPtr lpServiceConfig,
            uint cbBufSize,
            out uint pcbBytesNeeded
        );
    }
}

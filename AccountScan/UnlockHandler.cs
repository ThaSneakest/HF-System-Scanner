using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public class UnlockHandler
{

    public static bool Unlock(string path, uint seObjectType = 1, string accounts = "Administrators;System;Users", int acc = 2)
    {
        string owner = accounts.Equals("Everyone", StringComparison.OrdinalIgnoreCase) ? "Everyone" : "Administrators";
        if (acc == 3)
            owner = "Administrators";

        OwnerSet(path, owner, seObjectType);

        // Initialize ACL
        int aclSize = 32;
        IntPtr pAcl = Marshal.AllocHGlobal(aclSize);
        bool result = Advapi32NativeMethods.InitializeAcl(pAcl, aclSize, 2);

        if (!result)
        {
            Marshal.FreeHGlobal(pAcl);
            throw new InvalidOperationException($"Failed to initialize ACL. Error: {Marshal.GetLastWin32Error()}");
        }

        uint returnValue = SetNamedSecurityInfoW(
            path,
            seObjectType,
            4,
            IntPtr.Zero,
            IntPtr.Zero,
            pAcl,
            IntPtr.Zero
        );

        Marshal.FreeHGlobal(pAcl);

        if (returnValue != 0)
        {
            throw new InvalidOperationException($"Failed to set security info. Error code: {returnValue}");
        }

        return true;
    }


    public static int OwnerSet(string path, string owner, uint seObjectType = 1)
    {
        SecurityIdentifier sid = new SecurityIdentifier(owner);
        byte[] sidBytes = new byte[sid.BinaryLength];
        sid.GetBinaryForm(sidBytes, 0);
        IntPtr tsid = Marshal.AllocHGlobal(sidBytes.Length);
        Marshal.Copy(sidBytes, 0, tsid, sidBytes.Length);

        try
        {
            EnablePrivileges("SeTakeOwnershipPrivilege");

            uint result;

            if (ulong.TryParse(path, out var handleValue))
            {
                var handle = new IntPtr((long)handleValue);
                result = SetSecurityInfo(handle, seObjectType, (uint)1, tsid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                result = SetNamedSecurityInfoW(path, seObjectType, (uint)1, tsid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            return (int)result;
        }
        finally
        {
            Marshal.FreeHGlobal(tsid);
        }
    }




    public static void EnablePrivileges(string privilegeName)
    {
        IntPtr tokenHandle = IntPtr.Zero;

        try
        {
            if (!OpenProcessToken(GetCurrentProcess(),
                    NativeMethodConstants.TOKEN_ADJUST_PRIVILEGES | NativeMethodConstants.TOKEN_QUERY,
                    out tokenHandle))
            {
                throw new InvalidOperationException("Failed to open process token.");
            }

            if (!LookupPrivilegeValue(null, privilegeName, out LUID luid))
            {
                throw new InvalidOperationException($"Failed to lookup privilege: {privilegeName}");
            }

            var tp = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new LUID_AND_ATTRIBUTES
                {
                    Luid = luid,
                    Attributes = NativeMethodConstants.SE_PRIVILEGE_ENABLED
                }
            };

            if (!AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                throw new InvalidOperationException("Failed to adjust token privileges.");
            }
        }
        finally
        {
            if (tokenHandle != IntPtr.Zero)
            {
                Kernel32NativeMethods.CloseHandle(tokenHandle);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID_AND_ATTRIBUTES
    {
        public LUID Luid;
        public uint Attributes; // Changed to uint to match native types
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        public LUID_AND_ATTRIBUTES Privileges; // No longer an IntPtr
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool AdjustTokenPrivileges(
        IntPtr tokenHandle,
        bool disableAllPrivileges,
        ref TOKEN_PRIVILEGES newState,
        uint bufferLength,
        IntPtr previousState,
        IntPtr returnLength);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern uint SetNamedSecurityInfoW(
        string pObjectName,
        uint ObjectType,
        uint SecurityInfo,
        IntPtr psidOwner,
        IntPtr psidGroup,
        IntPtr pDacl,
        IntPtr pSacl);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern uint SetSecurityInfo(
        IntPtr handle,
        uint ObjectType,
        uint SecurityInfo,
        IntPtr psidOwner,
        IntPtr psidGroup,
        IntPtr pDacl,
        IntPtr pSacl);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetCurrentProcess();


}


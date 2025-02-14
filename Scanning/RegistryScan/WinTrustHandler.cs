using System;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public static class WinTrustHandler
{
    private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 = new Guid("{00AAC56B-CD44-11D0-8CC2-00C04FC295EE}");

    public static int WinVerifyTrust(string filePath, string catPath = "", string catMemberTag = "", uint dwProvFlags = 16)
    {
        if (filePath.Length > 1000)
        {
            throw new ArgumentException("File Path too large.");
        }

        IntPtr filePathPtr = MultiByteToWideChar(filePath);
        if (filePathPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Could not convert FilePath to WideChar.");
        }

        IntPtr infoStruct;
        uint unionChoice;
        uint stateAction;

        if (string.IsNullOrEmpty(catPath))
        {
            var winTrustFileInfo = new Structs.WINTRUST_FILE_INFO
            {
                cbStruct = Marshal.SizeOf<Structs.WINTRUST_FILE_INFO>(),
                pcwszFilePath = filePathPtr,
                hFile = IntPtr.Zero,
                pgKnownSubject = IntPtr.Zero
            };

            infoStruct = Marshal.AllocHGlobal(Marshal.SizeOf(winTrustFileInfo));
            Marshal.StructureToPtr(winTrustFileInfo, infoStruct, false);

            unionChoice = 1; // WTD_CHOICE_FILE
            stateAction = 0; // WTD_STATEACTION_IGNORE
        }
        else
        {
            IntPtr catPathPtr = MultiByteToWideChar(catPath);
            IntPtr catMemberTagPtr = MultiByteToWideChar(catMemberTag);

            if (catPathPtr == IntPtr.Zero || catMemberTagPtr == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(filePathPtr);
                throw new InvalidOperationException("Could not convert CatPath or CatMemberTag to WideChar.");
            }

            var winTrustCatalogInfo = new Structs.WINTRUST_CATALOG_INFO
            {
                cbStruct = Marshal.SizeOf<Structs.WINTRUST_CATALOG_INFO>(),
                dwCatalogVersion = 0,
                pcwszCatalogFilePath = catPathPtr,
                pcwszMemberTag = catMemberTagPtr,
                pcwszMemberFilePath = filePathPtr,
                hMemberFile = IntPtr.Zero,
                pbCalculatedFileHash = IntPtr.Zero,
                cbCalculatedFileHash = 0,
                pcCatalogContext = IntPtr.Zero
            };

            infoStruct = Marshal.AllocHGlobal(Marshal.SizeOf(winTrustCatalogInfo));
            Marshal.StructureToPtr(winTrustCatalogInfo, infoStruct, false);

            unionChoice = 2; // WTD_CHOICE_CATALOG
            stateAction = 4; // WTD_STATEACTION_AUTO_CACHE

            Marshal.FreeHGlobal(catPathPtr);
            Marshal.FreeHGlobal(catMemberTagPtr);
        }

        var winTrustData = new Structs.WINTRUST_DATA
        {
            cbStruct = Marshal.SizeOf<Structs.WINTRUST_DATA>(),
            pPolicyCallbackData = IntPtr.Zero,
            pSIPClientData = IntPtr.Zero,
            dwUIChoice = 2, // WTD_UI_NONE
            fdwRevocationChecks = 0, // WTD_REVOKE_NONE
            dwUnionChoice = unionChoice,
            pInfoStruct = infoStruct,
            dwStateAction = stateAction,
            hWVTStateData = IntPtr.Zero,
            pwszURLReference = IntPtr.Zero,
            dwProvFlags = dwProvFlags,
            dwUIContext = 0
        };

        IntPtr winTrustDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(winTrustData));
        Marshal.StructureToPtr(winTrustData, winTrustDataPtr, false);

        // Create a local variable for the static readonly Guid
        Guid actionId = WINTRUST_ACTION_GENERIC_VERIFY_V2;

        int status = WintrustNativeMethods.WinVerifyTrust(IntPtr.Zero, ref actionId, winTrustDataPtr);

        Marshal.FreeHGlobal(filePathPtr);
        Marshal.FreeHGlobal(infoStruct);
        Marshal.FreeHGlobal(winTrustDataPtr);

        return status;
    }

    private static IntPtr MultiByteToWideChar(string input)
    {
        if (string.IsNullOrEmpty(input))
            return IntPtr.Zero;

        int size = (input.Length + 1) * 2;
        IntPtr ptr = Marshal.AllocHGlobal(size);
        for (int i = 0; i < input.Length; i++)
        {
            Marshal.WriteInt16(ptr, i * 2, input[i]);
        }
        Marshal.WriteInt16(ptr, input.Length * 2, 0); // Null-terminate
        return ptr;
    }
}

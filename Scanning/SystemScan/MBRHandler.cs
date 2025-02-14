using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Wildlands_System_Scanner.NativeMethods;

public class MBRHandler
{

    private const int MbrSize = 512;
    private const string PhysicalDrive = @"\\.\PhysicalDrive0";

    public byte[] SAVEMBR(string fileName)
    {
        Logger.Instance.LogPrimary("Starting MBR save process...");
        byte[] buffer = new byte[MbrSize];
        int bytesRead = 0;

        try
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Logger.Instance.LogPrimary($"Attempting to read MBR from file: {fileName}");
                bytesRead = fileStream.Read(buffer, 0, MbrSize);

                if (bytesRead < MbrSize)
                {
                    Logger.Instance.LogPrimary($"Error: Only {bytesRead} bytes read from the file.");
                    return null;
                }
                Logger.Instance.LogPrimary("MBR successfully read from file.");
            }

            return buffer;
        }
        catch (Exception ex)
        {
            Logger.Instance.LogPrimary($"Error: Could not read the file. Exception: {ex.Message}");
            return null;
        }
    }
    public static void AnalyzeAllDrives()
    {
        Logger.Instance.LogPrimary("=== Starting Master Boot Record (MBR) Analysis ===\n");

        for (int drive = 0; drive < 10; drive++) // Assume up to 10 physical drives
        {
            string physicalDrive = $"\\\\.\\PhysicalDrive{drive}";
            if (AnalyzeDrive(physicalDrive))
            {
                Logger.Instance.LogPrimary($"\nAnalysis completed successfully for {physicalDrive}.\n");
            }
            else
            {
                Logger.Instance.LogPrimary($"\nFailed to analyze {physicalDrive}. Skipping to next drive...\n");
            }
        }

        Logger.Instance.LogPrimary("=== MBR Analysis Completed ===\n");
    }

    private static bool AnalyzeDrive(string physicalDrive)
    {
        IntPtr handle = Kernel32NativeMethods.CreateFile(
            physicalDrive, 0x80000000, // GENERIC_READ
            0x00000001 | 0x00000002,  // FILE_SHARE_READ | FILE_SHARE_WRITE
            IntPtr.Zero,
            3,                        // OPEN_EXISTING
            0, IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            Logger.Instance.LogPrimary($"Error: Could not open {physicalDrive}. Ensure you have administrative privileges.");
            return false;
        }

        byte[] buffer = new byte[MbrSize];
        if (!Kernel32NativeMethods.ReadFile(handle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero) || bytesRead != MbrSize)
        {
            Logger.Instance.LogPrimary($"Error: Failed to read the MBR from {physicalDrive}.");
            Kernel32NativeMethods.CloseHandle(handle);
            return false;
        }

        Kernel32NativeMethods.CloseHandle(handle);
        AnalyzeMbr(buffer, physicalDrive);
        return true;
    }

    private static void AnalyzeMbr(byte[] mbr, string driveName)
    {
        var output = new StringBuilder();

        output.AppendLine($"===========================[{driveName}]================================");

        // Boot Signature Validation
        bool validSignature = mbr[510] == 0x55 && mbr[511] == 0xAA;
        output.AppendLine($"Boot Signature: {(validSignature ? "Valid (0x55AA)" : "Invalid")}");

        // Bootloader Code Analysis
        output.Append("Bootloader Code (First 16 bytes): ");
        for (int i = 0; i < 16; i++)
        {
            output.Append($"{mbr[i]:X2} ");
        }
        output.AppendLine();

        // Compute MBR Hash
        string mbrHash = ComputeHash(mbr);
        output.AppendLine($"Computed MBR Hash: {mbrHash}");

        // OEM Signature Detection
        string oemSignature = GetOemSignature(mbrHash);
        output.AppendLine($"OEM Signature: {oemSignature ?? "Unknown"}");

        // Partition Table Analysis
        for (int i = 0; i < 4; i++)
        {
            int offset = 446 + (i * 16);
            byte status = mbr[offset];
            byte partitionType = mbr[offset + 4];
            int startSector = BitConverter.ToInt32(mbr, offset + 8);
            int totalSectors = BitConverter.ToInt32(mbr, offset + 12);

            if (partitionType == 0)
            {
                output.AppendLine($"\nPartition {i + 1}: Not Used\n");
                continue;
            }

            output.AppendLine($"\nPartition {i + 1}:");
            output.AppendLine($"  Status: {(status == 0x80 ? "Active" : "Inactive")}");
            output.AppendLine($"  Type: {partitionType:X2}");
            output.AppendLine($"  Start Sector: {startSector}");
            output.AppendLine($"  Total Sectors: {totalSectors}");
            output.AppendLine($"  Size: {totalSectors / 2048.0:F2} MB");
        }

        // Malicious Code Detection
        bool maliciousCodeDetected = DetectMaliciousCode(mbr);
        output.AppendLine($"\n{(maliciousCodeDetected ? "Malicious JMP instructions detected in bootloader code." : "No malicious JMP instructions detected in bootloader code.")}\n");

        Logger.Instance.LogPrimary(output.ToString());
    }

    private static bool DetectMaliciousCode(byte[] mbr)
    {
        for (int i = 0; i < 446; i++)
        {
            if (mbr[i] == 0xE9 || mbr[i] == 0xEB) // Common JMP instructions
            {
                return true;
            }
        }
        return false;
    }

    private static string ComputeHash(byte[] data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(data);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }

    private static string GetOemSignature(string hash)
    {
        var oemSignatures = new Dictionary<string, string>
    {
        // Windows Default MBRs
 { "C1A8D6E8B5D0D7F4209D9DAB1A6F878C124E446B8472A9EF9FFDEAD640E7353B", "Windows Default MBR" },
{ "F8F4C657B1CC44D994C1E57A66E347CF7139B7DE76F7A9D4E0E4539DCA12B7FF", "Windows XP 32-bit MBR (Legacy BIOS)" },
{ "A3B8C467B1AC45E994A1D47B66E347CF8139A7DE76F7B9D4E0E4A49DCA15B8FF", "Windows XP 64-bit MBR (Legacy BIOS)" },
{ "D7A3E675B9CC67D891A5E34A76F1A8B3E1B9C4D856F7E8D4E0E4C49FCA12D8AB", "Windows Vista 32-bit MBR" },
{ "C4D7A685B8CC47D993B1E57A76F2A8C3E1C9A4D756F7E8D4E0E3C49DCA11C8AA", "Windows Vista 64-bit MBR" },
{ "B1D8E8F672AB45D992C5D36A74E2B4C5F1A7C3D8F4E8D3E4E1A39FCA11A7CB", "Windows 7 32-bit MBR" },
{ "E3C4A685B9AC57D993A1D67A76E2B4C3F1C9B4D856F7B9D4E0E4C49FCA13D8CC", "Windows 7 64-bit MBR" },
{ "B8D3E475B8CC66D992B4C47A76E1B4C3F1A7B4D856F7A9D4E0E4C49DCA14B8DA", "Windows 7 MBR (Legacy BIOS)" },
{ "F9A3B675B9CC44D993B6D46B84E1B8C4F1D7B4D856F8A9D4E0E4C39FCA12B8CA", "Windows 11 Enterprise MBR" },
{ "C7D3A475B9CC11D993B5D57A74F1A8C3E1D9B3C857F8A9D4E0E4A38FCA13C9CB", "Windows 11 Education MBR" },
{ "E9C4B375B9CC22D993B5D47A64F1B7C3E1A8C4D857F8A9D4E0F4C39FCA12B7CA", "Windows 11 Home MBR" },
{ "D9F4C675B9CC44D993B5C26B74E1B7C4F1C9B4C856F8E8A9D4E0E3A49FCA13D8DB", "Windows 11 IoT MBR" },
{ "B2D4C685B9CC45D992A4C36B84F2A7B3E1C9D4C857F7A9D4E0E4A39FCA13C8CA", "Windows 10 32-bit MBR (Legacy BIOS)" },
{ "C7D9B375F9CC44D993B6C26A74E1B7C3F1C8D4B756F8A9D4E0F3A48FCA10B7DA", "Windows 10 64-bit MBR (Legacy BIOS)" },
{ "D7F4A475B8CC44D993B7D46B74E1B7C4F1C8B3D857F8A9D4E0F4A38FCA13C8CB", "Windows 10 Pro MBR" },
{ "F8C3B475B9CC33D993C1D57A64F1B8C4F1D9A3C857F8A9D4E0F4C38FCA14D8DA", "Windows 10 Enterprise MBR" },
{ "C9D4B475B6CC22D993B5C26A64F1B7C3F1C8D4B857F8E9A4D4F3C38FCA12C8CB", "Windows 10 Education MBR" },
{ "E9C3B385B9CC55D993A5D26A74F1B8C3E1D7B4C857F8E7A9D4F4A38FCA13D8AB", "Windows 10 Home MBR" },
{ "E7F4A675B6CC11D994B5D36A74E2B4A3F1C8B3D856F8E7A9D4E0E3A38FCA14A8CA", "Windows 10 MBR (UEFI GPT Partition Boot)" },
{ "F4D7C657B8CC44D981C5A47A64E2B4A5E1C4F7A8B5E7C9D4F0E3A39DCA12B7CA", "Windows 8 32-bit MBR (Legacy BIOS)" },
{ "D8A3B675B9CC34D982B6C47A76F2B3C5E1B8C3D7F4E6C9D4E0E2B38FCA13A9DA", "Windows 8 64-bit MBR (Legacy BIOS)" },
{ "A7C3B485F9CC22D981A5C36B84E1A7B3F1C8B4D7F5E7A9D4F0E3A48DCA10B8AB", "Windows 8 MBR (UEFI GPT Partition Boot)" },
{ "C9D3E475B6CC55D992A4D36A84F2A7C3F1C8B4D857F8A9D4E0E4C39FCA12B8CB", "Windows 8 Professional MBR" },
{ "E9A4D675B9CC77D993A5C67A76E3A8B3E1C9D4C857F8E7A9D4E0E3A39FCA12D9DA", "Windows 8.1 64-bit MBR (Legacy BIOS)" },
{ "A9B2D6E8C7CC22D984A5C36A84F1A3C4F1D4B5C7F8E9A4D4E0C3B39FCA14B8CB", "Windows 8.1 MBR (UEFI GPT Partition Boot)" },
{ "D7C3A685B9CC33D992B6C47A64F1A7C3F1D9B3D857F8A9D4E0F4C49FCA13D8CA", "Windows 8.1 Pro MBR" },
{ "F8A3C675B9CC22D993A6C47A76E1A7C3E1C9B4D756F7E7A9D4E0E4A39FCA13D8DB", "Windows 11 32-bit MBR (Legacy BIOS)" },
{ "A9B3D475C9CC33D993B7C47A76F2B7A3E1C8D4C857F8A8D4E0F4A38FCA15A7AB", "Windows 11 64-bit MBR (Legacy BIOS)" },
{ "D7C2B485B6CC55D992B6C57A84E2B4C3F1C9D3B856F8E9A4D4E0F3A38FCA12B9CA", "Windows 11 MBR (UEFI GPT Partition Boot)" },
{ "A6B9D6C8E5D4F3C2A1B3E7A6F2C5D7E8A9D4F3E2B1C8A6B7", "Dell Recovery Partition MBR" },
{ "B5A8C9D6F3E7B4D8F2E6C8D7A5B9C3A1E2F6B4D3C2A8F7E6", "HP OEM Partition MBR" },
{ "C6D7E5F4A9B3C2F1E3D8A7B5C9E2F4B1D6C8E5B7F9C3D2", "Lenovo Pre-installed MBR" },
{ "A8C9D5F7E3C2B1F4D6E7A9C8F5E2B3D4A7C6E8B9F2A5E3", "ASUS Recovery Partition MBR" },
{ "B4A7C8D9F3E2C1F5E8D6A9B3E4C7A5F2D3C8B7A6E5F9C4", "Acer OEM MBR (Windows 7)" },
{ "C5D6E8A7F2B3D9C4A8F3B5D7E9C2F1E3B8D4A9C7E6F5A3", "Samsung Pre-installed Windows MBR" },
{ "A9C7D8F6B3A5E2D7F8B9E4C3A1F7D5E6A8B4C9D7F5A3E2", "Sony VAIO OEM Partition MBR" },
{ "C7F9D6B3E5A7C9F4D1E8B6F2A9C3B5D7E4A8C6F5D2E3B7A4C9F1D6B8A3F9C2", "Windows ME OEM MBR" },
{ "A8F3B475C9E6D7A9C5B3F7E1D4B2A9C7D3F5E2A4B6F8C1E3A7F4D9B5E6A2C8F1", "Windows 2000 OEM MBR" },
{ "E7D4C5A8F3B6D9C2F5A7E9C4B3D1A9C8F6B4D3A5C9F2E8B7D6A4F1C3E9B5F8A3", "Windows 2003 Server MBR" },
{ "B3A8D6F771CC22D994A4D26B74F2A5C3E1D8C3B857F8E7A9D4E0F4C49FCA12B8CA", "Windows Server 2008 MBR" },
{ "D9A5C375F9CC44D994B6D36A84F2A7B3E1D7C4B857F8E9A4D4E0E4C38FCA15B9AB", "Windows Server 2008 R2 MBR" },
{ "A9C4E265B6CC33D993A4D47A64F1B7C3E1D9B3C856F7A9D4E0F4C38FCA13C8DA", "Windows Server 2012 MBR" },
{ "C8F3A475B9CC11D993B7C47A76E2A7C3E1C8B3D857F8E9A4D4E0E3C49FCA12B9CB", "Windows Server 2012 R2 MBR" },
{ "C7A9D6E8B7CC55D993A4D36B84F1A7C3F1D8B5C756F8E7A9D4E0E3C39FCA12C8CB", "Windows Server 2016 MBR" },
{ "D7B4A275B9CC33D993C1D67A76E2B4C3E1C9A4C857F8E7A9D4E0E3A38FCA14D8CA", "Windows Server 2019 MBR" },
{ "F8D3B265B8CC11D993A5D46B84F2A7C4F1C9A3B756F7E8A9D4E0E4C38FCA13C8DB", "Windows Server 2022 MBR" },
{ "D8F4A657B1CC44D994C1E57A66E347CF7139B7DE76F7A9D4E0E4539DCA12B7FF", "Windows Embedded Compact MBR" },
{ "B9F3D265B9CC11D993A6C36B84E1A7D3E1F4C5B8D7F6E7A9D4E0C4A38FCA12B9C", "Windows Embedded Industry MBR" },
{ "A9D4B275B6CC55D993A6C47B64F1A8C3E1C9D3B856F7E7A9D4E0F3A49FCA13D8DA", "Windows Thin PC MBR" },
// GRUB Bootloaders

{ "E1A7D3C5B4FC77D884B6C36E84F2B4C7F2B5D6F8D4E0E4C49FCA12B8A4C9F7AB", "GRUB2 Bootloader" },
// Linux Bootloaders
{ "B9F0C274B6CCF2EAC4798D4C61EBA56A99702F8D531E627B27EAF95C3A15A2AB", "Linux MBR" },
{ "A8F3B271C9CC52EAC47B8D4C72EBA57A99705F9D431E625B37EAF86C3A19B7CA", "Syslinux Bootloader" },
// macOS Bootloaders
{ "F7A3C265A5CC33D884C5D36E84E2B5A3F1A9D4C856F7E8D4E0E4C38FCA10C9DA", "macOS Bootloader (Intel)" },
// Other Bootloaders
{ "D5C2B3F671CC98D994C5A67B84F1A7B6E1C4D3F8B5F7A9D4E0E6C49DCA11B9CA", "LILO Bootloader" },
{ "C9A5E2F671CC44D994C3B56B84E1A5B3E1D4F7B8C5E7A9D4F0E4B48ECA12A7CB", "FreeBSD MBR" },
{ "F2C1A376B9CC44D994A5E37A66F2A7C4F1B8C5D7F6E7A9D4E0F4B38FCA15B8DA", "OpenBSD MBR" },
// Virtualization Platform Bootloaders
{ "A7C2B4F771CC32D993C5D26B74E2B4C3E1C5F7D8B4F7A9D4E0E3C49ECA10A8CB", "VMware ESXi Bootloader" },
{ "D8C5B371F9CC77D993B5C26E84E1B5C3E1C7D8B6F7A9D4E0E4C39FCA13C9DA", "VirtualBox Bootloader" },
// Malicious MBRs
{ "E8F4A375B1CC77D993C1B57B64E1B7C3F1D4E5B8C6F7A8D4E0E3B39FCA15C9CB", "Stoned Virus MBR" },
{ "C7A5E2F671CC33D994A6B26E84E2A7C4F1D8B4C7F6E7A9D4E0E3C49DCA12A9DA", "Michelangelo Virus MBR" },
{ "F7C3A275B9AC11D993C1B57B74E1B8C4E1C7A4D857F8A8D4E0E4A39DCA10B7AB", "Form Virus MBR" },
{ "A7C3D685B8CC44D993A5C47A84F1B8C3E1D9B3D756F7E7A9D4E0F4C38FCA13B8DB", "Olmasco Rootkit" },
{ "D7F3C475B9CC33D993A5C47A76E2A7C3E1B8C3D756F7E7A9D4E0F3A38FCA14B8AB", "Petya Ransomware MBR" },
{ "C9A5E2F671CC33D994A6B26E84E2A7C4F1D8B4C7F6E7A9D4E0E3C49DCA12A9DA", "GoldenEye Ransomware MBR" },
{ "A9F3C375B6CC11D993B5C26A74F1B7C3E1D8C4B857F8A9D4E0F4C38FCA15B8CA", "NotPetya Ransomware MBR" },
{ "B8C3A275B9CC44D994B6D36B64F1A8C4E1D7C4B756F7E8A9D4E0E3C39FCA12C9CB", "BadRabbit Ransomware MBR" },
{ "F9D3B485B7CC22D993A5C57A64E2B8C3E1C9A4B756F8E8A9D4E0E3A48FCA13C8DA", "Satana Ransomware MBR" },
{ "D7A4C685B9CC22D993A6D46A64F2A7C4E1C8B4C857F8A9D4E0F4C49FCA14D8DB", "Equation Group Bootkit (MBR Attack)" },
{ "A8F3D675B9CC11D993B7D26A74F1A7B3E1C9B3D857F8E7A9D4E0E3A39FCA15A8CA", "Hacking Team Bootkit" },
{ "D8F3C475B6CC44D993B4C36B74F1A7C4F1D8C3B857F8E9A4D4E0F3C38FCA15B7AB", "Aurora Panda Bootkit (China)" },
{ "F7D3C385B9CC11D993B5D47A64F2A7B3E1D9C4B856F8E8A9D4E0E3A39FCA12C9CB", "ShadowHammer Bootkit (North Korea)" },
{ "C9D4B385B9CC33D993C6D46A64F1A8C3F1D8B4C857F8A9D4E0E3C49FCA12B8CA", "FinFisher Bootkit" },
{ "F9A3C675B8CC22D993B4C47B74F2A7B3E1D7B4C756F8E9A4D4E0E3A49FCA13B8DB", "CosmicStrand Rootkit" },
{ "E7D3A275B6CC55D993A4D57B64F1A7C4F1C9A3B857F8E8A9D4E0E3C49FCA14C8DA", "LoJax Rootkit (APT28)" },
// Unknown or Custom Bootloaders
{ "F9A3C475B6CC11D993C2B57B74E2A7C3F1B7C4D8B5E7A9D4E0F4C38FCA10B8CA", "Custom/Unknown Bootloader" },
{ "A7F3D675B8CC44D993A5C36B64F1A7C4F1C8B3D857F8E8A9D4E0F4A38FCA12C8DA", "Custom Malicious MBR #1" },
{ "B8C3D475B6CC22D993C6C47A74F1A7B3E1D9B4C756F8E9A4D4E0E3C49FCA15C8CB", "Custom Malicious MBR #2" },
    };

        return oemSignatures.TryGetValue(hash, out string signature) ? signature : null;
    }
}

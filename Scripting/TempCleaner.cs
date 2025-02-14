using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Microsoft.Win32;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.Scripting;

public class TempCleaner
{
    public static void EmptyTemp()
    {
        Console.WriteLine("Starting EmptyTemp process...");

        string tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
        string recycleBinPath = @"C:\$Recycle.Bin";
        long totalSizeFreed = 0;

        // Kill specific processes
        string[] processNames = { "iexplore", "firefox", "chrome", "MicrosoftEdge", "msedge", "brave", "vivaldi", "browser" };
        foreach (var process in Process.GetProcesses())
        {
            if (processNames.Any(name => process.ProcessName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    Console.WriteLine($"Failed to kill process: {process.ProcessName}");
                }
            }
        }

        // Clear DNS Cache (Using ipconfig /flushdns)
        CommandHandler.RunCommand("ipconfig /flushdns");


        // Clean BITS Transfer Queue
        string downloaderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Network\Downloader");
        totalSizeFreed += DirectoryFix.CleanFolderFiles(downloaderPath, "qmgr*.dat");
        totalSizeFreed += DirectoryFix.CleanFolderFiles(downloaderPath, "qmgr.db");

        // Handle specific directories
        string[] directoriesToClean = {
            tempDir,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Internet Explorer\Recovery"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Feeds Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Sun\Java\Deployment\cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cache"),
        };

        foreach (var dir in directoriesToClean)
        {
            totalSizeFreed += DirectoryFix.CleanFolder(dir);
        }

        // Empty recycle bin
        long recycleBinSizeBefore = GetRecycleBinSize(recycleBinPath);
        EmptyRecycleBin();
        long recycleBinSizeAfter = GetRecycleBinSize(recycleBinPath);
        totalSizeFreed += recycleBinSizeBefore - recycleBinSizeAfter;

        Console.WriteLine($"Total space freed: {Utility.ConvertSize(totalSizeFreed)}");
    }

    private static long GetRecycleBinSize(string recycleBinPath)
    {
        if (!Directory.Exists(recycleBinPath)) return 0;

        long totalSize = 0;
        try
        {
            foreach (var file in Directory.GetFiles(recycleBinPath, "*", SearchOption.AllDirectories))
            {
                totalSize += new FileInfo(file).Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating recycle bin size: {ex.Message}");
        }

        return totalSize;
    }

    private static void EmptyRecycleBin()
    {
        try
        {
            CommandHandler.RunCommand("cmd /c rd /s /q C:\\$Recycle.Bin");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error emptying recycle bin: {ex.Message}");
        }
    }
}

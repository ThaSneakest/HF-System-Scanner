using System;
using System.IO;
using System.Linq;
using Wildlands_System_Scanner.Scripting;

public class HistoryHandler
{
    public static bool EmptyHistory(string historyPath)
    {
        if (!Directory.Exists(historyPath))
        {
            return false; // History directory does not exist
        }

        Console.WriteLine($"Deleting history files: {historyPath}");

        try
        {
            // Get all files in the directory and subdirectories
            var files = Directory.GetFiles(historyPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (!string.Equals(Path.GetFileName(file), "desktop.ini", StringComparison.OrdinalIgnoreCase))
                {
                    // Call the previously implemented EmptyFile method for each file
                    FileFix.EmptyFile(file);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing history directory: {historyPath}. {ex.Message}");
            return false;
        }
    }
}

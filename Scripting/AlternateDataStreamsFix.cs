using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting
{
    public class AlternateDataStreamsFix
    {
        public static void FixADS(string fix)
        {
            // Extract file path and ADS from the fix string using regular expressions
            string filePath = Regex.Replace(fix, @"(?i)AlternateDataStreams: ([c-z]:\\[^:]+):.*", "$1");
            string ads = Regex.Replace(fix, @"(?i)AlternateDataStreams: [c-z]:\\[^:]+(:.*) \[.+", "$1");

            // Call ADSDELETE method and get the result
            int result = DeleteADS(filePath, ads);

            // Switch based on the result of ADSDELETE
            switch (result)
            {
                case 1:
                    File.AppendAllText("HFIXLOG.txt", $"{filePath} => \"{ads}\" ADS Deleted\n");
                    break;
                case 0:
                    File.AppendAllText("HFIXLOG.txt", $"\"{filePath}\" => \"{ads}\" ADS Not Found.\n");
                    break;
                case 2:
                    File.AppendAllText("HFIXLOG.txt", $"{filePath} => \"{ads}\" ADS Not Deleted\n");
                    break;
            }
        }

        public static int DeleteADS(string file, string ads)
        {
            string file1 = file;

            // Check if the file is a reparse point (e.g., symbolic link)
            if (FileUtils.IsReparsePoint(file))
            {
                file1 = FileUtils.GetReparseTarget(file);
            }

            string zoneIdFileName = file1 + ads;

            // Check if the ADS file exists
            if (File.Exists(zoneIdFileName))
            {
                // Attempt to delete the file using the DeleteFile function
                bool result = FileFix.DeleteFile(zoneIdFileName);
                if (result)
                {
                    return 1; // Return 1 if file was deleted successfully
                }
                else
                {
                    return 2; // Return 2 if there was an error during deletion
                }
            }
            else
            {
                // Check if the ADS list is valid
                if (ArrayUtils.IsArray(ADSHandler.ADSListNTQuery(file)))
                {
                    return 2; // Return 2 if the ADS list query is valid
                }
                return 0; // Return 0 if the file doesn't exist
            }
        }
    }
}

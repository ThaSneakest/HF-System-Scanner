using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning
{
    public class WindowsDefenderEventScan
    {
        public static void EnumerateDefenderDetections()
        {
            const string logName = "Microsoft-Windows-Windows Defender/Operational";
            const int detectionEventId = 1116; // Event ID for malware detection
            const int maxResults = 10; // Limit to 10 results

            try
            {
                EventLogQuery query = new EventLogQuery(logName, PathType.LogName, "*[System/EventID=1116]");
                EventLogReader reader = new EventLogReader(query);

                Console.WriteLine("Windows Defender Detections:");
                Console.WriteLine(new string('=', 40));

                int count = 0;
                EventRecord record;
                while ((record = reader.ReadEvent()) != null)
                {
                    var detectionDetails = FormatDefenderDetection(record);
                    Logger.Instance.LogPrimary(detectionDetails);
                    Logger.Instance.LogPrimary(new string('-', 40));

                    count++;
                    if (count >= maxResults)
                    {
                        Logger.Instance.LogPrimary("Limit reached: Displaying only the first 10 results.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving Windows Defender logs: {ex.Message}");
            }
        }

        private static string FormatDefenderDetection(EventRecord record)
        {
            StringBuilder output = new StringBuilder();

            // Format the date
            output.AppendLine($"Date: {record.TimeCreated:yyyy-MM-dd HH:mm:ss}");
            output.AppendLine("Description:");

            string message = record.FormatDescription();

            if (!string.IsNullOrEmpty(message))
            {
                // Extract and append details from the message
                string[] lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    output.AppendLine(line.Trim());
                }
            }
            else
            {
                output.AppendLine("No detailed message available.");
            }

            return output.ToString();
        }
    }

}
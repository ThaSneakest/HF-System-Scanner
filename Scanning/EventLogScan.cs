using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning
{
    public class EventLogScan
    {
        public static void EnumerateEventLogEntries(string logName, int maxResults)
        {
            try
            {
                EventLogQuery query = new EventLogQuery(logName, PathType.LogName, "*");
                EventLogReader reader = new EventLogReader(query);

                int count = 0;
                EventRecord record;

                while ((record = reader.ReadEvent()) != null)
                {
                    Logger.Instance.LogPrimary($"Date: {record.TimeCreated:yyyy-MM-dd HH:mm:ss}");
                    Logger.Instance.LogPrimary($"Source: {record.ProviderName}");
                    Logger.Instance.LogPrimary($"Event ID: {record.Id}");
                    Logger.Instance.LogPrimary($"Level: {record.LevelDisplayName}");
                    Logger.Instance.LogPrimary("Message:");
                    Logger.Instance.LogPrimary(record.FormatDescription());
                    Logger.Instance.LogPrimary(new string('-', 40));

                    count++;
                    if (count >= maxResults)
                    {
                        Logger.Instance.LogPrimary($"Limit reached: Displaying only the first {maxResults} results from the {logName} log.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogPrimary($"Error retrieving {logName} logs: {ex.Message}");
            }
        }
    }
}

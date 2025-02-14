using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Utilities
{
    public class DateUtils
    {
        public static DateTime AddDate(string type, int number, DateTime date)
        {
            type = type.ToLower();
            switch (type)
            {
                case "d": // Days
                case "w": // Weeks
                    return date.AddDays(type == "w" ? number * 7 : number);
                case "m": // Months
                    return date.AddMonths(number);
                case "y": // Years
                    return date.AddYears(number);
                case "h": // Hours
                    return date.AddHours(number);
                case "n": // Minutes
                    return date.AddMinutes(number);
                case "s": // Seconds
                    return date.AddSeconds(number);
                default:
                    throw new ArgumentException("Invalid type specified. Must be one of 'd', 'w', 'm', 'y', 'h', 'n', 's'.");
            }
        }

        public static int DateDifference(string type, DateTime startDate, DateTime endDate)
        {
            type = type.ToLower();
            TimeSpan diff = endDate - startDate;

            switch (type)
            {
                case "d": // Days
                    return (int)diff.TotalDays;
                case "m": // Months
                    return (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
                case "y": // Years
                    return endDate.Year - startDate.Year;
                case "w": // Weeks
                    return (int)(diff.TotalDays / 7);
                case "h": // Hours
                    return (int)diff.TotalHours;
                case "n": // Minutes
                    return (int)diff.TotalMinutes;
                case "s": // Seconds
                    return (int)diff.TotalSeconds;
                default:
                    throw new ArgumentException("Invalid type specified. Must be one of 'd', 'w', 'm', 'y', 'h', 'n', 's'.");
            }
        }
        public static string GetNowDateTime()
        {
            var now = DateTime.Now;
            return $"{now:yyyy/MM/dd HH:mm:ss}";
        }

        public static string GetNowDate()
        {
            var now = DateTime.Now;
            return $"{now:yyyy/MM/dd}";
        }
        public static bool TicksToTime(long ticks, out int hours, out int minutes, out int seconds)
        {
            if (ticks >= 0)
            {
                ticks /= 1000; // Convert to seconds
                hours = (int)(ticks / 3600);
                ticks %= 3600;
                minutes = (int)(ticks / 60);
                seconds = (int)(ticks % 60);
                return true;
            }
            else
            {
                hours = minutes = seconds = 0;
                return false;
            }
        }

        public static long TimeToTicks(int? hours = null, int? minutes = null, int? seconds = null)
        {
            if (hours == null || minutes == null || seconds == null)
            {
                var now = DateTime.Now;
                if (hours == null) hours = now.Hour;
                if (minutes == null) minutes = now.Minute;
                if (seconds == null) seconds = now.Second;
            }

            if (hours >= 0 && minutes >= 0 && seconds >= 0)
            {
                return 1000 * ((3600 * hours.Value) + (60 * minutes.Value) + seconds.Value);
            }
            else
            {
                throw new ArgumentException("Invalid time values.");
            }
        }


        public static int[] DaysInMonth(int year)
        {
            return new int[]
            {
            31, IsLeapYear(year) ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
            };
        }

        public static bool IsLeapYear(int year)
        {
            if (year < 1) throw new ArgumentException("Year must be a positive integer.");
            return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
        }

        public static DateTime EncodeFileTime(int month, int day, int year, int hour = 0, int minute = 0, int second = 0, int milliseconds = 0)
        {
            return new DateTime(year, month, day, hour, minute, second, milliseconds, DateTimeKind.Utc);
        }

        public static DateTime EncodeSystemTime(int month, int day, int year, int hour = 0, int minute = 0, int second = 0, int milliseconds = 0)
        {
            return new DateTime(year, month, day, hour, minute, second, milliseconds);
        }

        public static DateTime FileTimeToDateTime(Structs.FILETIMEALT fileTime)
        {
            try
            {
                // Combine HighDateTime and LowDateTime into a 64-bit value
                long fileTimeTicks = ((long)fileTime.HighDateTime << 32) | (uint)fileTime.LowDateTime;

                // Convert the file time to DateTime
                return DateTime.FromFileTime(fileTimeTicks);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Handle invalid FILETIME values
                Console.WriteLine($"Invalid FILETIME: {ex.Message}");
                return DateTime.MinValue;
            }
        }


        public static Structs.FILETIMEALT DateTimeToFileTime(DateTime dateTime)
        {
            long fileTimeTicks = dateTime.ToFileTime();
            return new Structs.FILETIMEALT
            {
                LowDateTime = (uint)(fileTimeTicks & 0xFFFFFFFF),
                HighDateTime = (uint)(fileTimeTicks >> 32)
            };
        }

        public static Structs.SYSTEMTIME FileTimeToSystemTime(Structs.FILETIMEALT fileTime)
        {
            Structs.SYSTEMTIME systemTime = new Structs.SYSTEMTIME();
            if (!Kernel32NativeMethods.FileTimeToSystemTime(ref fileTime, ref systemTime))
            {
                throw new InvalidOperationException("Failed to convert FILETIME to SYSTEMTIME.");
            }
            return systemTime;
        }

        public static Structs.FILETIMEALT SystemTimeToFileTime(Structs.SYSTEMTIME systemTime)
        {
            Structs.FILETIMEALT fileTime = new Structs.FILETIMEALT();
            if (!Kernel32NativeMethods.SystemTimeToFileTime(ref systemTime, ref fileTime))
            {
                throw new InvalidOperationException("Failed to convert SYSTEMTIME to FILETIME.");
            }
            return fileTime;
        }

        public static DateTime GetLocalTime()
        {
            Structs.SYSTEMTIME systemTime = new Structs.SYSTEMTIME();
            Kernel32NativeMethods.GetLocalTime(ref systemTime);
            return new DateTime(systemTime.Year, systemTime.Month, systemTime.Day, systemTime.Hour, systemTime.Minute, systemTime.Second, systemTime.Milliseconds);
        }

        public static int[] SystemTimeToArray(Structs.SYSTEMTIME systemTime)
        {
            return new int[]
            {
            systemTime.Month,
            systemTime.Day,
            systemTime.Year,
            systemTime.Hour,
            systemTime.Minute,
            systemTime.Second,
            systemTime.Milliseconds,
            (int)systemTime.DayOfWeek
            };
        }

        public static DateTime GetFileTime(IntPtr hFile)
        {
            Structs.FILETIMEALT creationTime, lastAccessTime, lastWriteTime;
            if (!Kernel32NativeMethods.GetFileTime(hFile, out creationTime, out lastAccessTime, out lastWriteTime))
            {
                throw new InvalidOperationException("Failed to retrieve file time.");
            }
            return FileTimeToDateTime(lastWriteTime);
        }

        public static bool IsValidDate(string date)
        {
            return DateTime.TryParseExact(
                date,
                new[] { "yyyy/MM/dd", "yyyy-MM-dd", "yyyy.MM.dd", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        public static void SplitDateTime(string date, out int[] dateParts, out int[] timeParts)
        {
            dateParts = new int[3];
            timeParts = new int[3];

            if (!IsValidDate(date))
                throw new ArgumentException("Invalid date format.");

            string[] dateTimeParts = date.Split(new[] { ' ', 'T' }, StringSplitOptions.RemoveEmptyEntries);
            if (dateTimeParts.Length > 0)
            {
                string[] dateSplit = dateTimeParts[0].Split(new[] { '/', '-', '.' });
                if (dateSplit.Length != 3)
                    throw new ArgumentException("Invalid date format.");

                for (int i = 0; i < 3; i++)
                    dateParts[i] = int.Parse(dateSplit[i]);
            }

            if (dateTimeParts.Length > 1)
            {
                string[] timeSplit = dateTimeParts[1].Split(':');
                if (timeSplit.Length < 2 || timeSplit.Length > 3)
                    throw new ArgumentException("Invalid time format.");

                for (int i = 0; i < timeSplit.Length; i++)
                    timeParts[i] = int.Parse(timeSplit[i]);

                if (timeSplit.Length == 2)
                    timeParts[2] = 0; // Seconds default to 0 if not provided
            }
        }

        public static double DateToDayValue(int year, int month, int day)
        {
            if (!IsValidDate($"{year:D4}/{month:D2}/{day:D2}"))
                throw new ArgumentException("Invalid date.");

            if (month < 3)
            {
                month += 12;
                year--;
            }

            int factorA = year / 100;
            int factorB = factorA / 4;
            int factorC = 2 - factorA + factorB;
            int factorE = (int)(1461 * (year + 4716) / 4);
            int factorF = (int)(153 * (month + 1) / 5);

            return factorC + day + factorE + factorF - 1524.5;
        }

        public static string DayValueToDate(double julianDayValue)
        {
            if (julianDayValue < 0)
                throw new ArgumentException("Julian day value must be positive.");

            int z = (int)(julianDayValue + 0.5);
            int w = (int)((z - 1867216.25) / 36524.25);
            int x = w / 4;
            int a = z + 1 + w - x;
            int b = a + 1524;
            int c = (int)((b - 122.1) / 365.25);
            int d = (int)(365.25 * c);
            int e = (int)((b - d) / 30.6001);
            int f = (int)(30.6001 * e);

            int day = b - d - f;
            int month = e < 13 ? e - 1 : e - 13;
            int year = month > 2 ? c - 4716 : c - 4715;

            return $"{year:D4}/{month:D2}/{day:D2}";
        }
        public static string GetFormattedDate()
        {
            DateTime now = DateTime.Now;
            return $"{now.Day}-{now.Month}-{now.Year} {now.Hour}:{now.Minute}:{now.Second}";
        }

        public static string GetFileDate(string filePath, bool isCreationDate)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return isCreationDate ? fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss") : fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetCreationDate(string path)
        {
            try
            {
                return Directory.GetCreationTime(path).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "Unknown Date";
            }
        }
    }
}

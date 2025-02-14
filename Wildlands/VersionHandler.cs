using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class VersionHandler
{
    public static void CheckVersion(string version)
    {
        string day = Regex.Replace(version, @" \(x64\) Version: (\d\d)-.+", "$1");
        string month = Regex.Replace(version, @" \(x64\) Version: ..-(\d\d)-.+", "$1");
        string year = Regex.Replace(version, @" \(x64\) Version: ..-..-(\d{4}).*", "$1");

        // Parse the date using the extracted day, month, and year
        DateTime versionDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

        // Calculate the difference in days
        TimeSpan dateDifference = DateTime.Now - versionDate;
        int daysDifference = (int)dateDifference.TotalDays;

        if (daysDifference > 60)
        {
            // Show a message box if the version is more than 60 days old
            MessageBox.Show($"{UpdateIndicator}\n\n{UpdateText} {daysDifference} {UpdateDays}", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Placeholder for the UpdateIndicator text
    private static readonly string UpdateIndicator = "Update Needed";
    private static readonly string UpdateText = "This version is ";
    private static readonly string UpdateDays = "days old.";

}

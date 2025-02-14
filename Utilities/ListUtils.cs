using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ListUtils
{
    public static void AddToList<T>(ref List<T> list, T value0)
    {
        // Add a single value to the 1D list
        list.Add(value0);
    }

    public static void AddToList<T1, T2>(ref List<Tuple<T1, T2>> list, T1 value0, T2 value1)
    {
        // Add a pair of values to the 2D list (simulated as a list of tuples)
        list.Add(new Tuple<T1, T2>(value0, value1));
    }
    public static bool ListToMask(ref string mask, string list)
    {
        // Check for invalid characters in the input list
        if (Regex.IsMatch(list, @"[\\|/:<>|]"))
            return false;

        // Normalize the list: trim whitespace, replace semicolons with "|", escape regex special characters
        list = Regex.Replace(list, @"\s*;\s*", "|");
        list = Regex.Replace(list, @"[^\w\s|.*?]", match => "\\" + match.Value); // Escape special characters
        list = list.Replace("?", ".").Replace("*", ".*?");

        // Create the regex mask
        mask = $"(?i)^({list})\\z"; // Case-insensitive match
        return true;
    }
}
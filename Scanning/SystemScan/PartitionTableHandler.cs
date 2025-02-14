using System;
using System.IO;
using System.Text.RegularExpressions;

public class PartitionTableHandler
{
    public static void ProcessPartitionTable(string parts)
    {
        int p = 0;
        int partNr = 1;
        string size = string.Empty;
        string size1 = string.Empty;

        while (true)
        {
            string sus = string.Empty;
            if (p > 96) break;

            string a = "(Not Active) - ";
            string part = Regex.Replace(parts, $".{{{p}}}(.{{32}}).*", "$1");
            if (!Regex.IsMatch(part, "00000000000000000000000000000000"))
            {
                string type = Regex.Replace(part, ".{8}(.{2}).+", "$1");
                if (type == "EE")
                {
                    File.AppendAllText(Path.Combine(Path.GetTempPath(), "readmbr"), "\r\nPartition: GPT.\r\n");
                }
                else
                {
                    if (Regex.IsMatch(part, @"\A8.+")) a = "(Active) - ";
                    if (type == "0c" || type == "0b") type = "FAT32";
                    if (type == "0F") type = "0F Extended";
                    if (type == "07") type = "07 NTFS";

                    size = Regex.Replace(part, @".{24}(.{8})", "$1");
                    size = Regex.Replace(size, @"(.{2})(.{2})(.{2})(.{2})", "$4$3$2$1");
                    size1 = Convert.ToInt64(size, 16).ToString();
                    long sizeInt = Convert.ToInt64(size1);

                    if (sizeInt < 0) sizeInt = Convert.ToInt64(size, 16);
                    sizeInt *= 512;

                    if (sizeInt < 3000)
                    {
                        if (sizeInt > 0) size = sizeInt + " byte";
                        sus = "1";
                    }
                    else
                    {
                        sizeInt /= 1024;
                        if (sizeInt < 3000)
                        {
                            size = Math.Round((double)sizeInt) + " KB";
                            sus = "1";
                        }
                        else
                        {
                            sizeInt /= 1024;
                            if (sizeInt < 1024)
                            {
                                size = Math.Round((double)sizeInt) + " MB"; // Explicitly cast to double
                                if (sizeInt < 90) sus = "1";
                            }
                            else
                            {
                                size = Math.Round(sizeInt / 1024.0, 1) + " GB"; // Use double division for precision
                            }

                        }
                    }

                    string pihar = string.Empty;
                    if (type == "00" && size == "0 byte")
                    {
                        partNr++;
                        File.AppendAllText(Path.Combine(Path.GetTempPath(), "readmbr"),
                            $"Partition 00: {a}(Size={size}) - (Type={type}) ATTENTION ===> 0 byte partition bootkit.\r\n");
                    }
                    else
                    {
                        if (type == "17" && sus == "1") pihar = " ===> Suspicious partition bootkit on partition " + partNr;
                        File.AppendAllText(Path.Combine(Path.GetTempPath(), "readmbr"),
                            $"Partition {partNr}: {a}(Size={size}) - (Type={type}) {pihar}\r\n");
                    }
                }
            }
            p += 32;
            partNr++;
        }
    }
}

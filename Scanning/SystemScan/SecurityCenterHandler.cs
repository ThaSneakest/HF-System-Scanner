using System;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class SecurityCenterHandler
{
    private const string HADDITION = @"C:\Temp\addition.txt";  // Path to the output file
    private string COMERR = "";
    private const string HFIXLOG = @"C:\Temp\fixlog.txt"; // Path to log file
    private const string NFOUND = "Not Found";
    private const string ERDEL = "Error Deleting";
    private const string ITEMPRO = "Item Processed";
    private const string OSNUM = "10"; // Example OS version, replace as needed
    private string FIX = "AV: {1234-5678-90AB-CDEF}"; // Example FIX string, modify as needed


    public void SECCENT()
    {
        // Write initial section header to the file
        File.AppendAllText(HADDITION, Environment.NewLine + "==================== Security Center ========================" + Environment.NewLine);
        File.AppendAllText(HADDITION, Environment.NewLine + "(Security Center Information)" + Environment.NewLine + Environment.NewLine);

        // Call helper function for different product categories
        SECCENTLIST("AntiVirusProduct");
        if (COMERR == "5") COMERR = "";  // Reset error code
        SECCENTLIST("AntiSpywareProduct");
        if (COMERR == "5") COMERR = "";  // Reset error code
        SECCENTLIST("FirewallProduct");
        if (COMERR == "5") COMERR = "";  // Reset error code
    }

    public void SECCENTFIX()
    {
        // Reset COMERR to an empty string
        COMERR = "";

        // Call the helper function for security center fix
        SECCENTFIXIT();

        // If COMERR equals "5", reset it to an empty string
        if (COMERR == "5")
        {
            COMERR = "";
        }
    }

    public void SECCENTFIXIT()
    {
        // Extract product type (AV, AS, FW) and GUID from FIX string
        string pro = Regex.Replace(FIX, @"\A(..): .+", "$1");
        string gui = Regex.Replace(FIX, @"..: [^{]+(\{.+\})", "$1");

        // Map product type codes to full names
        if (pro == "AV") pro = "AntiVirusProduct";
        if (pro == "AS") pro = "AntiSpywareProduct";
        if (pro == "FW") pro = "FirewallProduct";

        // Create instance query string
        string strInstance = $"{pro}.instanceGuid='{gui}'";

        // Determine SecurityCenter class name based on OS version
        string secClass = (int.Parse(OSNUM) < 6) ? "SecurityCenter" : "SecurityCenter2";

        // Connect to WMI service
        ManagementScope managementScope = new ManagementScope(@"\\.\root\" + secClass);
        managementScope.Connect();
        ObjectGet(managementScope, pro, gui, strInstance);
    }

    private void ObjectGet(ManagementScope managementScope, string pro, string gui, string strInstance)
    {
        // Query WMI for instances of the product
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, new ObjectQuery($"Select * from {pro}"));
        ManagementObjectCollection devColItems = searcher.Get();

        bool exists = false;

        // Check if the instance exists in the WMI collection
        foreach (ManagementObject obj in devColItems)
        {
            if (obj["instanceGuid"].ToString() == gui)
            {
                exists = true;
            }
        }

        // If instance doesn't exist, log it as not found
        if (!exists)
        {
            File.AppendAllText(HFIXLOG, FIX + " => " + NFOUND + Environment.NewLine);
        }
        else
        {
            // Delete the instance
            DeleteInstance(managementScope, strInstance, pro, gui);
        }
    }

    private void DeleteInstance(ManagementScope managementScope, string strInstance, string pro, string gui)
    {
        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, new ObjectQuery($"Select * from {pro}"));
            ManagementObjectCollection devColItems = searcher.Get();

            // Find the instance and delete it
            foreach (ManagementObject obj in devColItems)
            {
                if (obj["instanceGuid"].ToString() == gui)
                {
                    obj.Delete(); // Delete the instance
                    File.AppendAllText(HFIXLOG, FIX + " => " + ITEMPRO + Environment.NewLine);
                    break;
                }
            }

            // Verify deletion
            VerifyDeletion(managementScope, pro, gui);
        }
        catch (Exception ex)
        {
            File.AppendAllText(HFIXLOG, FIX + " => " + ERDEL + " . Error: " + ex.Message + Environment.NewLine);
        }
    }

    private void VerifyDeletion(ManagementScope managementScope, string pro, string gui)
    {
        // Reconnect to WMI and verify if the instance still exists
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, new ObjectQuery($"Select * from {pro}"));
        ManagementObjectCollection devColItems = searcher.Get();

        bool del = false;
        foreach (ManagementObject obj in devColItems)
        {
            if (obj["instanceGuid"].ToString() == gui)
            {
                del = true;
                File.AppendAllText(HFIXLOG, FIX + " => " + ITEMPRO + Environment.NewLine);
            }
        }

        // If the instance is still there, log the deletion failure
        if (!del)
        {
            RegistryKeyHandler.DeleteRegistryKey(FIX);
        }
    }

    public void SECCENTLIST(string product)
    {
        ManagementScope managementScope;
        if (int.Parse(OSNUM) < 6)
        {
            managementScope = new ManagementScope(@"\\.\root\SecurityCenter");
        }
        else
        {
            managementScope = new ManagementScope(@"\\.\root\SecurityCenter2");
        }

        try
        {
            managementScope.Connect();

            // Query for the specific product (AntiVirusProduct, AntiSpywareProduct, or FirewallProduct)
            ObjectQuery query = new ObjectQuery($"Select * from {product}");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query);
            ManagementObjectCollection devColItems = searcher.Get();

            if (devColItems != null)
            {
                foreach (ManagementObject obj in devColItems)
                {
                    string pro = string.Empty;
                    string test = string.Empty;
                    string name = obj["displayName"]?.ToString();
                    string state1 = string.Empty, state2 = string.Empty;
                    string onAccessScanningEnabled = obj["onAccessScanningEnabled"]?.ToString();
                    string productUptoDate = obj["productUptoDate"]?.ToString();
                    string gui = obj["instanceGuid"]?.ToString();

                    // Process the information based on the OS version
                    if (int.Parse(OSNUM) < 6)
                    {
                        // Check for onAccessScanningEnabled and productUptoDate
                        state1 = onAccessScanningEnabled == "True" ? "Enabled" : "Disabled";
                        state2 = productUptoDate == "True" ? " - Up to date" : " - Out of date";
                    }
                    else
                    {
                        // Process the state for newer versions
                        int state = Convert.ToInt32(obj["productState"]);
                        string testHex = state.ToString("X");

                        state1 = Regex.IsMatch(testHex, @"....(.)...") ? "Enabled" : "Disabled";
                        state2 = Regex.IsMatch(testHex, @"......(.).") ? " - Out of date" : " - Up to date";
                    }

                    // Assign product type based on input
                    if (product == "AntiVirusProduct") pro = "AV";
                    if (product == "AntiSpywareProduct") pro = "AS";
                    if (product == "FirewallProduct")
                    {
                        pro = "FW";
                        state2 = string.Empty;  // No status for firewall
                    }

                    // Log the product information
                    if (!string.IsNullOrEmpty(name))
                    {
                        string logMessage = $"{pro}: {name} ({state1}{state2}) {gui}";
                        File.AppendAllText(HADDITION, logMessage + Environment.NewLine);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur
            File.AppendAllText(HADDITION, "Error: " + ex.Message + Environment.NewLine);
        }
    }
}

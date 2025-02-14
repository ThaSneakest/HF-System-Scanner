using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Scripting.Directives;

namespace Wildlands_System_Scanner.Scripting
{
    public class ScriptProcessor
    {
        public static void ProcessFixScript()
        {
            string filePath = "WildlandsFixScript.txt"; // Path to the fix script file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Fix script file not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                bool isProcessingScript = false;

                foreach (string line in lines)
                {
                    if (line.StartsWith("StartScript::"))
                    {
                        Console.WriteLine("StartScript:: directive found. Beginning fix script execution.");
                        isProcessingScript = true;
                        continue; // Skip the StartScript:: line itself
                    }

                    if (line.StartsWith("EndScript::"))
                    {
                        Console.WriteLine("EndScript:: directive found. Ending fix script execution.");
                        isProcessingScript = false;
                        break; // End processing after this point
                    }

                    if (isProcessingScript)
                    {
                        DoDirective(line);
                    }
                }

                if (!isProcessingScript)
                {
                    Console.WriteLine("Script processing completed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the script: {ex.Message}");
            }
        }

        private static void DoDirective(string line)
        {
            try
            {
                // Trim the line to remove leading/trailing whitespaces
                line = line.Trim();

                // Process specific directives based on line contents
                if (line.StartsWith("Process::"))
                {
                    Console.WriteLine("Process Directive found.");
                    ProcessDirective.ProcessKill();
                }
                if (line.StartsWith("KillAll::"))
                {
                    Console.WriteLine("KillAll Directive found.");
                    KillAllDirective.KillAll();
                }
                if (line.StartsWith("SnapShot::"))
                {
                    Console.WriteLine("SnapShot Directive found.");
                    SnapShotDirective.ProcessDirectives();
                }
                if (line.StartsWith("SystemRestore::"))
                {
                    Console.WriteLine("SystemRestore Directive found.");
                    SystemRestoreDirective.ProcessDirectives();
                }
                if (line.StartsWith("StepDelete::"))
                {
                    Console.WriteLine("StepDelete Directive found.");
                    StepDeleteDirective.ProcessDirectives();
                }
                if (line.StartsWith("NoOrphanRemoval::"))
                {
                    Console.WriteLine("NoOrphanRemoval Directive found.");
                    NoOrphanRemovalDirective.ProcessDirectives();
                }
                if (line.StartsWith("SkipFix::"))
                {
                    Console.WriteLine("SkipFix Directive found.");
                    SkipFixDirective.ProcessDirectives();
                }
                if (line.StartsWith("FixWSS::"))
                {
                    Console.WriteLine("FixWSS Directive found.");
                    FixWSSDirective.ProcessDirectives();
                }
                if (line.StartsWith("Reboot::"))
                {
                    Console.WriteLine("Reboot Directive found.");
                    RebootDirective.ProcessDirectives();
                }
                if (line.StartsWith("NetSvc::"))
                {
                    Console.WriteLine("NetSvc Directive found.");
                    NetSvcDirective.ProcessDirectives();
                }
                if (line.StartsWith("ADS::"))
                {
                    Console.WriteLine("ADS Directive found.");
                    ADSDirective.ProcessDirectives();
                }
                if (line.StartsWith("Driver::"))
                {
                    Console.WriteLine("Driver Directive found.");
                    DriverDirective.ProcessDirectives();
                }
                if (line.StartsWith("DeQuarantine::"))
                {
                    Console.WriteLine("DeQuarantine Directive found.");
                    DeQuarantineDirective.ProcessDirectives();
                }
                if (line.StartsWith("QuickQuit::"))
                {
                    Console.WriteLine("QuickQuit Directive found.");
                    QuickQuitDirective.ProcessDirectives();
                }
                if (line.StartsWith("RegUnlock::"))
                {
                    Console.WriteLine("RegUnlock Directive found.");
                    RegUnlockDirective.ProcessDirectives();
                }
                if (line.StartsWith("LockedRegDelete::"))
                {
                    Console.WriteLine("LockedRegDelete Directive found.");
                    LockedRegDeleteDirective.ProcessDirectives();
                }
                if (line.StartsWith("RegNull::"))
                {
                    Console.WriteLine("RegNull Directive found.");
                    RegNullDirective.ProcessDirectives();
                }
                if (line.StartsWith("Registry::"))
                {
                    Console.WriteLine("Registry Directive found.");
                    RegistryDirective.ProcessDirectives();
                }
                if (line.StartsWith("DeleteRegKey::"))
                {
                    Console.WriteLine("DeleteRegKey Directive found.");
                    DeleteRegistryKeyDirective.ProcessDirectives();
                }
                if (line.StartsWith("DeleteRegValue::"))
                {
                    Console.WriteLine("DeleteRegValue Directive found.");
                    DeleteRegistryValueDirective.ProcessDirectives();
                }
                if (line.StartsWith("FileLook::"))
                {
                    Console.WriteLine("FileLook Directive found.");
                    FileLookDirective.ProcessDirectives();
                }
                if (line.StartsWith("FolderLook::"))
                {
                    Console.WriteLine("FolderLook Directive found.");
                    FolderLookDirective.ProcessDirectives();
                }
                if (line.StartsWith("FindFolder::"))
                {
                    Console.WriteLine("FindFolder Directive found.");
                    FindFolderDirective.ProcessDirectives();
                }
                if (line.StartsWith("SignatureCheck::"))
                {
                    Console.WriteLine("SignatureCheck Directive found.");
                    SignatureCheckDirective.ProcessDirectives();
                }
                if (line.StartsWith("SRCopy::"))
                {
                    Console.WriteLine("SRCopy Directive found.");
                    SRCopyDirective.ProcessDirectives();
                }
                if (line.StartsWith("BackupCopy::"))
                {
                    Console.WriteLine("BackupCopy Directive found.");
                    BackupCopyDirective.ProcessDirectives();
                }
                if (line.StartsWith("Copy::"))
                {
                    Console.WriteLine("Copy Directive found.");
                    CopyDirective.ProcessDirectives();
                }
                if (line.StartsWith("Move::"))
                {
                    Console.WriteLine("Move Directive found.");
                    MoveDirective.ProcessDirectives();
                }
                if (line.StartsWith("Domains::"))
                {
                    Console.WriteLine("Domains Directive found.");
                    DomainsDirective.ProcessDirectives();
                }
                if (line.StartsWith("Firefox::"))
                {
                    Console.WriteLine("Firefox Directive found.");
                    FirefoxDirective.ProcessDirectives();
                }
                if (line.StartsWith("Chrome::"))
                {
                    Console.WriteLine("Chrome Directive found.");
                    ChromeDirective.ProcessDirectives();
                }
                if (line.StartsWith("Edge::"))
                {
                    Console.WriteLine("Edge Directive found.");
                    EdgeDirective.ProcessDirectives();
                }
                if (line.StartsWith("Security::"))
                {
                    Console.WriteLine("Security Directive found.");
                    SecurityDirective.ProcessDirectives();
                }
                if (line.StartsWith("Missing::"))
                {
                    Console.WriteLine("Missing Directive found.");
                    MissingDirective.ProcessDirectives();
                }
                if (line.StartsWith("Rootkit::"))
                {
                    Console.WriteLine("Rootkit Directive found.");
                    RootkitDirective.ProcessDirectives();
                }
                if (line.StartsWith("Collect::"))
                {
                    Console.WriteLine("Collect Directive found.");
                    CollectDirective.ProcessDirectives();
                }
                if (line.StartsWith("Suspect::"))
                {
                    Console.WriteLine("Suspect Directive found.");
                    SuspectDirective.ProcessDirectives();
                }
                if (line.StartsWith("Extra::"))
                {
                    Console.WriteLine("Extra Directive found.");
                    ExtraDirective.ProcessDirectives();
                }
                if (line.StartsWith("Command::"))
                {
                    Console.WriteLine("Command Directive found.");
                    CommandDirective.ProcessDirectives();
                }
                if (line.StartsWith("StartBatch::"))
                {
                    Console.WriteLine("StartBatch Directive found.");
                    BatchDirective.ProcessDirectives();
                }
                if (line.StartsWith("CreateDummy::"))
                {
                    Console.WriteLine("CreateDummy Directive found.");
                    CreateDummyDirective.ProcessDirectives();
                }
                if (line.StartsWith("DeleteJunctions::"))
                {
                    Console.WriteLine("DeleteJunctions Directive found.");
                    DeleteJunctionsDirective.ProcessDirectives();
                }
                if (line.StartsWith("DeleteQuarantine::"))
                {
                    Console.WriteLine("DeleteQuarantine Directive found.");
                    DeleteQuarantineDirective.ProcessDirectives();
                }
                if (line.StartsWith("Service::"))
                {
                    Console.WriteLine("Service Directive found.");
                    ServiceDirective.ProcessDirectives();
                }
                if (line.StartsWith("ClearEventLogs::"))
                {
                    Console.WriteLine("ClearEventLogs Directive found.");
                    ClearEventLogDirective.ProcessDirectives();
                }
                if (line.StartsWith("EmptyTemp::"))
                {
                    Console.WriteLine("EmptyTemp Directive found.");
                    EmptyTempDirective.ProcessDirectives();
                }
                if (line.StartsWith("ExportRegKey::"))
                {
                    Console.WriteLine("ExportRegKey Directive found.");
                    ExportRegistryKeyDirective.ProcessDirectives();
                }
                if (line.StartsWith("ExportRegValue::"))
                {
                    Console.WriteLine("ExportRegValue Directive found.");
                    ExportRegValueDirective.ProcessDirectives();
                }
                if (line.StartsWith("Hosts::"))
                {
                    Console.WriteLine("Hosts Directive found.");
                    HostsDirective.ProcessDirectives();
                }
                if (line.StartsWith("ListFolderPermissions::"))
                {
                    Console.WriteLine("ListFolderPermissions Directive found.");
                    ListFolderPermissionsDirective.ProcessDirectives();
                }
                if (line.StartsWith("ListFilePermissions::"))
                {
                    Console.WriteLine("ListFilePermissions Directive found.");
                    ListFilePermissionsDirective.ProcessDirectives();
                }
                if (line.StartsWith("ListRegPermission::"))
                {
                    Console.WriteLine("ListRegPermission Directive found.");
                    ListRegPermissionDirective.ProcessDirectives();
                }
                if (line.StartsWith("Powershell::"))
                {
                    Console.WriteLine("Powershell Directive found.");
                    PowershellDirective.ProcessDirectives();
                }
                if (line.StartsWith("PowershellScript::"))
                {
                    Console.WriteLine("PowershellScript Directive found.");
                    PowershellScriptDirective.ProcessDirectives();
                }
                if (line.StartsWith("StartPowershell::"))
                {
                    Console.WriteLine("StartPowershell Directive found.");
                    PowershellScriptBuilderDirective.ProcessDirectives();
                }
                if (line.StartsWith("StartVBScript::"))
                {
                    Console.WriteLine("StartVBScript Directive found.");
                    VBScriptDirective.ProcessDirectives();
                }
                if (line.StartsWith("RemoveProxy::"))
                {
                    Console.WriteLine("RemoveProxy Directive found.");
                    RemoveProxyDirective.ProcessDirectives();
                }
                if (line.StartsWith("RestoreFromBackup::"))
                {
                    Console.WriteLine("RestoreFromBackup Directive found.");
                    RestoreFromBackupDirective.ProcessDirectives();
                }
                if (line.StartsWith("SystemRestoreOnOff::"))
                {
                    Console.WriteLine("SystemRestoreOnOff Directive found.");
                    SystemRestoreOnOffDirectives.ProcessDirectives();
                }
                if (line.StartsWith("Unlock::"))
                {
                    Console.WriteLine("Unlock Directive found.");
                    UnlockDirective.ProcessDirectives();
                }
                if (line.StartsWith("VirusScan::"))
                {
                    Console.WriteLine("VirusScan Directive found.");
                    VirusScanDirective.ProcessDirectives();
                }
                if (line.StartsWith("Zip::"))
                {
                    Console.WriteLine("Zip Directive found.");
                    ZipDirective.ProcessDirectives();
                }
                if (line.StartsWith("MBR::"))
                {
                    Console.WriteLine("MBR Directive found.");
                    MBRDirective.ProcessDirectives();
                }
                if (line.StartsWith("MBRLook::"))
                {
                    Console.WriteLine("MBRLook Directive found.");
                    MBRLookDirective.ProcessDirectives();
                }
                if (line.StartsWith("SetPermissions::"))
                {
                    Console.WriteLine("SetPermissions Directive found.");
                    SetPermissionsDirective.ProcessDirectives();
                }
                if (line.StartsWith("StartRegedit::"))
                {
                    Console.WriteLine("StartRegedit Directive found.");
                    RegeditDirective.ProcessDirectives();
                }
                if (line.StartsWith("ListSymLink::"))
                {
                    Console.WriteLine("ListSymLink Directive found.");
                    SymLinkDirective.ProcessDirectives();
                }
                if (line.StartsWith("TaskDetails::"))
                {
                    Console.WriteLine("TaskDetails Directive found.");
                    TaskDetailsDirective.ProcessDirectives();
                }
                if (line.StartsWith("TestSigningOn::"))
                {
                    Console.WriteLine("TestSigningOn Directive found.");
                    TestSigningOnDirective.ProcessDirectives();
                }
                if (line.StartsWith("RenV::"))
                {
                    Console.WriteLine("RenV Directive found.");
                    RenVDirective.ProcessDirectives();
                }
                if (line.StartsWith("TDL::"))
                {
                    Console.WriteLine("TDL Directive found.");
                    TDLDirective.ProcessDirectives();
                }
                if (line.StartsWith("FakeSmoke::"))
                {
                    Console.WriteLine("FakeSmoke Directive found.");
                    FakeSmokeDirective.ProcessDirectives();
                }
                if (line.StartsWith("AWF::"))
                {
                    Console.WriteLine("AWF Directive found.");
                    AWFDirective.ProcessDirectives();
                }
                if (line.StartsWith("Vundo::"))
                {
                    Console.WriteLine("Vundo Directive found.");
                    VundoDirective.ProcessDirectives();
                }
                if (line.StartsWith("AtJob::"))
                {
                    Console.WriteLine("AtJob Directive found.");
                    AtJobDirective.ProcessDirectives();
                }
                else
                {
                    Console.WriteLine($"Unknown directive: {line}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directive '{line}': {ex.Message}");
            }
        }
    }
}

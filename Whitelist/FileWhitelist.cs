﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Whitelist
{
    public class FileWhitelist
    {
        public static readonly HashSet<string> WhitelistFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add specific file paths to the whitelist
            "C:\\SafeFolder\\safe_file.txt",
            "C:\\SafeFolder\\subfolder\\another_safe_file.log",
            @"C:\Windows\System32\ApplyTrustOffline.exe",
@"C:\Windows\System32\AppXApplicabilityBlob.dll",
@"C:\Windows\System32\AppXDeploymentExtensions.desktop.dll",
@"C:\Windows\System32\AppXDeploymentExtensions.onecore.dll",
@"C:\Windows\System32\AppXDeploymentServer.dll",
@"C:\Windows\System32\bcrypt.dll",
@"C:\Windows\System32\bdesvc.dll",
@"C:\Windows\System32\BdeUISrv.exe",
@"C:\Windows\System32\BootMenuUX.dll",
@"C:\Windows\System32\cdp.dll",
@"C:\Windows\System32\ci.dll",
@"C:\Windows\System32\ClipUp.exe",
@"C:\Windows\System32\combase.dll",
@"C:\Windows\System32\ConfigureExpandedStorage.dll",
@"C:\Windows\System32\cscobj.dll",
@"C:\Windows\System32\cscript.exe",
@"C:\Windows\System32\cscsvc.dll",
@"C:\Windows\System32\cscui.dll",
@"C:\Windows\System32\CustomInstallExec.exe",
@"C:\Windows\System32\d2d1debug3.dll",
@"C:\Windows\System32\d3d11_3SDKLayers.dll",
@"C:\Windows\System32\d3d12SDKLayers.dll",
@"C:\Windows\System32\D3DCompiler_33.dll",
@"C:\Windows\System32\D3DCompiler_34.dll",
@"C:\Windows\System32\D3DCompiler_35.dll",
@"C:\Windows\System32\D3DCompiler_36.dll",
@"C:\Windows\System32\D3DCompiler_37.dll",
@"C:\Windows\System32\D3DCompiler_38.dll",
@"C:\Windows\System32\D3DCompiler_39.dll",
@"C:\Windows\System32\D3DCompiler_40.dll",
@"C:\Windows\System32\D3DCompiler_41.dll",
@"C:\Windows\System32\D3DCompiler_42.dll",
@"C:\Windows\System32\d3dconfig.exe",
@"C:\Windows\System32\d3dcsx_42.dll",
@"C:\Windows\System32\d3dx10.dll",
@"C:\Windows\System32\d3dx10_33.dll",
@"C:\Windows\System32\d3dx10_34.dll",
@"C:\Windows\System32\d3dx10_35.dll",
@"C:\Windows\System32\d3dx10_36.dll",
@"C:\Windows\System32\d3dx10_37.dll",
@"C:\Windows\System32\d3dx10_38.dll",
@"C:\Windows\System32\d3dx10_39.dll",
@"C:\Windows\System32\d3dx10_40.dll",
@"C:\Windows\System32\d3dx10_41.dll",
@"C:\Windows\System32\d3dx10_42.dll",
@"C:\Windows\System32\d3dx11_42.dll",
@"C:\Windows\System32\d3dx9_24.dll",
@"C:\Windows\System32\d3dx9_25.dll",
@"C:\Windows\System32\d3dx9_26.dll",
@"C:\Windows\System32\d3dx9_27.dll",
@"C:\Windows\System32\d3dx9_28.dll",
@"C:\Windows\System32\d3dx9_29.dll",
@"C:\Windows\System32\d3dx9_30.dll",
@"C:\Windows\System32\d3dx9_32.dll",
@"C:\Windows\System32\d3dx9_33.dll",
@"C:\Windows\System32\d3dx9_34.dll",
@"C:\Windows\System32\d3dx9_35.dll",
@"C:\Windows\System32\d3dx9_36.dll",
@"C:\Windows\System32\D3DX9_37.dll",
@"C:\Windows\System32\D3DX9_38.dll",
@"C:\Windows\System32\D3DX9_39.dll",
@"C:\Windows\System32\D3DX9_40.dll",
@"C:\Windows\System32\D3DX9_41.dll",
@"C:\Windows\System32\D3DX9_42.dll",
@"C:\Windows\System32\dciman32.dll",
@"C:\Windows\System32\DirectML.Debug.dll",
@"C:\Windows\System32\dmenrollengine.dll",
@"C:\Windows\System32\dnsapi.dll",
@"C:\Windows\System32\dnsrslvr.dll",
@"C:\Windows\System32\DrtmAuthTxt.wim",
@"C:\Windows\System32\drvstore.dll",
@"C:\Windows\System32\DXCap.exe",
@"C:\Windows\System32\DXCaptureReplay.dll",
@"C:\Windows\System32\DXCpl.exe",
@"C:\Windows\System32\DXGIDebug.dll",
@"C:\Windows\System32\DXToolsMonitor.dll",
@"C:\Windows\System32\DXToolsOfflineAnalysis.dll",
@"C:\Windows\System32\DxToolsReportGenerator.dll",
@"C:\Windows\System32\DXToolsReporting.dll",
@"C:\Windows\System32\EdgeContent.dll",
@"C:\Windows\System32\edgehtml.dll",
@"C:\Windows\System32\edgeIso.dll",
@"C:\Windows\System32\ExplorerFrame.dll",
@"C:\Windows\System32\fhcpl.dll",
@"C:\Windows\System32\fontdrvhost.exe",
@"C:\Windows\System32\fontsub.dll",
@"C:\Windows\System32\g711codc.ax",
@"C:\Windows\System32\GameInput.dll",
@"C:\Windows\System32\GameInputInbox.dll",
@"C:\Windows\System32\GdiPlus.dll",
@"C:\Windows\System32\hmkd.dll",
@"C:\Windows\System32\HostNetSvc.dll",
@"C:\Windows\System32\hvax64.exe",
@"C:\Windows\System32\hvix64.exe",
@"C:\Windows\System32\ie4uinit.exe",
@"C:\Windows\System32\ie4ushowIE.exe",
@"C:\Windows\System32\ieframe.dll",
@"C:\Windows\System32\iemigplugin.dll",
@"C:\Windows\System32\iernonce.dll",
@"C:\Windows\System32\iertutil.dll",
@"C:\Windows\System32\IESettingSync.exe",
@"C:\Windows\System32\iesetup.dll",
@"C:\Windows\System32\IndexedDbLegacy.dll",
@"C:\Windows\System32\inetcpl.cpl",
@"C:\Windows\System32\iphlpsvc.dll",
@"C:\Windows\System32\ISM.dll",
@"C:\Windows\System32\IumSdk.dll",
@"C:\Windows\System32\KerbClientShared.dll",
@"C:\Windows\System32\kerberos.dll",
@"C:\Windows\System32\KernelBase.dll",
@"C:\Windows\System32\ksproxy.ax",
@"C:\Windows\System32\ListSvc.dll",
@"C:\Windows\System32\LocationFramework.dll",
@"C:\Windows\System32\LocationFrameworkInternalPS.dll",
@"C:\Windows\System32\LocationFrameworkPS.dll",
@"C:\Windows\System32\lpk.dll",
@"C:\Windows\System32\lsasrv.dll",
@"C:\Windows\System32\lsass.exe",
@"C:\Windows\System32\lsm.dll",
@"C:\Windows\System32\mfcore.dll",
@"C:\Windows\System32\mfksproxy.dll",
@"C:\Windows\System32\mmc.exe",
@"C:\Windows\System32\mshtml.dll",
@"C:\Windows\System32\msi.dll",
@"C:\Windows\System32\msieftp.dll",
@"C:\Windows\System32\msimsg.dll",
@"C:\Windows\System32\msIso.dll",
@"C:\Windows\System32\msscntrs.dll",
@"C:\Windows\System32\mssitlb.dll",
@"C:\Windows\System32\mssph.dll",
@"C:\Windows\System32\mssprxy.dll",
@"C:\Windows\System32\mssrch.dll",
@"C:\Windows\System32\mssvp.dll",
@"C:\Windows\System32\negoexts.dll",
@"C:\Windows\System32\NgcCtnrSvc.dll",
@"C:\Windows\System32\NgcIso.exe",
@"C:\Windows\System32\NgcIsoCtnr.dll",
@"C:\Windows\System32\ntoskrnl.exe",
@"C:\Windows\System32\nvspinfo.exe",
@"C:\Windows\System32\offlinelsa.dll",
@"C:\Windows\System32\ole32.dll",
@"C:\Windows\System32\PeerDist.dll",
@"C:\Windows\System32\PeerDistAD.dll",
@"C:\Windows\System32\PeerDistCleaner.dll",
@"C:\Windows\System32\PeerDistHttpTrans.dll",
@"C:\Windows\System32\PeerDistSvc.dll",
@"C:\Windows\System32\PeerDistWSDDiscoProv.dll",
@"C:\Windows\System32\perf_gputiming.dll",
@"C:\Windows\System32\pku2u.dll",
@"C:\Windows\System32\poqexec.exe",
@"C:\Windows\System32\Print.PrintSupport.Source.dll",
@"C:\Windows\System32\Print.Workflow.Source.dll",
@"C:\Windows\System32\PrintWorkflowService.dll",
@"C:\Windows\System32\propsys.dll",
@"C:\Windows\System32\rasmans.dll",
@"C:\Windows\System32\rdpserverbase.dll",
@"C:\Windows\System32\remotesp.tsp",
@"C:\Windows\System32\rpcss.dll",
@"C:\Windows\System32\rtpm.dll",
@"C:\Windows\System32\SDFHost.dll",
@"C:\Windows\System32\Search.ProtocolHandler.MAPI2.dll",
@"C:\Windows\System32\SearchFilterHost.exe",
@"C:\Windows\System32\SearchFolder.dll",
@"C:\Windows\System32\SearchIndexer.exe",
@"C:\Windows\System32\SearchProtocolHost.exe",
@"C:\Windows\System32\SecConfig.efi",
@"C:\Windows\System32\securekernel.exe",
@"C:\Windows\System32\sendmail.dll",
@"C:\Windows\System32\setupapi.dll",
@"C:\Windows\System32\SgrmBroker.exe",
@"C:\Windows\System32\SgrmEnclave.dll",
@"C:\Windows\System32\SgrmEnclave_secure.dll",
@"C:\Windows\System32\SgrmLpac.exe",
@"C:\Windows\System32\shell32.dll",
@"C:\Windows\System32\skci.dll",
@"C:\Windows\System32\SmartcardCredentialProvider.dll",
@"C:\Windows\System32\smartscreen.exe",
@"C:\Windows\System32\smartscreenps.dll",
@"C:\Windows\System32\smss.exe",
@"C:\Windows\System32\SpeechPal.dll",
@"C:\Windows\System32\sppobjs.dll",
@"C:\Windows\System32\sppsvc.exe",
@"C:\Windows\System32\sqlsrv32.dll",
@"C:\Windows\System32\srchadmin.dll",
@"C:\Windows\System32\ssdpapi.dll",
@"C:\Windows\System32\ssdpsrv.dll",
@"C:\Windows\System32\sspicli.dll",
@"C:\Windows\System32\sspisrv.dll",
@"C:\Windows\System32\tapi3.dll",
@"C:\Windows\System32\tapi32.dll",
@"C:\Windows\System32\tcblaunch.exe",
@"C:\Windows\System32\tcbloader.dll",
@"C:\Windows\System32\themecpl.dll",
@"C:\Windows\System32\themeui.dll",
@"C:\Windows\System32\TpmEngUM.dll",
@"C:\Windows\System32\TpmEngUM138.dll",
@"C:\Windows\System32\TpmTasks.dll",
@"C:\Windows\System32\tquery.dll",
@"C:\Windows\System32\udhisapi.dll",
@"C:\Windows\System32\uDWM.dll",
@"C:\Windows\System32\upnpcont.exe",
@"C:\Windows\System32\upnphost.dll",
@"C:\Windows\System32\urlmon.dll",
@"C:\Windows\System32\vmcompute.exe",
@"C:\Windows\System32\vmms.exe",
@"C:\Windows\System32\vmwp.exe",
@"C:\Windows\System32\VSD3DWARPDebug.dll",
@"C:\Windows\System32\VsGraphicsCapture.dll",
@"C:\Windows\System32\VsGraphicsDesktopEngine.exe",
@"C:\Windows\System32\VsGraphicsExperiment.dll",
@"C:\Windows\System32\VsGraphicsProxyStub.dll",
@"C:\Windows\System32\VsGraphicsRemoteEngine.exe",
@"C:\Windows\System32\wavemsp.dll",
@"C:\Windows\System32\wdigest.dll",
@"C:\Windows\System32\Websocket.dll",
@"C:\Windows\System32\wevtutil.exe",
@"C:\Windows\System32\wfdprov.dll",
@"C:\Windows\System32\WiFiConfigSP.dll",
@"C:\Windows\System32\wincorlib.dll",
@"C:\Windows\System32\Windows.Graphics.Printing.Workflow.dll",
@"C:\Windows\System32\Windows.Graphics.Printing.Workflow.Native.dll",
@"C:\Windows\System32\windows.storage.dll",
@"C:\Windows\System32\Windows.Storage.Search.dll",
@"C:\Windows\System32\Windows.UI.Xaml.dll",
@"C:\Windows\System32\windowsudk.shellcommon.dll",
@"C:\Windows\System32\wininet.dll",
@"C:\Windows\System32\winload.efi",
@"C:\Windows\System32\winload.exe",
@"C:\Windows\System32\winresume.efi",
@"C:\Windows\System32\winresume.exe",
@"C:\Windows\System32\wintrust.dll",
@"C:\Windows\System32\WinTypes.dll",
@"C:\Windows\System32\wlanapi.dll",
@"C:\Windows\System32\wlanhlp.dll",
@"C:\Windows\System32\wlanmsm.dll",
@"C:\Windows\System32\wlansec.dll",
@"C:\Windows\System32\wlansvc.dll",
@"C:\Windows\System32\wlansvcpal.dll",
@"C:\Windows\System32\wldp.dll",
@"C:\Windows\System32\wscript.exe",
@"C:\Windows\System32\wshrm.dll",
@"C:\Windows\System32\x3daudio1_0.dll",
@"C:\Windows\System32\x3daudio1_1.dll",
@"C:\Windows\System32\X3DAudio1_2.dll",
@"C:\Windows\System32\X3DAudio1_3.dll",
@"C:\Windows\System32\X3DAudio1_4.dll",
@"C:\Windows\System32\X3DAudio1_5.dll",
@"C:\Windows\System32\X3DAudio1_6.dll",
@"C:\Windows\System32\X3DAudio1_7.dll",
@"C:\Windows\System32\xactengine2_0.dll",
@"C:\Windows\System32\xactengine2_1.dll",
@"C:\Windows\System32\xactengine2_10.dll",
@"C:\Windows\System32\xactengine2_2.dll",
@"C:\Windows\System32\xactengine2_3.dll",
@"C:\Windows\System32\xactengine2_4.dll",
@"C:\Windows\System32\xactengine2_5.dll",
@"C:\Windows\System32\xactengine2_6.dll",
@"C:\Windows\System32\xactengine2_7.dll",
@"C:\Windows\System32\xactengine2_8.dll",
@"C:\Windows\System32\xactengine2_9.dll",
@"C:\Windows\System32\xactengine3_0.dll",
@"C:\Windows\System32\xactengine3_1.dll",
@"C:\Windows\System32\xactengine3_2.dll",
@"C:\Windows\System32\xactengine3_3.dll",
@"C:\Windows\System32\xactengine3_4.dll",
@"C:\Windows\System32\xactengine3_5.dll",
@"C:\Windows\System32\xactengine3_6.dll",
@"C:\Windows\System32\XAPOFX1_0.dll",
@"C:\Windows\System32\XAPOFX1_1.dll",
@"C:\Windows\System32\XAPOFX1_2.dll",
@"C:\Windows\System32\XAPOFX1_3.dll",
@"C:\Windows\System32\XAPOFX1_4.dll",
@"C:\Windows\System32\XAudio2_0.dll",
@"C:\Windows\System32\XAudio2_1.dll",
@"C:\Windows\System32\XAudio2_2.dll",
@"C:\Windows\System32\XAudio2_3.dll",
@"C:\Windows\System32\XAudio2_4.dll",
@"C:\Windows\System32\XAudio2_5.dll",
@"C:\Windows\System32\XAudio2_6.dll",
@"C:\Windows\System32\xinput1_1.dll",
@"C:\Windows\System32\xinput1_2.dll",
@"C:\Windows\System32\xinput1_3.dll",
@"C:\Windows\System32\appraiser\appraiser.sdb",
@"C:\Windows\System32\Boot\winload.efi",
@"C:\Windows\System32\Boot\winload.exe",
@"C:\Windows\System32\Boot\winresume.efi",
@"C:\Windows\System32\Boot\winresume.exe",
@"C:\Windows\System32\CodeIntegrity\driver.stl",
@"C:\Windows\System32\vmhgfs.dll",
@"C:\Windows\System32\VMWSU.DLL",
@"C:\Windows\System32\vsocklib.dll",
@"C:\Windows\System32\wuaueng2.dll",
@"C:\Windows\System32\CodeIntegrity\bootcat.cache",
@"C:\Windows\System32\restore\MachineGuid.txt",

        };

    }
}

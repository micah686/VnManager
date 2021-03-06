; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
#define MyAppName "VnManager"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "Micah686"
#define MyAppURL "https://micah686.github.io/VnManager/"
#define MyAppExeName "VnManager.exe"



[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{86768EB6-DA96-4D6F-B1FC-E7703F5A121F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
LicenseFile=MIT-License.txt
InfoBeforeFile=Metrics-Warning.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputBaseFilename=VnManagerSetup
Compression=lzma
SolidCompression=yes
WizardStyle=classic

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\build\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\build\runtimes\*"; DestDir: "{app}\runtimes\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\build\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\AdonisUI.ClassicTheme.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\AdonisUI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\AdysTech.CredentialManager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\FluentValidation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\LiteDB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\MahApps.Metro.IconPacks.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\MahApps.Metro.IconPacks.Material.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Mayerch1.GithubUpdateCheck.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\MvvmDialogs.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Sentry.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\Stylet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.CodeDom.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.IO.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.IO.Abstractions.TestingHelpers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\VndbSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\VnManager.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\PropertyChanged.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\VnManager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\VnManager.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\VnManager.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "netcorecheck_x64.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall
Source: "netcorecheck.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall; AfterInstall : DownloadInstallNetCore;

[Dirs]
Name: "{localappdata}\VnManager"
Name: "{userappdata}\VnManager"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\VnManager"
Type: filesandordirs; Name: "{userappdata}\VnManager"





//Code functions below
[Code]
var
  ProgressPage: TOutputProgressWizardPage;
  DownloadPage: TDownloadWizardPage;
function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if Progress = ProgressMax then
    Log(Format('Successfully downloaded file to {tmp}: %s', [FileName]));
  Result := True;
end;
procedure InitializeWizard;
begin
  ProgressPage := CreateOutputProgressPage('Finalization of installation','');
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;
function GetArchitectureSuffix: String;
begin
  if IsWin64 then
    Result:= '_x64'
  else
    Result:= ''
end;
// source code: https://github.com/dotnet/deployment-tools/tree/master/src/clickonce/native/projects/NetCoreCheck
function IsNetCoreInstalled(const Version: String): Boolean;
var
  ResultCode: Integer;
begin
  if not FileExists(ExpandConstant('{tmp}{\}') + 'netcorecheck' + GetArchitectureSuffix + '.exe') then begin
    ExtractTemporaryFile('netcorecheck' + GetArchitectureSuffix + '.exe');
  end;
  Result := ShellExec('', ExpandConstant('{tmp}{\}') + 'netcorecheck' + GetArchitectureSuffix + '.exe', Version, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;
procedure DownloadNetCore;
begin
    if GetArchitectureSuffix = '_x64' then begin
          //x64
          DownloadPage.Clear;
          DownloadPage.Add('https://go.microsoft.com/fwlink/?linkid=2153460', 'netcore31.exe', '');
          DownloadPage.Add('https://go.microsoft.com/fwlink/?linkid=2153459', 'netcore31desktop.exe', '');
          DownloadPage.Show;
            try
              DownloadPage.Download;
              DownloadPage.Hide;
              Exit;
            except
              SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
              DownloadPage.Hide;
              Exit;
            end;
         //End X64 download
          end else begin  //If arch is x86
          //DoADifferentThing;
          DownloadPage.Clear;
          DownloadPage.Add('https://go.microsoft.com/fwlink/?linkid=2153351', 'netcore31.exe', '');
          DownloadPage.Add('https://go.microsoft.com/fwlink/?linkid=2153350', 'netcore31desktop.exe', '');
          DownloadPage.Show;
            try
              DownloadPage.Download;
              DownloadPage.Hide;
              Exit;
            except
              SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
              DownloadPage.Hide;
              Exit;
            end;
         //End x86 download        
    end; //end arch check
end;
procedure InstallNetCore;
var
  StatusText: string;
  ResultCode: Integer;
begin
  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := 'Installing .NET COre 3.1';
  WizardForm.ProgressGauge.Style := npbstMarquee;
  try
    { here put the .NET setup execution code }
    Exec(ExpandConstant('{tmp}\netcore31.exe'), '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
    Exec(ExpandConstant('{tmp}\netcore31desktop.exe'), '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;
  end;
end;
procedure DownloadInstallNetCore;
begin
   if (not IsNetCoreInstalled('Microsoft.NETCore.App 3.1.11')) or (not IsNetCoreInstalled('Microsoft.WindowsDesktop.App 3.1.11')) then begin
      DownloadNetCore
      InstallNetCore
   end;
   
   
end;

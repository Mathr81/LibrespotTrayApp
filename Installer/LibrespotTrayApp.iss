; Inno Setup 6 script. Built by the CI workflow:
;   ISCC.exe /DPublishDir=<publish dir> /DAppVersion=2.0.0 /DAppNumericVersion=2.0.0 LibrespotTrayApp.iss

#ifndef PublishDir
  #define PublishDir "..\publish_output"
#endif
#ifndef AppVersion
  #define AppVersion "dev"
#endif
#ifndef AppNumericVersion
  #define AppNumericVersion "0.0.0"
#endif

#define AppName "LibrespotTrayApp"
#define AppExe "LibrespotTrayApp.exe"
; Must match AppPaths.AppUserModelId: Windows shows the shortcut name and icon for the app.
#define AppUserModelID "Mathr81.LibrespotTrayApp"

[Setup]
; Same AppId as the 1.x installer (which used the AppName), so that it is upgraded in place.
AppId={#AppName}
AppName=Librespot Tray App
AppVersion={#AppVersion}
AppVerName=Librespot Tray App {#AppVersion}
AppPublisher=Mathr81
AppPublisherURL=https://github.com/Mathr81/LibrespotTrayApp
AppSupportURL=https://github.com/Mathr81/LibrespotTrayApp/issues
VersionInfoVersion={#AppNumericVersion}
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=yes
DisableDirPage=auto
; Per-machine install by default; the user can choose a per-user install (no admin rights).
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; The startup entry and the cache are per-user on purpose: they belong to the user running setup.
UsedUserAreasWarning=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputBaseFilename=LibrespotTrayApp-Installer-v{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\LibrespotTrayApp\Resources\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName=Librespot Tray App
CloseApplications=no

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Lancer au démarrage de Windows"; GroupDescription: "Options :"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LibrespotTrayApp\Resources\app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Librespot Tray App"; Filename: "{app}\{#AppExe}"; AppUserModelID: "{#AppUserModelID}"
Name: "{autodesktop}\Librespot Tray App"; Filename: "{app}\{#AppExe}"; AppUserModelID: "{#AppUserModelID}"; Tasks: desktopicon

[Registry]
; Name and icon for the AppUserModelID (media controls, notifications), even without the shortcut.
Root: HKA; Subkey: "Software\Classes\AppUserModelId\{#AppUserModelID}"; ValueType: string; ValueName: "DisplayName"; ValueData: "Librespot"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\AppUserModelId\{#AppUserModelID}"; ValueType: string; ValueName: "IconUri"; ValueData: "{app}\app.ico"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#AppName}"; ValueData: """{app}\{#AppExe}"""; Tasks: startup
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#AppName}"; Flags: deletevalue uninsdeletevalue; Check: not WizardIsTaskSelected('startup')
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#AppName}"; Flags: dontcreatekey uninsdeletevalue

[UninstallDelete]
; The audio cache can be large; settings, logs and the Spotify login are kept.
Type: filesandordirs; Name: "{localappdata}\{#AppName}\cache"

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,Librespot Tray App}"; Flags: nowait postinstall skipifsilent

[Code]
procedure StopRunningApp();
var
  ResultCode: Integer;
begin
  // librespot.exe is in a kill-on-close job of the tray app: it stops with it.
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#AppExe}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(500);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  StopRunningApp();
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  StopRunningApp();
  Result := True;
end;

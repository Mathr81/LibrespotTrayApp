[Setup]
AppName=LibrespotTrayApp
AppVersion={#emit AppVersion}
AppPublisher=Mathr81
DefaultDirName={autopf}\LibrespotTrayApp
DisableProgramGroupPage=yes
DisableWelcomePage=yes
OutputBaseFilename=LibrespotTrayApp-Installer-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\app.ico
UninstallDisplayIcon={app}\LibrespotTrayApp.exe

[Files]
Source: "{#MyAppPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\LibrespotTrayApp"; Filename: "{app}\LibrespotTrayApp.exe"
Name: "{autodesktop}\LibrespotTrayApp"; Filename: "{app}\LibrespotTrayApp.exe"

[Run]
Filename: "{app}\LibrespotTrayApp.exe"; Description: "{cm:LaunchProgram,LibrespotTrayApp}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\unins000.exe"; Parameters: "/SILENT"
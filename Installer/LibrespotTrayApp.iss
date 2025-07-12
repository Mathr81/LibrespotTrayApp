[Setup]
AppName=LibrespotTrayApp
AppVersion=1.0.0 ; Placeholder, will be replaced by GitHub Actions
AppPublisher=Your Name/Company
DefaultDirName={autopf}\LibrespotTrayApp
DisableProgramGroupPage=yes
DisableWelcomePage=yes
OutputBaseFilename=LibrespotTrayApp-Installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\app.ico
UninstallDisplayIcon={app}\LibrespotTrayApp.exe

[Files]
Source: "..\LibrespotTrayApp\bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\LibrespotTrayApp"; Filename: "{app}\LibrespotTrayApp.exe"
Name: "{autodesktop}\LibrespotTrayApp"; Filename: "{app}\LibrespotTrayApp.exe"

[Run]
Filename: "{app}\LibrespotTrayApp.exe"; Description: "{cm:LaunchProgram,LibrespotTrayApp}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\unins000.exe"; Parameters: "/SILENT"
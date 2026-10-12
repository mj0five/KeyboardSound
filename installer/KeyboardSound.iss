#ifndef PublishDir
  #define PublishDir "..\publish"
#endif
[Setup]
AppId={{70FCBC54-63E0-43FD-8B66-E4679C68453A}
AppName=Keyboard Sound
AppVersion=1.0.0
DefaultDirName={localappdata}\Programs\KeyboardSound
DefaultGroupName=Keyboard Sound
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir=..\..
OutputBaseFilename=KeyboardSound-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\KeyboardSound.exe
SetupIconFile=..\src\KeyboardSound\Assets\KeyboardSound.ico
CloseApplications=yes
AppMutex=Local\KeyboardSound.V1
[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\Keyboard Sound"; Filename: "{app}\KeyboardSound.exe"
Name: "{userdesktop}\Keyboard Sound"; Filename: "{app}\KeyboardSound.exe"; Tasks: desktopicon
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "KeyboardSound"; Flags: uninsdeletevalue
[Run]
Filename: "{app}\KeyboardSound.exe"; Description: "Launch Keyboard Sound"; Flags: nowait postinstall skipifsilent

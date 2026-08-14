#define AppName "Window Lock"
#define AppVersion "0.2.0"
#define AppPublisher "flyingbrick88"
#define RepoUrl "https://github.com/flyingbrick88/window-lock-privacy"

[Setup]
AppId={{B4341ED4-B928-4B08-89B0-61527024CD36}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#RepoUrl}
AppSupportURL={#RepoUrl}/issues/new/choose
AppUpdatesURL={#RepoUrl}/releases
DefaultDirName={localappdata}\Programs\Window Lock
DefaultGroupName=Window Lock
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist\installer
OutputBaseFilename=WindowLock-Setup-{#AppVersion}
SetupIconFile=..\src\WindowLock\Assets\window-lock.ico
UninstallDisplayIcon={app}\WindowLock.exe
LicenseFile=..\LICENSE
InfoBeforeFile=privacy-first.txt
InfoAfterFile=post-install.txt
WizardStyle=modern
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
MinVersion=10.0.17763
CloseApplications=force
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Start Window Lock when I sign in"; GroupDescription: "Startup:"; Flags: checkedonce
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "..\dist\win-x64\WindowLock.exe"; DestDir: "{app}"; DestName: "WindowLock.exe"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[InstallDelete]
Type: files; Name: "{app}\WindowLock-0.1.0.exe"

[Icons]
Name: "{group}\Window Lock"; Filename: "{app}\WindowLock.exe"; WorkingDir: "{app}"
Name: "{group}\Privacy information"; Filename: "{app}\PRIVACY.md"
Name: "{group}\Uninstall Window Lock"; Filename: "{uninstallexe}"
Name: "{autostartup}\Window Lock"; Filename: "{app}\WindowLock.exe"; WorkingDir: "{app}"; Tasks: startup
Name: "{autodesktop}\Window Lock"; Filename: "{app}\WindowLock.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\WindowLock.exe"; Description: "Launch Window Lock"; Flags: nowait postinstall skipifsilent

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM WindowLock.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM WindowLock-0.1.0.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

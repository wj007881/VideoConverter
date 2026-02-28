; Inno Setup Script for Bailian Video Converter
; Requires Inno Setup 6.x or later

#define MyAppName "Bailian Video Converter"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Bailian Coding"
#define MyAppURL "https://github.com/bailiancoding"
#define MyAppExeName "BailianCoding.exe"
#define MyAppCopyright "Copyright (C) 2025"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright={#MyAppCopyright}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=installer
OutputBaseFilename=BailianVideoConverter_Setup_{#MyAppVersion}
SetupIconFile=Assets\AppIcon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Application executable
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; FFmpeg binaries
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\ffmpeg.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\ffprobe.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\ffplay.exe"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; Runtime files
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\*.json"; DestDir: "{app}"; Flags: ignoreversion

; Assets
Source: "bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
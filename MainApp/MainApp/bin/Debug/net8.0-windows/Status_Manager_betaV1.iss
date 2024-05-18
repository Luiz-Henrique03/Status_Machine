; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Status_Manager_Beta"
#define MyAppVersion "1.0"
#define MyAppPublisher "My Company, Inc."
#define MyAppURL "https://www.example.com/"
#define MyAppExeName "MainApp.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".exe"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{E4F42508-94CB-4172-B302-547F19479D0A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Status_Manager_Betav1
ChangesAssociations=yes
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=C:\Users\User\Downloads
OutputBaseFilename=Status_Manager_Betav1
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\logs.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\MainApp.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\MainApp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\MainApp.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\MainApp.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\MainApp.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\User\source\repos\MainApp\MainApp\bin\Debug\net8.0-windows\UnitTests\*"; DestDir: "{app}\UnitTests"; Flags: ignoreversion recursesubdirs createallsubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


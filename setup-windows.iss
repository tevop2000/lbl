; AgentManagement.Avalonia Windows 安装脚本
; 使用 Inno Setup 编译器
; 下载地址: https://jrsoftware.org/isdl.php

[Setup]
AppName=数智营
AppVersion=1.0.0
AppPublisher=Your Company
AppPublisherURL=https://yourcompany.com
AppSupportURL=https://yourcompany.com/support
AppUpdatesURL=https://yourcompany.com/updates
DefaultDirName={pf}\数智营
DefaultGroupName=数智营
AllowNoIcons=yes
OutputDir=.
OutputBaseFilename=数智营-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
SetupIconFile=Assets\avalonia-logo.ico
UninstallDisplayIcon={app}\AgentManagement.Avalonia.exe

[Files]
Source: "publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Assets\avalonia-logo.ico"; DestDir: "{app}"

[Icons]
Name: "{group}\数智营"; Filename: "{app}\AgentManagement.Avalonia.exe"; IconFilename: "{app}\avalonia-logo.ico"
Name: "{commondesktop}\数智营"; Filename: "{app}\AgentManagement.Avalonia.exe"; IconFilename: "{app}\avalonia-logo.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "其他选项"

[Run]
Filename: "{app}\AgentManagement.Avalonia.exe"; Description: "运行数智营"; Flags: nowait postinstall skipifsilent

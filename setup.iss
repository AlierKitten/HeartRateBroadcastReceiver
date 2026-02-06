#ifndef MyAppVersion
  #error "MyAppVersion must be defined via command line parameter /DMyAppVersion=x.x.x.x"
#endif
#pragma message "MyAppVersion: {#MyAppVersion}"
#define MyAppName "HeartRateBroadcastReceiver"
#define MyAppPublisher "Alierkitten"
#define MyAppCopyright "Copyright © 2025 Alierkitten"

[Setup]
AppId={{1E5C1300-213D-417C-84AA-{#MyAppName}}
AppName=心率广播接收器
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
VersionInfoCopyright={#MyAppCopyright}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppName}_setup_{#MyAppVersion}
OutputDir={#SourcePath}\installer
ArchitecturesInstallIn64BitMode=x64
Compression=lzma
SolidCompression=yes
MinVersion=10.0.17763

DisableDirPage=no
ShowComponentSizes=yes
WizardStyle=modern

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourcePath}\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式(&D)"; GroupDescription: "附加任务"
Name: "autorun"; Description: "开机时自动启动(&A)"; GroupDescription: "附加任务"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppName}.exe"""; \
    Flags: uninsdeletevalue; Tasks: autorun

[Run]
Filename: "{app}\{#MyAppName}.exe"; Description: "启动 心率广播接收器(&S)"; Flags: nowait postinstall unchecked

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  UninstallResult: Integer;
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      if MsgBox('检测到已安装旧版本的心率广播接收器。' + #13#10 +
                '必须先卸载旧版本才能继续安装新版本。' + #13#10#13#10 +
                '是否要卸载旧版本并继续安装？',
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
      begin
        UninstallResult := UnInstallOldVersion();
        if UninstallResult <> 3 then
        begin
          MsgBox('卸载旧版本失败！安装将被取消。', mbError, MB_OK);
          Abort();
        end;
      end
      else
      begin
        MsgBox('安装已取消。必须先卸载旧版本才能安装新版本。', mbInformation, MB_OK);
        Abort();
      end;
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', '{#MyAppName}');
  end;

  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('是否要删除配置文件？' + #13#10 +
              '这将删除所有保存的配置信息。',
              mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
    begin
      DelTree(ExpandConstant('{app}\*.ini'), False, True, True);
    end;
    RemoveDir(ExpandConstant('{app}'));
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpInstalling then
  begin
    WizardForm.FilenameLabel.Visible := False;
    if WizardForm.ProgressGauge.Top < WizardForm.StatusLabel.Top + 40 then
      WizardForm.ProgressGauge.Top := WizardForm.StatusLabel.Top + WizardForm.StatusLabel.Height + 8;
  end;
end;

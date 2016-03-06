; XENOCIDE RELATED DIRECTIVES
#define MyAppName "Xenocide"
#define MyAppVerName "Xenocide v0.4"
#define MyAppPublisher "Project Xenocide"
#define MyAppURL "http://www.projectxenocide.com/"
#define MyAppExeName "Xenocide.exe"
#define PrequisiteCheck "dxxna.exe"

[Setup]
AppId={{93537871-8E72-4214-A8EA-D8EF8314C2F9}
AppName={#MyAppName}
AppVerName={#MyAppVerName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=xenocide_04
Compression=lzma/ultra
SolidCompression=false
InternalCompressLevel=ultra
PrivilegesRequired=admin
EnableDirDoesntExistWarning=true
UpdateUninstallLogAppName=false
ShowLanguageDialog=auto
VersionInfoVersion=0.4
VersionInfoCompany=Project Xenocide
WindowVisible=true
AlwaysShowDirOnReadyPage=true
AlwaysShowGroupOnReadyPage=true
WizardImageFile=D:\Programs\Inno Setup 5\WizModernImage-IS.bmp
WizardSmallImageFile=D:\Programs\Inno Setup 5\WizModernSmallImage-IS.bmp
InfoBeforeFile=D:\Xenocide Repository\pre-install-info.txt
LicenseFile=D:\Xenocide Repository\license.txt
AllowRootDirectory=false
AllowUNCPath=false
SetupLogging=true
VersionInfoCopyright=2008
CompressionThreads=auto
ExtraDiskSpaceRequired=25165824
DirExistsWarning=no
BackColor=clGreen
WizardImageBackColor=clGreen
SignedUninstaller=false

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
Source: D:\Xenocide Repository\dxxna.exe; DestDir: {app}\xnari\; Flags: ignoreversion
Source: D:\Xenocide Repository\XNA\trunk\Xenocide\bin\x86\Release\*; DestDir: {app}; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: {commondesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}; Tasks: desktopicon
Name: {group}\{cm:UninstallProgram, {#MyAppName}}; Filename: {uninstallexe}; WorkingDir: {app}
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {group}\Xenocide homepage; Filename: http://www.projectxenocide.com/

[Run]
Filename: {app}\xnari\{#PrequisiteCheck}; Description: Run the automatic XNA Requirements Checker/Installer (xnari 0.5); Flags: postinstall unchecked
Filename: {app}\{#MyAppExeName}; Description: Launch the game; Flags: nowait postinstall skipifsilent
Filename: {app}\Xenocide_Audio_Pack.exe; Description: Extracts the audio self-extracting zip; Flags: skipifsilent nowait skipifdoesntexist
Filename: {app}\Xenocide_HighDef_Globe.exe; Description: Extracts the High resolution globe textures zip; Flags: skipifsilent nowait skipifdoesntexist

[Components]
Name: directx; Description: Download the latest DirectX 9.0c (December 2006 or newer needed!); Flags: disablenouninstallwarning; Types: full prereq; ExtraDiskSpaceRequired: 307200
Name: dotnet2sp1; Description: Download .NET Framework Version 2.0 SP1; Flags: disablenouninstallwarning; Types: full prereq; ExtraDiskSpaceRequired: 23511040
Name: xna; Description: XNA Refresh 1.0 Runtime Distribution; Flags: disablenouninstallwarning; Types: full prereq; ExtraDiskSpaceRequired: 2038784
Name: audio_pack; Description: Music & SFX (This will download additional an 16.5 MB from the Project Xenocide Server!); Flags: disablenouninstallwarning; Types: full addons; ExtraDiskSpaceRequired: 17252352
Name: texture_pack; Description: High resolution textures for the globe (This will download an additional 2.1 MB! - Please note that this might have a grave impact on performance on lower-end graphics cards); Flags: disablenouninstallwarning; Types: full addons; ExtraDiskSpaceRequired: 2197504

[Types]
Name: addons; Description: Game-packs installation (Game addon-components selected)
Name: full; Description: Full installation (All components selected)
Name: prereq; Description: Full prerequisite installation (XNA Prerequisites selected)
Name: minimal; Description: Minimal installation (No components selected)
Name: custom; Description: Custom installation; Flags: iscustom

[_ISToolDownload]
Source: http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=0856eacb-4362-4b0d-8edd-aab15c5e04f5&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f5%2f6%2f7%2f567758a3-759e-473e-bf8f-52154438565a%2fdotnetfx.exe; DestDir: {commondesktop}; DestName: dotnetfx.exe; Components: dotnet2sp1
Source: http://download.microsoft.com/download/1/7/1/1718ccc4-6315-4d8e-9543-8e28a4e18c4c/dxwebsetup.exe; DestDir: {commondesktop}; DestName: dxwebsetup.exe; Components: directx
Source: http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=a7da4763-6807-4bd5-8d18-18c60c437f93&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f1%2f7%2fd%2f17d2b68e-3ba4-4ec3-b225-cbd3d2e510a2%2fxnafx_redist.msi; DestDir: {commondesktop}; DestName: xnafx_redist.msi; Components: xna
Source: http://www.projectxenocide.com/download/Xenocide_Audio_Pack.exe; DestDir: {app}; DestName: Xenocide_Audio_Pack.exe; Components: audio_pack
Source: http://projectxenocide.com/download/Xenocide_HighDef_Globe.exe; DestDir: {app}; DestName: Xenocide_HighDef_Globe.exe; Components: texture_pack

[Code]
function NextButtonClick(CurPage: Integer): Boolean;
begin
	if (CurPage = wpReady) then
	begin
		ForceDirectories(ExpandConstant('{app}'));
		Result := ISTool_Download(CurPage)
    end
	else
		Result := True;
end;

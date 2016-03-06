#############################
####  TABLE OF CONTENTS  ####
#############################

; (1) Installer Preparation
;    (1.1) Compression settings
;    (1.2) Include macros
;    (1.3) General define macros
;    (1.4) MUI Settings
; (2) Installer options
;    (2.1) XenoInstaller-specific settings
;    (2.2) Shortcut names
;    (2.3) Version/Exe information
; (3) Installer Sections
;    (3.1) .onInit
;    (3.2) OS/Hardware Check
;    (3.3) MSI Check
;    (3.4) .Net2/XNA/DirectX Check
;    (3.5) Main program files
;    (3.6) Custom installation shortcuts/keys
;    (3.7) Bonus Section
; (4) Uninstaller Sections
;    (4.1) un.onInit
;    (4.2) Main uninstall section
;    (4.3) Custom file/key uninstallation
; (5) Miscellaneous
;    (5.1) Section Descriptions
;    (5.2) DumpLog function


# (1.1) Compression settings
SetCompressor /FINAL /SOLID lzma

# (1.2) Include macros
!include "MUI2.nsh"                             ; Modern UI 2
!include "LogicLib.nsh"                         ; Logical operation macros
!include ".\Includes\ZipDLL.nsh"                ; Zip functions
#!include ".\Includes\strtok.nsh"               ; strtok function
!include ".\Includes\DotNET check.nsh"          ; dotNet 2 SP1 check/installer
!include ".\Includes\XNA check.nsh"             ; XNA Framework 1.0 Refresh check/installer
!include ".\Includes\DirectX check.nsh"         ; DirectX 9.0c check/installer
!include ".\Includes\bonus.nsh"                 ; Bonus material download
!include ".\Includes\random_download.nsh"       ; Download functions with mirror support :D

# (1.3) General define macros
# For setup log dumping function
!define LVM_GETITEMCOUNT 0x1004
!define LVM_GETITEMTEXT 0x102D

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "Xenocide"
!define PRODUCT_VERSION "0.4"
!define PRODUCT_BUILD "1838"
!define PRODUCT_PUBLISHER "Project Xenocide"
!define PRODUCT_WEB_SITE "http://www.projectxenocide.com"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Xenocide.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"


#(1.4) MUI Settings
!define MUI_ABORTWARNING

; MUI Settings / Icons
!define MUI_ICON ".\icons\GameInstall.ico"
!define MUI_UNICON ".\icons\GameUninstall.ico"

; MUI Settings / Header
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_RIGHT
!define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\orange-r-nsis.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "${NSISDIR}\Contrib\Graphics\Header\orange-uninstall-r-nsis.bmp"

; MUI Settings / Wizard
!define MUI_WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\orange-nsis.bmp"

; Language Selection Dialog Settings
!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_UNINST_ROOT_KEY}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"

; Welcome page
!insertmacro MUI_PAGE_WELCOME

; License page
!insertmacro MUI_PAGE_LICENSE "license.txt"

; Components page
!insertmacro MUI_PAGE_COMPONENTS

; Directory page
!insertmacro MUI_PAGE_DIRECTORY

; Instfiles page
!insertmacro MUI_PAGE_INSTFILES

; Finish page
#!define MUI_FINISHPAGE_RUN "$INSTDIR\Xenocide.exe"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!define MUI_LANGDLL_ALLLANGUAGES
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "Spanish"
!insertmacro MUI_LANGUAGE "Greek"


#######################################################################################
############################### Start of Main Installer ###############################
#######################################################################################

# (2.1) XenoInstaller-specific settings
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Xenocide installer.exe"
InstallDir "$PROGRAMFILES\Xenocide"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails hide
ShowUnInstDetails show

# (2.2) Shortcut names
!define DESKTOP_LINK_NAME "${PRODUCT_NAME} ${PRODUCT_VERSION}.lnk"
!define HOMEPAGE_URL_NAME "Xenocide Homepage.url"
!define SM_NAME "$SMPROGRAMS\Xenocide"
!define PLAY_LINK_NAME "Play Xenocide.lnk"
!define UNINSTALL_LINK_NAME "Uninstall Xenocide.lnk"
!define STARTSETTINGS_LINK_NAME "Xenocide Newgame Settings.lnk"


# (2.3) Version/Exe information
VIProductVersion "${PRODUCT_VERSION}.0.${PRODUCT_BUILD}"
VIAddVersionKey /LANG=1033 "ProductName" "Xenocide"
VIAddVersionKey /LANG=1033 "Comments" "Xenocide, an free and open-source game inspired by the X-Com universe"
VIAddVersionKey /LANG=1033 "CompanyName" "Project Xenocide"
VIAddVersionKey /LANG=1033 "LegalTrademarks" "Xenocide is released under CreativeCommons"
VIAddVersionKey /LANG=1033 "LegalCopyright" "CC Project Xenocide"
VIAddVersionKey /LANG=1033 "FileDescription" "Xenocide"
VIAddVersionKey /LANG=1033 "FileVersion" "${PRODUCT_VERSION}"


##################################################
###  Sections, Variables & Installer Functions ###
##################################################

var winVer
var winArch

# (3.1) .onInit
Function .onInit
   SetDetailsPrint both
   # Language selection dialog
   !insertmacro MUI_LANGDLL_DISPLAY

   # Check if the game is already installed and get the path, or set default one
   ReadRegStr $9 HKLM "Software\Project Xenocide" "LastInstallPath"

   ${If} $9 == ""
      strcpy $INSTDIR "$PROGRAMFILES\Xenocide\"        # Default Installation Path
   ${Else}
      strcpy $INSTDIR $9                               # Current Installation Path
   ${EndIf}

   # Check if already running
   # If so don't open another but bring to front
   BringToFront
   System::Call "kernel32::CreateMutexA(i 0, i 0, t '$(^Name)') i .r0 ?e"
   Pop $0
   StrCmp $0 0 launch
    StrLen $0 "$(^Name)"
    IntOp $0 $0 + 1

   loop:
     FindWindow $1 '#32770' '' 0 $1
     IntCmp $1 0 +4
     System::Call "user32::GetWindowText(i r1, t .r2, i r0) i."
     StrCmp $2 "$(^Name)" 0 loop
     System::Call "user32::ShowWindow(i r1,i 9) i."         ; If minimized then maximize
     System::Call "user32::SetForegroundWindow(i r1) i."    ; Bring to front
     Abort

   launch:
FunctionEnd


# (3.2) OS/Hardware Check
Section "-PC Check" SEC_PC

   DetailPrint "**********************************************************"
   DetailPrint "* Checking windows version/architecture,..."
   GetVersion::WindowsName
      Pop $R0
      DetailPrint '*  - Windows Version: "$R0"'
      StrCpy $WinVer $R0


   ${If} $R0 == '95 OSR2'
   ${OrIf} $R0 == '95'
   ${OrIf} $R0 == '98 SE'
   ${OrIf} $R0 == '98'
   ${OrIf} $R0 == 'ME'
   ${OrIf} $R0 == 'NT'
   ${OrIf} $R0 == 'CE'
      MessageBox MB_OK|MB_ICONSTOP "Project Xenocide only works under Windows 2000 or newer"
      abort "Wrong Windows version detected, aborting installation..."
   ${EndIf}


   GetVersion::WindowsPlatformArchitecture
      Pop $R0
      DetailPrint '*  - Platform Architecture: "$R0"'
      StrCpy $WinArch $R0


   HwInfo::GetCpuNameAndSpeed
      StrCpy $R0 $0


   HwInfo::GetSystemMemory
      StrCpy $R1 $0


   HwInfo::GetVideoCardName
      StrCpy $R2 $0


   HwInfo::GetVideoCardMemory
      StrCpy $R3 $0

   DetailPrint "*  - CPU: $R0$\r$\n*  - RAM: $R1 MB$\r$\n*  - Video Card: $R2$\r$\n*  - VRAM: $R3 MB."
   DetailPrint "* End of System Information Section"
   DetailPrint "**********************************************************$\r$\n"

SectionEnd


# (3.3) MSI Check
Section "-MSI 3.1" SEC_MSI

   GetDLLVersion "$SYSDIR\msi.dll" $R0 $R1
   IntOp $R2 $R0 >> 16
   IntOp $R2 $R2 & 0x0000FFFF ; $R2 now contains major version
   IntOp $R3 $R0 & 0x0000FFFF ; $R3 now contains minor version
   IntOp $R4 $R1 >> 16
   IntOp $R4 $R4 & 0x0000FFFF ; $R4 now contains release
   IntOp $R5 $R1 & 0x0000FFFF ; $R5 now contains build
   StrCpy $0 "$R2.$R3.$R4.$R5" ; $0 now contains string like "1.2.0.192"


   ${If} $winVer == 'XP'
   ${OrIf} $winVer == '2000'
         ${If} $R2 > 3
             Goto good
         ${ElseIf} $R2 == 3
             ${If} $R5 >= 1823
             Goto good
             ${Else}
                 DetailPrint '* MSI Installer version found: "$0", but is older than the minimal required version: "3.1.4000.2435"'
                 Goto installMSI
             ${EndIf}
         ${Else}
             DetailPrint '* MSI Installer version found: "$0", but is older than the minimal required version: "3.1.4000.2435"'
             Goto installMSI
         ${EndIf}

   ${Else}
   good:
      DetailPrint '* MSI Installer version is OK ($0)! Installation can continue.'
      Goto End

   ${EndIf}


   installMSI:
      MESSAGEBOX MB_OK "You need MSI 3.1 in order to install .Net Framework 2 and the XNA Runtimes. Press OK to open the download page.$\r$\nAfter installing MSI 3.1, restart the computer and then run the Xenocide Installer again." IDYES 0 IDNO pass

      ${If} winArch = '"32"'
         DetailPrint "Opening webpage for MSI 3.1 x32"
         MESSAGEBOX MB_OK '"Choose file "WindowsInstaller-KB893803-v2-x86.exe"'
         ExecShell open "http://www.microsoft.com/downloads/details.aspx?FamilyId=889482FC-5F56-4A38-B838-DE776FD4138C" "" SW_SHOWNORMAL
      ${ElseIf} winArch = '"64"'
         DetailPrint "Opening webpage for MSI 3.1 x64"
         MESSAGEBOX MB_OK '"Choose the file "WindowsXP-KB898715-x64-enu.exe"'
         ExecShell open "http://www.microsoft.com/downloads/details.aspx?FamilyId=8B4E6B93-1886-4D47-A18D-35581C42ECA0" "" SW_SHOWNORMAL
      ${EndIf}
         DetailPrint "Installation aborted. Please press cancel, install MSI and re-run the installer."
         Abort


   pass:
      DetailPrint "* User decided not to install MSI 3.1. Xenocide installation will continue"
      goto End

   End:

SectionEnd


# (3.4) .Net2/XNA/DirectX Check
# Checking for .Net Framework 2.0 SP1
Section "-.Net 2.0 Framework" SEC_DOTNET
   !insertmacro CheckDotNET "2.0"
SectionEnd

   
# Checking For XNA Framework
Section "-XNA Framework 1.0 Refresh" SEC_XNA
  Call XNA_Check
SectionEnd


# Checking for DirectX
Section "-DirectX 9.0c" SEC_DIRECTX
  Call DirectX_Install
SectionEnd


# (3.5) Main program files
Section "Core" SEC_CORE
  SetDetailsPrint none
  Detailprint "Installing main program files..."
  SetOutPath "$INSTDIR"
  SetOverwrite try
  File "Release\CeGui.dll"
  File "Release\CeGui.pdb"
  File "Release\CeGui.Renderers.Xna.dll"
  File "Release\CeGui.Renderers.Xna.pdb"
  File "Release\CeGui.WidgetSets.Taharez.dll"
  File "Release\CeGui.WidgetSets.Taharez.pdb"
  File "Release\CeGui.WidgetSets.Taharez.xml"
  File "Release\CeGui.xml"
  SetOutPath "$INSTDIR\Content\Audio\Sounds\Menu"
  File "Release\Content\Audio\Sounds\Menu\buttonclick1_ok.ogg"
  File "Release\Content\Audio\Sounds\Menu\buttonclick2_changesetting.ogg"
  File "Release\Content\Audio\Sounds\Menu\buttonover.ogg"
  File "Release\Content\Audio\Sounds\Menu\exitgame.ogg"
  SetOutPath "$INSTDIR\Content\Audio\Sounds\PlanetView"
  File "Release\Content\Audio\Sounds\PlanetView\clickobjectonplanet.ogg"
  File "Release\Content\Audio\Sounds\PlanetView\speedfast.ogg"
  File "Release\Content\Audio\Sounds\PlanetView\speedslow.ogg"
  File "Release\Content\Audio\Sounds\PlanetView\speedveryfast.ogg"
  File "Release\Content\Audio\Sounds\PlanetView\zoomin.ogg"
  File "Release\Content\Audio\Sounds\PlanetView\zoomout.ogg"
  SetOutPath "$INSTDIR\Content\DataFiles"
  File "Release\Content\DataFiles\credits.txt"
  SetOutPath "$INSTDIR\Content\DataFiles\Geoscape"
  File "Release\Content\DataFiles\Geoscape\countries.png"
  File "Release\Content\DataFiles\Geoscape\regions.png"
  File "Release\Content\DataFiles\Geoscape\terrain.png"
  SetOutPath "$INSTDIR\Content\FBX\BE4D1BAE\Viper.fbm"
  File "Release\Content\FBX\BE4D1BAE\Viper.fbm\snake1_0.xnb"
  SetOutPath "$INSTDIR\Content\FBX\BFB81BBC\Barracks.fbm"
  File "Release\Content\FBX\BFB81BBC\Barracks.fbm\BARRACKS_0.xnb"
  File "Release\Content\FBX\BFB81BBC\Barracks.fbm\TABLE_0.xnb"
  SetOutPath "$INSTDIR\Content\FBX\CDD01F7B\alien_containment.fbm"
  File "Release\Content\FBX\CDD01F7B\alien_containment.fbm\alien1_0.xnb"
  File "Release\Content\FBX\CDD01F7B\alien_containment.fbm\post-36-1077887638[1]_0.xnb"
  SetOutPath "$INSTDIR\Content\FBX\CDD51F74\Research_Facility.fbm"
  File "Release\Content\FBX\CDD51F74\Research_Facility.fbm\Lab(Diffuse)_0.xnb"
  File "Release\Content\FBX\CDD51F74\Research_Facility.fbm\Microscope(Diffuse)_0.xnb"
  File "Release\Content\FBX\CDD51F74\Research_Facility.fbm\Server(Diffuse)_0.xnb"
  File "Release\Content\FBX\CDD51F74\Research_Facility.fbm\Tube(Diffuse)_0.xnb"
  File "Release\Content\FBX\CDD51F74\Research_Facility.fbm\TV(Diffuse)_0.xnb"
  SetOutPath "$INSTDIR\Content\FBX\34DC1D44\Laser Rifle.fbm"
  File "Release\Content\FBX\34DC1D44\Laser Rifle.fbm\h_l_rifle(diffuse)_0.xnb"
  SetOutPath "$INSTDIR\Content\FBX\6FD41E3F\FemaleShirt.fbm"
  File "Release\Content\FBX\6FD41E3F\FemaleShirt.fbm\FShirt(Diffuse)_0.xnb"
  SetOutPath "$INSTDIR\Content\Layouts"
  File "Release\Content\Layouts\AircraftOrdersDialog.layout"
  File "Release\Content\Layouts\BuildFacilityDialog.layout"
  File "Release\Content\Layouts\LaunchInterceptDialog.layout"
  File "Release\Content\Layouts\NameNewBaseDialog.layout"
  File "Release\Content\Layouts\OptionsDialog.layout"
  File "Release\Content\Layouts\PickActionDialog.layout"
  File "Release\Content\Layouts\SoundOptionsDialog.layout"
  File "Release\Content\Layouts\StartBattlescapeDialog.layout"
  File "Release\Content\Layouts\TrackingLostDialog.layout"
  File "Release\Content\Layouts\UfoInfoDialog.layout"
  SetOutPath "$INSTDIR\Content\Models\Characters\Alien"
  File "Release\Content\Models\Characters\Alien\alien-morlock-01_0.xnb"
  File "Release\Content\Models\Characters\Alien\f_silab_0.xnb"
  File "Release\Content\Models\Characters\Alien\Morlock.xnb"
  File "Release\Content\Models\Characters\Alien\Silibrate.xnb"
  File "Release\Content\Models\Characters\Alien\Viper.xnb"
  SetOutPath "$INSTDIR\Content\Models\Characters\XCorp"
  File "Release\Content\Models\Characters\XCorp\FemaleShirt.xnb"
  SetOutPath "$INSTDIR\Content\Models\Craft\Weapons"
  File "Release\Content\Models\Craft\Weapons\Avalanche.xnb"
  File "Release\Content\Models\Craft\Weapons\Avalanche_0.xnb"
  File "Release\Content\Models\Craft\Weapons\Stingray.xnb"
  File "Release\Content\Models\Craft\Weapons\STINGRAY_0.xnb"
  SetOutPath "$INSTDIR\Content\Models\Craft\Xcorps"
  File "Release\Content\Models\Craft\Xcorps\AvengExterior_0.xnb"
  File "Release\Content\Models\Craft\Xcorps\AvengInterior_0.xnb"
  File "Release\Content\Models\Craft\Xcorps\XC1-hull-col_0.xnb"
  File "Release\Content\Models\Craft\Xcorps\xc1.xnb"
  File "Release\Content\Models\Craft\Xcorps\xc33.xnb"
  SetOutPath "$INSTDIR\Content\Models\Facility"
  File "Release\Content\Models\Facility\alien_containment.xnb"
  File "Release\Content\Models\Facility\barracks.xnb"
  File "Release\Content\Models\Facility\base_access.xnb"
  File "Release\Content\Models\Facility\FacilityNames_0.xnb"
  File "Release\Content\Models\Facility\gaia_defense.xnb"
  File "Release\Content\Models\Facility\generalstorage.xnb"
  File "Release\Content\Models\Facility\grav_shield.xnb"
  File "Release\Content\Models\Facility\laser_defense.xnb"
  File "Release\Content\Models\Facility\launch_pad.xnb"
  File "Release\Content\Models\Facility\long_range_neudar.xnb"
  File "Release\Content\Models\Facility\missile_defense.xnb"
  File "Release\Content\Models\Facility\neural_shield.xnb"
  File "Release\Content\Models\Facility\plasma_defense.xnb"
  File "Release\Content\Models\Facility\psi_training.xnb"
  File "Release\Content\Models\Facility\research_facility.xnb"
  File "Release\Content\Models\Facility\short_range_neudar.xnb"
  File "Release\Content\Models\Facility\tachyon_detector.xnb"
  File "Release\Content\Models\Facility\workshop.xnb"
  SetOutPath "$INSTDIR\Content\Models\Facility\Xnet"
  File "Release\Content\Models\Facility\Xnet\alien_containment.FBX"
  File "Release\Content\Models\Facility\Xnet\alien_containment.xnb"
  File "Release\Content\Models\Facility\Xnet\Barracks.FBX"
  File "Release\Content\Models\Facility\Xnet\Barracks.xnb"
  File "Release\Content\Models\Facility\Xnet\Research_Facility.FBX"
  File "Release\Content\Models\Facility\Xnet\Research_Facility.xnb"
  SetOutPath "$INSTDIR\Content\Models\Weapons\Alien"
  File "Release\Content\Models\Weapons\Alien\stun launcher.xnb"
  File "Release\Content\Models\Weapons\Alien\stun-launcher-col_0.xnb"
  SetOutPath "$INSTDIR\Content\Models\Weapons\Xcorps"
  File "Release\Content\Models\Weapons\Xcorps\assault rifle.xnb"
  File "Release\Content\Models\Weapons\Xcorps\assault-rifle-col_0.xnb"
  File "Release\Content\Models\Weapons\Xcorps\barreta_0.xnb"
  File "Release\Content\Models\Weapons\Xcorps\grenade-col_0.xnb"
  File "Release\Content\Models\Weapons\Xcorps\grenade.xnb"
  File "Release\Content\Models\Weapons\Xcorps\Laser Rifle.xnb"
  File "Release\Content\Models\Weapons\Xcorps\pistol.xnb"
  File "Release\Content\Models\Weapons\Xcorps\smoke-grenade-col_0.xnb"
  File "Release\Content\Models\Weapons\Xcorps\smoke-grenade.xnb"
  SetOutPath "$INSTDIR\Content\Models\Xcaps"
  File "Release\Content\Models\Xcaps\xcap-cannon-col_0.xnb"
  File "Release\Content\Models\Xcaps\xcap-cannon.xnb"
  File "Release\Content\Models\Xcaps\xcap-laser-col_0.xnb"
  File "Release\Content\Models\Xcaps\xcap-laser.xnb"
  File "Release\Content\Models\Xcaps\xcap-missile-col_0.xnb"
  File "Release\Content\Models\Xcaps\xcap-missile.xnb"
  File "Release\Content\Models\Xcaps\Xcaps-GAIA-col_0.xnb"
  File "Release\Content\Models\Xcaps\Xcaps-hover-col_0.xnb"
  File "Release\Content\Models\Xcaps\Xcaps-hover-GAIA.xnb"
  File "Release\Content\Models\Xcaps\Xcaps-hover-plasma.xnb"
  File "Release\Content\Models\Xcaps\Xcaps-plasma-col_0.xnb"
  File "Release\Content\Models\Xcaps\xcap_body_col_0.xnb"
  SetOutPath "$INSTDIR\Content\Models"
  File "Release\Content\Models\XCorps.xnb"
  File "Release\Content\Models\xcorps_0.xnb"
  SetOutPath "$INSTDIR\Content\Schema"
  File "Release\Content\Schema\basic.xsd"
  File "Release\Content\Schema\combatant.xml"
  File "Release\Content\Schema\combatant.xsd"
  File "Release\Content\Schema\facility.xml"
  File "Release\Content\Schema\facility.xsd"
  File "Release\Content\Schema\item.xml"
  File "Release\Content\Schema\item.xsd"
  File "Release\Content\Schema\peopleNames.xml"
  File "Release\Content\Schema\planets.xml"
  File "Release\Content\Schema\planets.xsd"
  File "Release\Content\Schema\research.xml"
  File "Release\Content\Schema\research.xsd"
  File "Release\Content\Schema\startsettings.xml"
  File "Release\Content\Schema\startsettings.xsd"
  File "Release\Content\Schema\xnet.xml"
  File "Release\Content\Schema\xnet.xsd"
  SetOutPath "$INSTDIR\Content\Shaders"
  File "Release\Content\Shaders\GeoscapeShader.xnb"
  File "Release\Content\Shaders\skybox.xnb"
  SetOutPath "$INSTDIR\Content"
  File "Release\Content\SpriteFont1.xnb"
  SetOutPath "$INSTDIR\Content\Textures\Battlescape"
  File "Release\Content\Textures\Battlescape\textureAtlas.png"
  SetOutPath "$INSTDIR\Content\Textures\EquipSoldier"
  File "Release\Content\Textures\EquipSoldier\EquipScreenBackground.png"
  File "Release\Content\Textures\EquipSoldier\InventorySprites.png"
  File "Release\Content\Textures\EquipSoldier\InventorySprites.txt"
  SetOutPath "$INSTDIR\Content\Textures\Geoscape"
  File "Release\Content\Textures\Geoscape\EarthDiffuseMap.jpg"
  File "Release\Content\Textures\Geoscape\EarthNightMap.png"
  File "Release\Content\Textures\Geoscape\EarthNormalMap.png"
  File "Release\Content\Textures\Geoscape\skybox.png"
  SetOutPath "$INSTDIR\Content\Textures\OutpostLayout"
  File "Release\Content\Textures\OutpostLayout\BuildTimes.xnb"
  SetOutPath "$INSTDIR\Content\Textures\UI"
  File "Release\Content\Textures\UI\BasesScreenBackground.png"
  File "Release\Content\Textures\UI\GeoscapeScreenBackground.png"
  File "Release\Content\Textures\UI\StartScreenBackground.png"
  File "Release\Content\Textures\UI\XnetScreenBackground.png"
  SetOutPath "$INSTDIR"
  File "Release\Falagard.xsd"
  File "Release\FMOD.dll"
  File "Release\FMOD.pdb"
  File "Release\fmodex.dll"
  File "Release\GUILayout.xsd"
  File "Release\Imageset.xsd"
  SetOutPath "$INSTDIR\Resources"
  File "Release\Resources\TaharezLook.imageset"
  File "Release\Resources\TaharezLook.tga"
  File "Release\Resources\XenoNew.png"
  SetOutPath "$INSTDIR"
  File "Icons\Game.ico"
  File "Release\Xenocide.exe"
  File "Release\Xenocide.pdb"
  File "Release\Xenocide.Pipeline.dll"
  File "Release\Xenocide.Pipeline.pdb"
  File "Release\Xenocide.Test.dll"
  File "Release\Xenocide.Test.pdb"
  CreateDirectory "$INSTDIR\Bonus"
  SetDetailsPrint textonly
  Detailprint "Main program files installed!"

# (3.6) Custom installation shortcuts/keys

  # Custom keys
  WriteRegStr HKLM "Software\Project Xenocide" "LastInstallPath" "$INSTDIR"

  # Shortcuts
  CreateShortCut "$DESKTOP\${DESKTOP_LINK_NAME}" "$INSTDIR\Xenocide.exe"
  WriteIniStr    "$INSTDIR\${HOMEPAGE_URL_NAME}" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateDirectory "${SM_NAME}"
  CreateShortCut "${SM_NAME}\${PLAY_LINK_NAME}" "$INSTDIR\Xenocide.exe"
  WriteIniStr    "${SM_NAME}\${HOMEPAGE_URL_NAME}" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "${SM_NAME}\${UNINSTALL_LINK_NAME}" "$INSTDIR\uninst.exe"
  CreateShortCut "${SM_NAME}\${STARTSETTINGS_LINK_NAME}" "$INSTDIR\Content\Schema\startsettings.xml"
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Xenocide.exe"

SectionEnd


# (3.7) Bonus Section
SectionGroup /e "Bonus Material" SEC_BONUS

  # Audio Pack
  Section /o "Audio" SEC_AUDIO
     AddSize 16848
     Call DL_Audio
  SectionEnd

  # Globe Texture
  Section /o "Better Globe" SEC_GLOBE
     AddSize 2146
     Call DL_Globe
  sectionEnd

SectionGroupEnd


Section -Post

  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Xenocide.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  Call CallLogDump

SectionEnd


# (4.1) un.onInit
Function un.onInit

!insertmacro MUI_UNGETLANGUAGE

   # Check if already running
   # If so don't open another but bring to front
   BringToFront
   System::Call "kernel32::CreateMutexA(i 0, i 0, t '$(^Name)') i .r0 ?e"
   Pop $0
   StrCmp $0 0 launch
    StrLen $0 "$(^Name)"
    IntOp $0 $0 + 1

   loop:
     FindWindow $1 '#32770' '' 0 $1
     IntCmp $1 0 +4
     System::Call "user32::GetWindowText(i r1, t .r2, i r0) i."
     StrCmp $2 "$(^Name)" 0 loop
     System::Call "user32::ShowWindow(i r1,i 9) i."         ; If minimized then maximize
     System::Call "user32::SetForegroundWindow(i r1) i."    ; Bring to front
     Abort

   launch:
     MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
     Abort

FunctionEnd


Function un.onUninstSuccess

  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."

FunctionEnd


# (4.2) Main uninstall section
Section Uninstall

  MessageBox MB_YESNO|MB_ICONQUESTION "Would you like to keep your profile and saved games?" IDYES pass IDNO remove_saves

remove_saves:
   ReadRegStr $0 HKCU "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" Personal

   /* TODO: Replace <Savegame folder> with the real folder
   RMDir /r "$0\<Savegame folder>"
   */
   Goto pass

#########################################################################################
# WARNING: Depending on the compilation enviroment, the following file tree may differ. #
#          If an error occurs, change the file paths accordingly.                       #
#########################################################################################

pass:
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\Xenocide.Test.pdb"
  Delete "$INSTDIR\Xenocide.Test.dll"
  Delete "$INSTDIR\Xenocide.Pipeline.pdb"
  Delete "$INSTDIR\Xenocide.Pipeline.dll"
  Delete "$INSTDIR\Xenocide.pdb"
  Delete "$INSTDIR\Xenocide.exe"
  Delete "$INSTDIR\Resources\XenoNew.png"
  Delete "$INSTDIR\Resources\TaharezLook.tga"
  Delete "$INSTDIR\Resources\TaharezLook.imageset"
  Delete "$INSTDIR\Imageset.xsd"
  Delete "$INSTDIR\GUILayout.xsd"
  Delete "$INSTDIR\fmodex.dll"
  Delete "$INSTDIR\FMOD.pdb"
  Delete "$INSTDIR\FMOD.dll"
  Delete "$INSTDIR\Falagard.xsd"
  Delete "$INSTDIR\Content\Textures\UI\XnetScreenBackground.png"
  Delete "$INSTDIR\Content\Textures\UI\StartScreenBackground.png"
  Delete "$INSTDIR\Content\Textures\UI\GeoscapeScreenBackground.png"
  Delete "$INSTDIR\Content\Textures\UI\BasesScreenBackground.png"
  Delete "$INSTDIR\Content\Textures\OutpostLayout\BuildTimes.xnb"
  Delete "$INSTDIR\Content\Textures\Geoscape\skybox.png"
  Delete "$INSTDIR\Content\Textures\Geoscape\EarthNormalMap.png"
  Delete "$INSTDIR\Content\Textures\Geoscape\EarthNightMap.png"
  Delete "$INSTDIR\Content\Textures\Geoscape\EarthDiffuseMap.jpg"
  Delete "$INSTDIR\Content\Textures\EquipSoldier\InventorySprites.txt"
  Delete "$INSTDIR\Content\Textures\EquipSoldier\InventorySprites.png"
  Delete "$INSTDIR\Content\Textures\EquipSoldier\EquipScreenBackground.png"
  Delete "$INSTDIR\Content\Textures\Battlescape\textureAtlas.png"
  Delete "$INSTDIR\Content\SpriteFont1.xnb"
  Delete "$INSTDIR\Content\Shaders\skybox.xnb"
  Delete "$INSTDIR\Content\Shaders\GeoscapeShader.xnb"
  Delete "$INSTDIR\Content\Schema\xnet.xsd"
  Delete "$INSTDIR\Content\Schema\xnet.xml"
  Delete "$INSTDIR\Content\Schema\startsettings.xsd"
  Delete "$INSTDIR\Content\Schema\startsettings.xml"
  Delete "$INSTDIR\Content\Schema\research.xsd"
  Delete "$INSTDIR\Content\Schema\research.xml"
  Delete "$INSTDIR\Content\Schema\planets.xsd"
  Delete "$INSTDIR\Content\Schema\planets.xml"
  Delete "$INSTDIR\Content\Schema\peopleNames.xml"
  Delete "$INSTDIR\Content\Schema\item.xsd"
  Delete "$INSTDIR\Content\Schema\item.xml"
  Delete "$INSTDIR\Content\Schema\facility.xsd"
  Delete "$INSTDIR\Content\Schema\facility.xml"
  Delete "$INSTDIR\Content\Schema\combatant.xsd"
  Delete "$INSTDIR\Content\Schema\combatant.xml"
  Delete "$INSTDIR\Content\Schema\basic.xsd"
  Delete "$INSTDIR\Content\Models\xcorps_0.xnb"
  Delete "$INSTDIR\Content\Models\XCorps.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap_body_col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\Xcaps-plasma-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\Xcaps-hover-plasma.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\Xcaps-hover-GAIA.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\Xcaps-hover-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\Xcaps-GAIA-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-missile.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-missile-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-laser.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-laser-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-cannon.xnb"
  Delete "$INSTDIR\Content\Models\Xcaps\xcap-cannon-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\smoke-grenade.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\smoke-grenade-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\pistol.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\Laser Rifle.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\grenade.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\grenade-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\barreta_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\assault-rifle-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Xcorps\assault rifle.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Alien\stun-launcher-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Weapons\Alien\stun launcher.xnb"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\Research_Facility.xnb"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\Research_Facility.FBX"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\Barracks.xnb"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\Barracks.FBX"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\alien_containment.xnb"
  Delete "$INSTDIR\Content\Models\Facility\Xnet\alien_containment.FBX"
  Delete "$INSTDIR\Content\Models\Facility\workshop.xnb"
  Delete "$INSTDIR\Content\Models\Facility\tachyon_detector.xnb"
  Delete "$INSTDIR\Content\Models\Facility\short_range_neudar.xnb"
  Delete "$INSTDIR\Content\Models\Facility\research_facility.xnb"
  Delete "$INSTDIR\Content\Models\Facility\psi_training.xnb"
  Delete "$INSTDIR\Content\Models\Facility\plasma_defense.xnb"
  Delete "$INSTDIR\Content\Models\Facility\neural_shield.xnb"
  Delete "$INSTDIR\Content\Models\Facility\missile_defense.xnb"
  Delete "$INSTDIR\Content\Models\Facility\long_range_neudar.xnb"
  Delete "$INSTDIR\Content\Models\Facility\launch_pad.xnb"
  Delete "$INSTDIR\Content\Models\Facility\laser_defense.xnb"
  Delete "$INSTDIR\Content\Models\Facility\grav_shield.xnb"
  Delete "$INSTDIR\Content\Models\Facility\generalstorage.xnb"
  Delete "$INSTDIR\Content\Models\Facility\gaia_defense.xnb"
  Delete "$INSTDIR\Content\Models\Facility\FacilityNames_0.xnb"
  Delete "$INSTDIR\Content\Models\Facility\base_access.xnb"
  Delete "$INSTDIR\Content\Models\Facility\barracks.xnb"
  Delete "$INSTDIR\Content\Models\Facility\alien_containment.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Xcorps\xc33.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Xcorps\xc1.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Xcorps\XC1-hull-col_0.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Xcorps\AvengInterior_0.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Xcorps\AvengExterior_0.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Weapons\STINGRAY_0.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Weapons\Stingray.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Weapons\Avalanche_0.xnb"
  Delete "$INSTDIR\Content\Models\Craft\Weapons\Avalanche.xnb"
  Delete "$INSTDIR\Content\Models\Characters\XCorp\FemaleShirt.xnb"
  Delete "$INSTDIR\Content\Models\Characters\Alien\Viper.xnb"
  Delete "$INSTDIR\Content\Models\Characters\Alien\Silibrate.xnb"
  Delete "$INSTDIR\Content\Models\Characters\Alien\Morlock.xnb"
  Delete "$INSTDIR\Content\Models\Characters\Alien\f_silab_0.xnb"
  Delete "$INSTDIR\Content\Models\Characters\Alien\alien-morlock-01_0.xnb"
  Delete "$INSTDIR\Content\Layouts\UfoInfoDialog.layout"
  Delete "$INSTDIR\Content\Layouts\TrackingLostDialog.layout"
  Delete "$INSTDIR\Content\Layouts\StartBattlescapeDialog.layout"
  Delete "$INSTDIR\Content\Layouts\SoundOptionsDialog.layout"
  Delete "$INSTDIR\Content\Layouts\PickActionDialog.layout"
  Delete "$INSTDIR\Content\Layouts\OptionsDialog.layout"
  Delete "$INSTDIR\Content\Layouts\NameNewBaseDialog.layout"
  Delete "$INSTDIR\Content\Layouts\LaunchInterceptDialog.layout"
  Delete "$INSTDIR\Content\Layouts\BuildFacilityDialog.layout"
  Delete "$INSTDIR\Content\Layouts\AircraftOrdersDialog.layout"
  Delete "$INSTDIR\Content\FBX\E3851FC0\FemaleShirt.fbm\FShirt(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\A58B1EC5\Laser Rifle.fbm\h_l_rifle(diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm\TV(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm\Tube(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm\Server(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm\Microscope(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm\Lab(Diffuse)_0.xnb"
  Delete "$INSTDIR\Content\FBX\461320FC\alien_containment.fbm\post-36-1077887638[1]_0.xnb"
  Delete "$INSTDIR\Content\FBX\461320FC\alien_containment.fbm\alien1_0.xnb"
  Delete "$INSTDIR\Content\FBX\2A721D3D\Barracks.fbm\TABLE_0.xnb"
  Delete "$INSTDIR\Content\FBX\2A721D3D\Barracks.fbm\BARRACKS_0.xnb"
  Delete "$INSTDIR\Content\FBX\29071D2F\Viper.fbm\snake1_0.xnb"
  Delete "$INSTDIR\Content\DataFiles\Geoscape\terrain.png"
  Delete "$INSTDIR\Content\DataFiles\Geoscape\regions.png"
  Delete "$INSTDIR\Content\DataFiles\Geoscape\countries.png"
  Delete "$INSTDIR\Content\DataFiles\credits.txt"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\zoomout.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\zoomin.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\speedveryfast.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\speedslow.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\speedfast.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\PlanetView\clickobjectonplanet.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\Menu\exitgame.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\Menu\buttonover.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\Menu\buttonclick2_changesetting.ogg"
  Delete "$INSTDIR\Content\Audio\Sounds\Menu\buttonclick1_ok.ogg"
  Delete "$INSTDIR\Content\Audio\Music\XNet\xnet.ogg"
  Delete "$INSTDIR\Content\Audio\Music\Planetview\Tiskaite_-_Xenocide_Geoscape.ogg"
  Delete "$INSTDIR\Content\Audio\Music\Planetview\planetview.ogg"
  Delete "$INSTDIR\Content\Audio\Music\Planetview\10. Thomas Torfs - Planetview.ogg"
  Delete "$INSTDIR\Content\Audio\Music\main_theme.ogg"
  Delete "$INSTDIR\Content\Audio\Music\Baseview\7. XerO - Baseview.ogg"
  Delete "$INSTDIR\CeGui.xml"
  Delete "$INSTDIR\CeGui.WidgetSets.Taharez.xml"
  Delete "$INSTDIR\CeGui.WidgetSets.Taharez.pdb"
  Delete "$INSTDIR\CeGui.WidgetSets.Taharez.dll"
  Delete "$INSTDIR\CeGui.Renderers.Xna.pdb"
  Delete "$INSTDIR\CeGui.Renderers.Xna.dll"
  Delete "$INSTDIR\CeGui.pdb"
  Delete "$INSTDIR\CeGui.dll"

  RMDir "$INSTDIR\Resources"
  RMDir "$INSTDIR\Content\Textures\UI"
  RMDir "$INSTDIR\Content\Textures\OutpostLayout"
  RMDir "$INSTDIR\Content\Textures\Geoscape"
  RMDir "$INSTDIR\Content\Textures\EquipSoldier"
  RMDir "$INSTDIR\Content\Textures\Battlescape"
  RMDir "$INSTDIR\Content\Shaders"
  RMDir "$INSTDIR\Content\Schema"
  RMDir "$INSTDIR\Content\Models\Xcaps"
  RMDir "$INSTDIR\Content\Models\Weapons\Xcorps"
  RMDir "$INSTDIR\Content\Models\Weapons\Alien"
  RMDir "$INSTDIR\Content\Models\Facility\Xnet"
  RMDir "$INSTDIR\Content\Models\Facility"
  RMDir "$INSTDIR\Content\Models\Craft\Xcorps"
  RMDir "$INSTDIR\Content\Models\Craft\Weapons"
  RMDir "$INSTDIR\Content\Models\Characters\XCorp"
  RMDir "$INSTDIR\Content\Models\Characters\Alien"
  RMDir "$INSTDIR\Content\Models"
  RMDir "$INSTDIR\Content\Layouts"
  RMDir "$INSTDIR\Content\FBX\E3851FC0\FemaleShirt.fbm"
  RMDir "$INSTDIR\Content\FBX\A58B1EC5\Laser Rifle.fbm"
  RMDir "$INSTDIR\Content\FBX\461820F5\Research_Facility.fbm"
  RMDir "$INSTDIR\Content\FBX\461320FC\alien_containment.fbm"
  RMDir "$INSTDIR\Content\FBX\2A721D3D\Barracks.fbm"
  RMDir "$INSTDIR\Content\FBX\29071D2F\Viper.fbm"
  RMDir "$INSTDIR\Content\DataFiles\Geoscape"
  RMDir "$INSTDIR\Content\DataFiles"
  RMDir "$INSTDIR\Content\Audio\Sounds\PlanetView"
  RMDir "$INSTDIR\Content\Audio\Sounds\Menu"
  RMDir "$INSTDIR\Content\Audio\Music\XNet"
  RMDir "$INSTDIR\Content\Audio\Music\Planetview"
  RMDir "$INSTDIR\Content\Audio\Music\Baseview"
  RMDir "$INSTDIR\Content\Audio\Music"
  RMDir /r "$INSTDIR\Content"

# (4.3) Custom file/key uninstallation

  Delete "$DESKTOP\${DESKTOP_LINK_NAME}"
  Delete "${SM_NAME}\${UNINSTALL_LINK_NAME}"
  Delete "${SM_NAME}\${STARTSETTINGS_LINK_NAME}"
  Delete "${SM_NAME}\${HOMEPAGE_URL_NAME}"
  Delete "${SM_NAME}\${PLAY_LINK_NAME}"
  RMDir  "${SM_NAME}"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  DeleteRegKey HKLM "Software\Project Xenocide"

SectionEnd


# (5.1) Section Descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN

  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_CORE}  "The main game files!"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_BONUS} "Some additional bonus files, not needed for the game to run."
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_Audio} "Sound effects and music files."
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_GLOBE} "A more detailed globe texture. Uses more Video Memory and takes longer to load."

!insertmacro MUI_FUNCTION_DESCRIPTION_END



# (5.2) DumpLog function

Function CallLogDump
         strcpy $0 "$INSTDIR\install.log"
         Push $0
         Call DumpLog
FunctionEnd

Function DumpLog

  Exch $5
  Push $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $6

  FindWindow $0 "#32770" "" $HWNDPARENT
  GetDlgItem $0 $0 1016
  StrCmp $0 0 error
  FileOpen $5 $5 "w"
  StrCmp $5 0 error

    SendMessage $0 ${LVM_GETITEMCOUNT} 0 0 $6
    System::Alloc ${NSIS_MAX_STRLEN}

    Pop $3
    StrCpy $2 0

    System::Call "*(i, i, i, i, i, i, i, i, i) i \
      (0, 0, 0, 0, 0, r3, ${NSIS_MAX_STRLEN}) .r1"

    loop: StrCmp $2 $6 done
      System::Call "User32::SendMessageA(i, i, i, i) i \
        ($0, ${LVM_GETITEMTEXT}, $2, r1)"
        
      System::Call "*$3(&t${NSIS_MAX_STRLEN} .r4)"

      FileWrite $5 "$4$\r$\n"
      IntOp $2 $2 + 1
      Goto loop

    done:

      FileClose $5
      System::Free $1
      System::Free $3
      Goto exit

  error:
    MessageBox MB_OK "Error while dumping install.log"

  exit:
    Pop $6
    Pop $4
    Pop $3
    Pop $2
    Pop $1
    Pop $0
    Exch $5

FunctionEnd
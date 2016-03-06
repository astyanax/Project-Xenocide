Function DL_Audio

   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\zoomout.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\zoomin.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\speedveryfast.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\speedslow.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\speedfast.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\PlanetView\clickobjectonplanet.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\Menu\exitgame.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\Menu\buttonover.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\Menu\buttonclick2_changesetting.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Sounds\Menu\buttonclick1_ok.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\XNet\xnet.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\Planetview\Tiskaite_-_Xenocide_Geoscape.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\Planetview\planetview.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\Planetview\10. Thomas Torfs - Planetview.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\main_theme.ogg" 0 checkIfPackExists
   IfFileExists "$INSTDIR\Content\Audio\Music\Baseview\7. XerO - Baseview.ogg" found.Audio checkIfPackExists

   checkIfPackExists:
      ; Check for the file in the bonus folder
      IfFileExists "$INSTDIR\Bonus\Xenocide_Audio_Pack.zip" 0 notFound.Audio
      ; If found, extract
      ZipDLL::extractall "$INSTDIR\Bonus\Xenocide_Audio_Pack.zip" "$INSTDIR"
      goto End

   notFound.Audio:
      MessageBox MB_YESNOCANCEL|MB_ICONQUESTION "Choose Yes to automatically download/install the Audio pack$\r$\nChoose no if you'd like to download and extract it manually (the download page will open automatically)$\r$\nChoose cancel to abort the Audio pack installation and continue with the Xenocide one." IDYES DL IDNO IndirectDL
      goto End

   IndirectDL:
      ExecShell open "http://www.projectxenocide.com/downloads.html" "" SW_SHOWNORMAL
      goto End

   DL:
      StrCpy $1 "$INSTDIR\Bonus\Xenocide_Audio_Pack.zip"
      NSISdl::download "http://www.projectxenocide.com/download/Xenocide_Audio_Pack.zip" $1
      Pop $0

      StrCmp $0 success dl_sucessful
      SetDetailsView show
      DetailPrint "Failed to download the Audio Pack: $0"
      MessageBox MB_OK "Xenocide Audio Pack could not be installed, download was aborted. Installation will now continue"
      goto End

   dl_sucessful:
      DetailPrint "Extracting the Audio Pack"
      ZipDLL::extractall "$INSTDIR\Bonus\Xenocide_Audio_Pack.zip" "$INSTDIR"
      DetailPrint "Audio Pack extracted successfully!"
      goto End

   found.Audio:
      DetailPrint "Audio Pack sucessfully installed!"
      goto End

   End:
FunctionEnd

# To be converted
Function DL_Globe

   ; Check for the file in the bonus folder
   IfFileExists "$INSTDIR\Bonus\Xenocide_HighDef_Globe.zip" 0 notFound.globe
   ; If found, extract
   ZipDLL::extractall "$INSTDIR\Bonus\Xenocide_HighDef_Globe.zip" "$INSTDIR"
   goto found.globe

   notFound.globe:
      MessageBox MB_YESNOCANCEL|MB_ICONQUESTION "Choose Yes to automatically download/install the globe texture$\r$\nChoose no if you'd like to download and extract it manually (the download page will open automatically)$\r$\nChoose cancel to abort the globe texture installation and continue with the Xenocide one." IDYES DL IDNO IndirectDL
      goto End

   IndirectDL:
      ExecShell open "http://www.projectxenocide.com/download/Xenocide_HighDef_Globe.zip" "" SW_SHOWNORMAL
      goto End

   DL:
      StrCpy $2 "$INSTDIR\Bonus\Xenocide_HighDef_Globe.zip"
      NSISdl::download "http://www.projectxenocide.com/download/Xenocide_HighDef_Globe.zip" $2
      Pop $0

      StrCmp $0 success dl_sucessful
      SetDetailsView show
      DetailPrint "Failed to download the globe texture: $0"
      MessageBox MB_OK "Xenocide Improved Globe Texture could not be installed, download was aborted. Installation will now continue"
      goto End

   dl_sucessful:
      DetailPrint "Extracting the Improved Globe Texture"
      ZipDLL::extractall "$INSTDIR\Bonus\Xenocide_HighDef_Globe.zip" "$INSTDIR"
      DetailPrint "Globe Texture extracted successfully!"
      goto found.globe

   found.globe:
      DetailPrint "Globe Texture sucessfully installed!"
      goto End

   End:
FunctionEnd
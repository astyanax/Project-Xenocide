Function DirectX_Install
      SetDetailsPrint none
      SetOutPath "$TEMP"
      SetOverwrite try
      File "Installers\dxwebsetup.exe"

      SetDetailsPrint both
      DetailPrint "Updating DirectX"

      SetDetailsPrint none
      ExecWait 'dxwebsetup.exe /Q'
      ; Now install DirectX9
      
      SetDetailsPrint both
      DetailPrint "* DirectX installed! Proceeding with remainder of installation."
      #DetailPrint "Removing temp DirectX installation file..."
      #Delete $2
      #DetailPrint "DirectX installation file removed successfully"
      goto End
   End:

FunctionEnd
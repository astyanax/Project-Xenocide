; Check if XNA 3.0 is installed
# TODO: give credit/link

Function IsXNAInstalled

   Push $0

   # If we can't find XNA in the "common files" folder, look at the registry
   IfFileExists "$COMMONFILES\Microsoft Shared\XNA\Framework\v3.0\*.*" found.XNA notFound.XNA    

   # If the key doesn't exist, then XNA is surely NOT installed!
   EnumRegKey $0 HKLM "SOFTWARE\Microsoft\XNA\Framework\" 0
   StrCmp $0 "" notFound.XNA found.XNA

   notFound.XNA:
      StrCpy $0 0
      Goto done

   found.XNA:
      StrCpy $0 1

   done:
      StrCpy $R1 $0
      Exec $0

FunctionEnd

Function XNA_Check

   SetDetailsPrint none
   # Keeping the path in a variable for easier access
   StrCpy $2 "$TEMP\xnafx_redist.msi"
      
   # We will save the $0 variable in the stack
   call IsXNAInstalled
   # and retrieve it after calling IsXNAInstalled
   pop $0

   StrCmp $R1 1 found.XNA checkForInstaller.XNA
   
   checkForInstaller.XNA:
      ; Check for the file in temp folder
      IfFileExists $2 0 notFound.XNA
      SetDetailsPrint both

### A useful link explaining msiexec's command-line options ###
# http://helpnet.acresso.com/robo/projects/helplibdevstudio9/IHelpCmdLineMSI.htm

      ; If found, extract quietly and run
      DetailPrint "Installing XNA Framework 3.0"

      SetDetailsPrint none
      ExecWait 'msiexec /i $2 /passive /norestart'

      SetDetailsPrint both
      DetailPrint "* XNA Framework 3.0 installed successfully!"

      goto End

   notFound.XNA:
      SetDetailsPrint none
      SetOutPath "$TEMP"
      SetOverwrite try
      File "Installers\xnafx_redist.msi"

      SetDetailsPrint both
      DetailPrint "Installing XNA Framework 3.0"
      
      SetDetailsPrint none
      ExecWait 'msiexec /i $TEMP\xnafx_redist.msi /passive /norestart'

      SetDetailsPrint both
      DetailPrint "* XNA Framework 3.0 installed successfully!"
      
      #DetailPrint "Removing temp XNA installation file..."
      #Delete $2
      #DetailPrint "XNA installation file removed successfully"
      goto End

   found.XNA:
      SetDetailsPrint both
      DetailPrint "* Found XNA Framework 3.0! Proceeding with remainder of installation."
      goto End

   End:
      SetDetailsPrint none

FunctionEnd
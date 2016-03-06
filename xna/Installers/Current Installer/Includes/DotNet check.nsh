# DotNET version checking/installing macro.
# Written by AnarkiNet(AnarkiNet@gmail.com)
# Modified by eyal0 (for use in http://www.sourceforge.net/projects/itwister)
# Identation and further modification by Kafros (http://www.projectxenocide.com)

!macro CheckDotNET DotNetReqVer

   DetailPrint "Checking your .NET Framework version..."
   ;callee register save
   Push $0
   Push $1
   Push $2
   Push $3
   Push $4
   Push $5
   Push $6 ;backup of intsalled ver
   Push $7 ;backup of DoNetReqVer

   StrCpy $7 ${DotNetReqVer}

   System::Call "mscoree::GetCORVersion(w .r0, i ${NSIS_MAX_STRLEN}, *i r2r2) i .r1 ?u"

   ${If} $0 == ''
   	DetailPrint "* .NET Framework not found, download is required for program to run."
     Goto NoDotNET
   ${EndIf}

   ;at this point, $0 has maybe v2.345.678.
   StrCpy $0 $0 $2 1 ;remove the starting "v", $0 has the installed version num as a string
   StrCpy $6 $0
   StrCpy $1 $7 ;$1 has the requested verison num as a string

   ;now let's compare the versions, installed against required <part0>.<part1>.<part2>.
   ${Do}
      StrCpy $2 "" ;clear out the installed part
      StrCpy $3 "" ;clear out the required part

      ${Do}
         ${If} $0 == "" ;if there are no more characters in the version
            StrCpy $4 "." ;fake the end of the version string
         ${Else}
            StrCpy $4 $0 1 0 ;$4 = character from the installed ver
            ${If} $4 != "."
               StrCpy $0 $0 ${NSIS_MAX_STRLEN} 1 ;remove that first character from the remaining
            ${EndIf}
         ${EndIf}

         ${If} $1 == ""  ;if there are no more characters in the version
            StrCpy $5 "." ;fake the end of the version string
         ${Else}
            StrCpy $5 $1 1 0 ;$5 = character from the required ver
            ${If} $5 != "."
               StrCpy $1 $1 ${NSIS_MAX_STRLEN} 1 ;remove that first character from the remaining
            ${EndIf}
         ${EndIf}

         ${If} $4 == "."
         ${AndIf} $5 == "."
            ${ExitDo} ;we're at the end of the part
         ${EndIf}

         ${If} $4 == "." ;if we're at the end of the current installed part
            StrCpy $2 "0$2" ;put a zero on the front
         ${Else} ;we have another character
            StrCpy $2 "$2$4" ;put the next character on the back
         ${EndIf}

         ${If} $5 == "." ;if we're at the end of the current required part
            StrCpy $3 "0$3" ;put a zero on the front
         ${Else} ;we have another character
            StrCpy $3 "$3$5" ;put the next character on the back
         ${EndIf}
      ${Loop}

      ${If} $0 != "" ;let's remove the leading period on installed part if it exists
         StrCpy $0 $0 ${NSIS_MAX_STRLEN} 1
      ${EndIf}
      ${If} $1 != "" ;let's remove the leading period on required part if it exists
         StrCpy $1 $1 ${NSIS_MAX_STRLEN} 1
      ${EndIf}

      ;$2 has the installed part, $3 has the required part
      ${If} $2 S< $3
         IntOp $0 0 - 1 ;$0 = -1, installed less than required
         ${ExitDo}
      ${ElseIf} $2 S> $3
         IntOp $0 0 + 1 ;$0 = 1, installed greater than required
         ${ExitDo}
      ${ElseIf} $2 == ""
      ${AndIf} $3 == ""
         IntOp $0 0 + 0 ;$0 = 0, the versions are identical
         ${ExitDo}
      ${EndIf} ;otherwise we just keep looping through the parts
   ${Loop}

   ${If} $0 < 0
      DetailPrint '* .NET Framework Version found: "$6", but is older than the required version: "$7"'
      Goto OldDotNET
   ${Else}
      DetailPrint '* .NET Framework Version found: "$6", equal or newer to required version: "$7"'
      Goto NewDotNET
   ${EndIf}

NoDotNET:
    MessageBox MB_YESNOCANCEL|MB_ICONEXCLAMATION \
    ".NET Framework not installed.$\nRequired Version: $7.$\nDownload and install .NET Framework automatically?" \
    /SD IDYES IDYES DownloadDotNET IDNO NewDotNET
    goto GiveUpDotNET ;IDCANCEL

OldDotNET:
    MessageBox MB_YESNOCANCEL|MB_ICONEXCLAMATION \
    "Your .NET Framework version: $6.$\nRequired Version: $7.$\nDownload and install .NET Framework automatically?" \
    /SD IDYES IDYES DownloadDotNET IDNO NewDotNET
    goto GiveUpDotNET ;IDCANCEL

DownloadDotNET:
   ; Check for the file in temp folder
   IfFileExists "$TEMP\NetFx20SP1_x86.exe" 0 DownloadDotNet_continue
   ; If found, extract quietly and run
   DetailPrint "Pausing installation while downloaded .NET Framework installer runs."
   ExecWait '$TEMP\NetFx20SP1_x86.exe /q /c:"install /q"' #
   DetailPrint "* Completed .NET Framework install/update."
   SetRebootFlag true
   goto NewDotNET

DownloadDotNet_continue:
   !insertmacro DL_dotNet

   ${If} $0 == "cancel"
      MessageBox MB_YESNO|MB_ICONEXCLAMATION \
      "Download cancelled.  Continue Installation?" \
      IDYES NewDotNET IDNO GiveUpDotNET
   ${ElseIf} $0 != "success"
      MessageBox MB_YESNO|MB_ICONEXCLAMATION \
      "Download failed:$\n$0$\n$\nContinue Installation?" \
      IDYES NewDotNET IDNO GiveUpDotNET
   ${EndIf}

   DetailPrint "Pausing installation while downloaded .NET Framework installer runs."
   ExecWait '$TEMP\NetFx20SP1_x86.exe /q /c:"install /q"' #
   DetailPrint "* Completed .NET Framework install/update."
   # DetailPrint "Removing .NET Framework installer."
   # Delete "$TEMP\NetFx20SP1_x86.exe"
   # DetailPrint ".NET Framework installer removed."
   SetRebootFlag true
   goto NewDotNet

GiveUpDotNET:
   Abort "Installation cancelled by user."

NewDotNET:
   DetailPrint "* Found/Installed .Net Framework 2 SP1! Proceeding with remainder of installation."
   Pop $0
   Pop $1
   Pop $2
   Pop $3
   Pop $4
   Pop $5
   Pop $6 ;backup of intsalled ver
   Pop $7 ;backup of DoNetReqVer
!macroend

!macro DL_dotNet

   DetailPrint "Beginning download of .NET Framework 2.0 SP1."

   Push "http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=79bc3b77-e02c-4ad3-aacf-a7633f706ba5&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f0%2f8%2fc%2f08c19fa4-4c4f-4ffb-9d6c-150906578c9e%2fNetFx20SP1_x86.exe"
   Push "http://download.microsoft.com/download/0/8/c/08c19fa4-4c4f-4ffb-9d6c-150906578c9e/NetFx20SP1_x86.exe"
   Push "http://www.filehippo.com/download/file/79d354d906da3aecff3ad6fff2efa58e721da18dadedaf683456bdd6759f1499/"
   Push 3
   Push "$TEMP\NetFx20SP1_x86.exe"

   Call DownloadFromRandomMirror
   DetailPrint "* Completed download of .NET Framework 2.0 SP1."
   Pop $0

!macroend
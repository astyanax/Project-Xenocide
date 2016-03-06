HwInfo::GetCpuSpeed

; $0 Now contains the CPU speed
StrCpy $R0 $0
 
HwInfo::GetSystemMemory
StrCpy $0
MessageBox MB_OK "You have $0MB of RAM"
 
; Here I want to have a message with two HW values read from the system
; so I must copy them each to a separate variable first.
HwInfo::GetVideoCardName
StrCpy $R0 $0
 
HwInfo::GetVideoCardMemory
StrCpy $R1 $0
 
MessageBox MB_OK "You have a $R0 Video Card with $R1MB of memory."
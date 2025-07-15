!define Name "HNode"
!define DevName "Happyrobot33"
Name "${Name}"
Outfile "${Name} setup.exe"
RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)
InstallDir "$ProgramFiles\${Name}"

!include LogicLib.nsh
!include MUI.nsh

Function .onInit
SetShellVarContext all
UserInfo::GetAccountType
pop $0
${If} $0 != "admin" ;Require admin rights on NT4+
    MessageBox mb_iconstop "Administrator rights required!"
    SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
    Quit
${EndIf}
FunctionEnd

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"


Section
SetOutPath "$INSTDIR"
WriteUninstaller "$INSTDIR\Uninstall.exe"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${DevName}"   "DisplayName" "${Name}"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${DevName}"   "UninstallString" "$INSTDIR\Uninstall.exe"
;Grab all the files
File /r "..\..\Builds\StandaloneWindows64\*"
CreateShortCut "$SMPROGRAMS\${Name}.lnk" "$INSTDIR\HNode.exe"
SectionEnd

Section "Uninstall"
;TODO: Delete your files
Delete "$SMPROGRAMS\${Name}.lnk"
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${DevName}"
Delete "$INSTDIR\Uninstall.exe"
RMDir /r "$INSTDIR"
SectionEnd

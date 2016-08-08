#####
# Installer for TreeStats


##### Globals
!define APPNAME "TreeStats"
!define GUID "{FBC6259D-86FF-4189-ABB9-E2E810E0AC22}"
!define SURROGATE "{71A69713-6593-47EC-0002-0000000DECA1}"
!define DLL "TreeStats.dll"


##### Settings
OutFile "Install.exe"
InstallDir "$PROGRAMFILES\${APPNAME}"
 

##### Pages
page directory
page instfiles


##### Installer section
# default section start
Section
 
SetOutPath $INSTDIR
File "bin\Release\${DLL}"
WriteUninstaller $INSTDIR\Uninstall.exe
 
# Registry
WriteRegStr HKLM "SOFTWARE\Decal\Plugins\${GUID}" "" "${APPNAME}"
WriteRegStr HKLM "SOFTWARE\Decal\Plugins\${GUID}" "Assembly" "${DLL}"
WriteRegDWORD HKLM "SOFTWARE\Decal\Plugins\${GUID}" "Enabled" 0x01
WriteRegStr HKLM "SOFTWARE\Decal\Plugins\${GUID}" "Object" "${APPNAME}"
WriteRegStr HKLM "SOFTWARE\Decal\Plugins\${GUID}" "Path" "$INSTDIR"
WriteRegStr HKLM "SOFTWARE\Decal\Plugins\${GUID}" "Surrogate" "${SURROGATE}"

SectionEnd


##### Uninstaller section
Section "un.Uninstall"
 
delete $INSTDIR\Uninstall.exe
delete "$INSTDIR\${DLL}"
rmDir $INSTDIR
DeleteRegKey HKLM "SOFTWARE\Decal\Plugins\${GUID}"

SectionEnd
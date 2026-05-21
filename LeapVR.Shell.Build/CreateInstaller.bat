cd "%~dp0"
set installerProject=%~dp0..\LeapVR.Shell.Installer\
set installerCompiler=%~dp0..\_Tools\Inno_Setup_5\ISCC.exe
set installerOutputDir=%~dp0..\LeapVR.Shell.Installer\Setup
set installerBaseScript=%~dp0..\LeapVR.Shell.Installer\LeapVR_Shell_Setup.iss
set installerTempScript=%~dp0..\LeapVR.Shell.Installer\LeapVR_Shell_Setup_Temp.iss
set installerSourceFilesDir=%~dp0
@echo off

:: Fetch param1
set "version=%~1"
goto :versionCheck
:versionPrompt
set /p "version=Enter Version: "
:versionCheck
if "%version%"=="" goto :versionPrompt

::Fetch param2
set "outputname=%2"
goto :nameCheck
:namePrompt
set /p "outputname=Enter OutputName: "
:nameCheck
if "%outputname%"=="" goto :namePrompt

cd %installerProject%
set installerOutputScript=LeapVR_%outputname%_%version%.iss

del /F /Q %installerTempScript%
del /F /Q %installerProject%%installerOutputScript%
del /F /Q %installerOutputDir%\Shell_Setup_%version%.exe

REM set command=-Command "(gc %installerBaseScript%) -replace '#define MyAppVersion \"UNKNOWN_VERSION\"', '#define MyAppVersion \"%version%\"' | Out-File -Encoding "UTF8" %installerProject%%installerOutputScript%"
REM echo %command%
powershell -Command "(gc %installerBaseScript%) -replace '#define MyAppVersion \"UNKNOWN_VERSION\"', '#define MyAppVersion \"%version%\"' | Out-File -Encoding "UTF8" %installerTempScript%"
powershell -Command "(gc %installerTempScript%) -replace 'SHELL_SOURCE_DIR', '%installerSourceFilesDir%' | Out-File -Encoding "UTF8" %installerTempScript%"
powershell -Command "(gc %installerTempScript%) -replace 'INSTALLER_SOURCE_DIR', '%installerProject%' | Out-File -Encoding "UTF8" %installerProject%%installerOutputScript%"

del %installerTempScript%
%installerCompiler% /O"%installerOutputDir%" /S"MsSign=signtool.exe sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a $p" /F"%outputname%_%version%" "%installerProject%%installerOutputScript%"
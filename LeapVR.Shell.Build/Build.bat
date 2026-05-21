set contentCreatorBinFolder=%~dp0..\LeapVR.Content.Creator\bin\x64\Release_ShellClient
set shellBinFolder=%~dp0..\LeapVR.Shell\bin\x64\Release_ShellClient
set installDirFolder=%~dp0InstallDir
set mediaDirFolder=%~dp0Media
set obfuscationFolder=%~dp0Obfuscation_Input
set obfuscationOutputFolder=%~dp0Obfuscation_Output
set outputname=LeapPlay_Setup


IF EXIST "%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe" set msBuildExe=%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe
IF EXIST "%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe" set msBuildExe=%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe
echo Using: %msBuildExe%

REM Delete all Folders and Files
cd "%~dp0"
del Build_Release.log

RD /S /Q  %installDirFolder%
Mkdir %installDirFolder%

RD /S /Q %shellBinFolder%
Mkdir %shellBinFolder%

RD /S /Q %contentCreatorBinFolder%
Mkdir %contentCreatorBinFolder%

RD /S /Q %obfuscationFolder%
Mkdir %obfuscationFolder%

RD /S /Q %obfuscationOutputFolder%


REM Prepare to Build
cd /d %~dp0
call "%msBuildExe%"  %~dp0..\PoD.sln /p:Configuration=Release_ShellClient /p:Platform="x64" /l:FileLogger,Microsoft.Build.Engine;logfile=Build_Release.log

REM Get the Version
for /f %%i in ('LeapVR.Utilities.VersionInfo.exe %shellBinFolder%\LeapPlay.Shell.exe') do set version=%%i
echo The directory is %RESULT%


REM Copy Shell and Creator for Obfuscation
xcopy %contentCreatorBinFolder% %obfuscationFolder% /E /I /Y /Q
xcopy %shellBinFolder% %obfuscationFolder% /E /I /Y /Q
del /F /Q %obfuscationFolder%\NLog.config
del /F /Q %obfuscationFolder%\libgrpc_csharp_ext.x64.dylib
del /F /Q %obfuscationFolder%\libgrpc_csharp_ext.x64.so
del /F /Q %obfuscationFolder%\libgrpc_csharp_ext.x86.dylib
del /F /Q %obfuscationFolder%\libgrpc_csharp_ext.x86.so

REM Remove unused Languages Folders
RD /S /Q %obfuscationFolder%\de
RD /S /Q %obfuscationFolder%\es
RD /S /Q %obfuscationFolder%\fr
RD /S /Q %obfuscationFolder%\it
RD /S /Q %obfuscationFolder%\pl
RD /S /Q %obfuscationFolder%\ru

REM Obfuscation
call "%~dp0Obfuscate.bat"
@echo on

REM Copy Obfuscated Files to Install Folder
xcopy %obfuscationOutputFolder% %installDirFolder% /E /I /Y /Q

REM Delete Obfuscation Folders
RD /S /Q %obfuscationFolder%
RD /S /Q %obfuscationOutputFolder%

REM Copy Media Files
xcopy %mediaDirFolder% %installDirFolder%\Media /E /I /Y /Q

REM Copy License Agreements to Install Folder
xcopy %~dp0License\* %installDirFolder%\License /E /I /Y /Q

REM CleanUp Directory
RD /S /Q %~dp0bin
RD /S /Q %~dp0obj
RD /S /Q %~dp0Properties
del symbols_%version%.map

REM Rename Symbols Map and Build Log
ren symbols.map symbols_%version%.map
ren Build_Release.log Build_Release_%version%.log

REM Code Signing
call SignFiles.bat %installDirFolder%

REM Create Installer
call CreateInstaller.bat %version% %outputname%
REM Finished, Press any Key to close
Pause
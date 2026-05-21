@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM  Build_Free.bat
REM  Builds Server (Pod.Web.Center) + Client (LeapVR.Shell +
REM  LeapVR.Content.Creator) + unsigned Inno Setup installer.
REM
REM  No obfuscation, no code-signing.
REM  Anchored on its own location (%~dp0), so it works from any
REM  repo path as long as the repo structure is intact.
REM ============================================================

REM ---- Self-locate ----
set "BUILD_DIR=%~dp0"
pushd "%BUILD_DIR%.."
set "REPO_ROOT=%CD%"
popd

REM Alias paths that contain "(x86)" - parens break FOR loops below.
set "PFX86=%ProgramFiles(x86)%"
set "TOOLS_DIR=%REPO_ROOT%\_Tools"
set "INSTALLER_DIR=%REPO_ROOT%\LeapVR.Shell.Installer"
set "SHELL_BIN=%REPO_ROOT%\LeapVR.Shell\bin\x64\Release_ShellClient"
set "CONTENT_BIN=%REPO_ROOT%\LeapVR.Content.Creator\bin\x64\Release_ShellClient"
set "INSTALL_DIR=%BUILD_DIR%InstallDir"
set "OUTPUT_NAME=LeapPlay_Setup"
set "FFMPEG_BIN=%REPO_ROOT%\LeapVR.Shell.3rdParty\bin\ffmpeg-4.0.2-win64-shared\bin"

echo ============================================================
echo  PoD Build_Free   (no obfuscation, no signing)
echo  Repo root      : %REPO_ROOT%
echo ============================================================

REM ---- 0. Prerequisite checks ----
echo.
echo === [0/5] Checking prerequisites ===

REM --- vswhere / MSBuild ---
set "VSWHERE=%PFX86%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [ERROR] vswhere.exe not found. Install Visual Studio 2017+ Build Tools:
    echo         https://aka.ms/vs/buildtools
    exit /b 1
)
"%VSWHERE%" -latest -prerelease -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" > "%TEMP%\_pod_msbuild.txt"
set /p MSBUILD=<"%TEMP%\_pod_msbuild.txt"
del "%TEMP%\_pod_msbuild.txt"
if not defined MSBUILD (
    echo [ERROR] MSBuild not found via vswhere.
    exit /b 1
)
if not exist "%MSBUILD%" (
    echo [ERROR] MSBuild path from vswhere does not exist: %MSBUILD%
    exit /b 1
)
echo MSBuild         : %MSBUILD%

REM --- dotnet + .NET Core 2.1 SDK ---
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet CLI not on PATH. Install .NET SDK from
    echo         https://dotnet.microsoft.com/download
    exit /b 1
)
dotnet --list-sdks | findstr /b /c:"2.1." >nul
if errorlevel 1 (
    echo [ERROR] .NET Core 2.1 SDK not installed - required by Pod.Web.Center.
    echo         Download dotnet-sdk-2.1.818-win-x64.exe from
    echo         https://dotnet.microsoft.com/download/dotnet/2.1
    exit /b 1
)
echo .NET 2.1 SDK    : OK

REM --- global.json pinning the server SDK to 2.1 ---
if not exist "%REPO_ROOT%\global.json" (
    echo Creating %REPO_ROOT%\global.json pinning SDK to 2.1.818
    > "%REPO_ROOT%\global.json" echo {
    >>"%REPO_ROOT%\global.json" echo   "sdk": {
    >>"%REPO_ROOT%\global.json" echo     "version": "2.1.818",
    >>"%REPO_ROOT%\global.json" echo     "rollForward": "disable"
    >>"%REPO_ROOT%\global.json" echo   }
    >>"%REPO_ROOT%\global.json" echo }
)

REM --- nuget.exe ---
if not exist "%TOOLS_DIR%\nuget.exe" (
    echo [ERROR] nuget.exe missing at %TOOLS_DIR%\nuget.exe
    echo         Download from https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    exit /b 1
)
echo nuget.exe       : OK

REM --- Inno Setup ---
if not exist "%TOOLS_DIR%\Inno_Setup_5\ISCC.exe" (
    echo [ERROR] Inno Setup 5 compiler not found at %TOOLS_DIR%\Inno_Setup_5\ISCC.exe
    exit /b 1
)
echo Inno Setup      : OK

REM --- FFmpeg native binaries ---
if not exist "%FFMPEG_BIN%\avcodec-58.dll" (
    echo [ERROR] FFmpeg 4.0.2 native DLLs missing at:
    echo         %FFMPEG_BIN%
    echo         Drop avcodec-58.dll, avdevice-58.dll, avfilter-7.dll, avformat-58.dll,
    echo         avutil-56.dll, postproc-55.dll, swresample-3.dll, swscale-5.dll there.
    exit /b 1
)
echo FFmpeg natives  : OK

REM --- Latest installed Windows 10 SDK (for the C++ XInputInterface project) ---
REM    Dump dir listing to a temp file first - avoids parens-in-path issues
REM    when the FOR command would be inline-parsed at script load time.
set "WINSDK_VERSION="
dir /b /a:d "%PFX86%\Windows Kits\10\Include" 2>nul | findstr /r "^10\." > "%TEMP%\_pod_winsdk.txt"
REM Last line wins => highest sorted SDK version.
for /f "delims=" %%v in (%TEMP%\_pod_winsdk.txt) do set "WINSDK_VERSION=%%v"
del "%TEMP%\_pod_winsdk.txt"
if not defined WINSDK_VERSION (
    echo [ERROR] No Windows 10 SDK found under "%PFX86%\Windows Kits\10\Include"
    exit /b 1
)
echo Windows SDK     : %WINSDK_VERSION%

REM ============================================================
REM  1. Build server
REM ============================================================
echo.
echo === [1/5] Building Server (Pod.Web.Center) ===
pushd "%REPO_ROOT%"
dotnet build "Pod.Web.Center\Pod.Web.Center.csproj" -c Release
if errorlevel 1 ( popd & echo [ERROR] Server build failed. & exit /b 1 )
popd

REM ============================================================
REM  2. NuGet restore for client (.NET Framework projects)
REM ============================================================
echo.
echo === [2/5] Restoring NuGet packages (client solution) ===
"%TOOLS_DIR%\nuget.exe" restore "%REPO_ROOT%\PoD.sln" -Verbosity quiet
if errorlevel 1 ( echo [ERROR] NuGet restore failed. & exit /b 1 )

REM ============================================================
REM  3. Build native XInputInterface.dll (vcxproj)
REM     Original .vcxproj pins v140 / Win10 SDK 10.0.17134.0 -
REM     override to whatever modern toolset/SDK is installed.
REM ============================================================
echo.
echo === [3/5] Building native XInputInterface.dll ===
"%MSBUILD%" "%REPO_ROOT%\LeapVR.Shell.3rdParty\XInputDotNet\XInputInterface\XInputInterface.vcxproj" ^
    -p:Configuration=Release -p:Platform=x64 ^
    -p:PlatformToolset=v143 -p:WindowsTargetPlatformVersion=%WINSDK_VERSION% ^
    -nologo -v:minimal
if errorlevel 1 ( echo [ERROR] XInputInterface build failed. & exit /b 1 )

REM ============================================================
REM  4. Build client (Shell + Content Creator)
REM ============================================================
echo.
echo === [4/5] Building Client (Release_ShellClient ^| x64) ===
if exist "%SHELL_BIN%" rmdir /s /q "%SHELL_BIN%"
if exist "%CONTENT_BIN%" rmdir /s /q "%CONTENT_BIN%"

REM Expose SolutionDir to MSBuild so LeapVR.Shell's PostBuildEvent target that
REM copies the vr_desktop / vrlounge_desktop assets resolves correctly even
REM when we build the .csproj directly (it would be set automatically if we
REM built .sln).
set "SolutionDir=%REPO_ROOT%\"

REM LeapVR.Shell.csproj is SDK-style (Microsoft.NET.Sdk.WindowsDesktop) and
REM resolves through the .NET 10 SDK toolchain, which requires MSBuild 18.
REM VS 2022 17.14 ships MSBuild 17.14; use `dotnet build` to pick up the
REM bundled MSBuild 18 from the global.json-pinned .NET 10 SDK. (When VS
REM 2022 17.15+ becomes the baseline this can flip back to %MSBUILD%.)
REM LeapVR.Shell.Setup.csproj is referenced as a ProjectReference from
REM LeapVR.Shell, so it builds in the same dotnet invocation.
dotnet build "%REPO_ROOT%\LeapVR.Shell\LeapVR.Shell.csproj" ^
    -c Release_ShellClient -p:Platform=x64 -v:minimal --nologo
if errorlevel 1 ( echo [ERROR] LeapVR.Shell build failed. & exit /b 1 )

REM LeapVR.Content.Creator.csproj is legacy-style itself but transitively
REM references SDK-style projects (LeapVR.Shared.Lib, etc.), so the build has
REM to flow through `dotnet build` for the same MSBuild-18 reason as Shell.
dotnet build "%REPO_ROOT%\LeapVR.Content.Creator\LeapVR.Content.Creator.csproj" ^
    -c Release_ShellClient -p:Platform=x64 -v:minimal --nologo
if errorlevel 1 ( echo [ERROR] LeapVR.Content.Creator build failed. & exit /b 1 )

REM ============================================================
REM  5. Build installer (replicates Build.bat post-obfuscation +
REM     CreateInstaller.bat, with signing disabled)
REM ============================================================
echo.
echo === [5/5] Building Installer ===

REM --- Determine version from built Shell.exe ---
"%BUILD_DIR%LeapVR.Utilities.VersionInfo.exe" "%SHELL_BIN%\LeapPlay.Shell.exe" > "%TEMP%\_pod_ver.txt"
set /p VERSION=<"%TEMP%\_pod_ver.txt"
del "%TEMP%\_pod_ver.txt"
if not defined VERSION ( echo [ERROR] Could not read version from LeapPlay.Shell.exe & exit /b 1 )
echo Version         : %VERSION%

REM --- Stage InstallDir (replaces obfuscation step) ---
if exist "%INSTALL_DIR%" rmdir /s /q "%INSTALL_DIR%"
mkdir "%INSTALL_DIR%"
xcopy "%CONTENT_BIN%\*" "%INSTALL_DIR%\" /E /I /Y /Q >nul
xcopy "%SHELL_BIN%\*"   "%INSTALL_DIR%\" /E /I /Y /Q >nul

REM --- Strip the same dev / cross-platform garbage Build.bat strips ---
del /F /Q "%INSTALL_DIR%\NLog.config" 2>nul
del /F /Q "%INSTALL_DIR%\libgrpc_csharp_ext.x64.dylib" 2>nul
del /F /Q "%INSTALL_DIR%\libgrpc_csharp_ext.x64.so"    2>nul
del /F /Q "%INSTALL_DIR%\libgrpc_csharp_ext.x86.dylib" 2>nul
del /F /Q "%INSTALL_DIR%\libgrpc_csharp_ext.x86.so"    2>nul
for %%L in (de es fr it pl ru) do if exist "%INSTALL_DIR%\%%L" rmdir /s /q "%INSTALL_DIR%\%%L"

REM --- Copy Media + License bundles into InstallDir ---
xcopy "%BUILD_DIR%Media"   "%INSTALL_DIR%\Media"   /E /I /Y /Q >nul
xcopy "%BUILD_DIR%License" "%INSTALL_DIR%\License" /E /I /Y /Q >nul

REM --- Generate the .iss with version + path substitutions
REM     and the SignTool=MsSign line commented out. ---
set "ISS_TEMPLATE=%INSTALLER_DIR%\LeapVR_Shell_Setup.iss"
set "ISS_TEMP=%INSTALLER_DIR%\LeapVR_LeapPlay_Setup_Temp.iss"
set "ISS_OUT=%INSTALLER_DIR%\LeapVR_LeapPlay_Setup_%VERSION%.iss"
if exist "%ISS_TEMP%" del /F /Q "%ISS_TEMP%"
if exist "%ISS_OUT%"  del /F /Q "%ISS_OUT%"
if exist "%INSTALLER_DIR%\Setup\%OUTPUT_NAME%_%VERSION%.exe" del /F /Q "%INSTALLER_DIR%\Setup\%OUTPUT_NAME%_%VERSION%.exe"

powershell -NoProfile -Command "(gc '%ISS_TEMPLATE%') -replace '#define MyAppVersion \"UNKNOWN_VERSION\"', '#define MyAppVersion \"%VERSION%\"' | Out-File -Encoding UTF8 '%ISS_TEMP%'"
powershell -NoProfile -Command "(gc '%ISS_TEMP%') -replace 'SHELL_SOURCE_DIR', '%BUILD_DIR%' | Out-File -Encoding UTF8 '%ISS_TEMP%'"
powershell -NoProfile -Command "(gc '%ISS_TEMP%') -replace 'INSTALLER_SOURCE_DIR', '%INSTALLER_DIR%\\' | Out-File -Encoding UTF8 '%ISS_TEMP%'"
powershell -NoProfile -Command "(gc '%ISS_TEMP%') -replace '^SignTool=MsSign \$f', '; SignTool=MsSign $f ; disabled by Build_Free.bat' | Out-File -Encoding UTF8 '%ISS_OUT%'"
del /F /Q "%ISS_TEMP%"

if not exist "%INSTALLER_DIR%\Setup" mkdir "%INSTALLER_DIR%\Setup"

"%TOOLS_DIR%\Inno_Setup_5\ISCC.exe" "/F%OUTPUT_NAME%_%VERSION%" "%ISS_OUT%"
if errorlevel 1 ( echo [ERROR] Inno Setup compile failed. & exit /b 1 )

echo.
echo ============================================================
echo  Build complete.
echo  Server    : %REPO_ROOT%\Pod.Web.Center\bin\Release\net10.0\Pod.Web.Center.dll
echo  Shell     : %SHELL_BIN%\LeapPlay.Shell.exe
echo  Creator   : %CONTENT_BIN%\LeapPlay.Content.Creator.exe
echo  Installer : %INSTALLER_DIR%\Setup\%OUTPUT_NAME%_%VERSION%.exe   (UNSIGNED)
echo ============================================================

endlocal
exit /b 0

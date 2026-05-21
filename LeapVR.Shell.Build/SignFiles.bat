set signtool=signtool.exe
set signToolParam=sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a 

:: Fetch param1
set "signfilesdir=%~1"
goto :directoryCheck
:directoryPrompt
set /p "signfilesdir=Enter Directory with files to sign: "
:directoryCheck
if "%signfilesdir%"=="" goto :directoryPrompt

REM EXE Signing
%signtool% %signToolParam% %signfilesdir%\LeapPlay.Shell.exe
%signtool% %signToolParam% %signfilesdir%\LeapPlay.Content.Creator.exe
%signtool% %signToolParam% %signfilesdir%\vr_desktop\vrlounge_desktop.exe

REM DLL Signing
%signtool% %signToolParam% %signfilesdir%\vr_desktop\vrlounge_desktop_Data\Plugins\DesktopDuplication.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Utilities.Windows.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Utilities.Steam.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Setup.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Services.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Repository.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Repository.Interfaces.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Modules.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Modules.Interfaces.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Managers.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Language.dll
%signtool% %signToolParam% %signfilesdir%\zh-CN\LeapVR.Shell.Language.resources.dll
%signtool% %signToolParam% %signfilesdir%\zh-CN\LeapVR.Shell.Categories.resources.dll
%signtool% %signToolParam% %signfilesdir%\en-US\LeapVR.Shell.Language.resources.dll
%signtool% %signToolParam% %signfilesdir%\en-US\LeapVR.Shell.Categories.resources.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Domain.Models.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Controllers.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shell.Categories.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shared.Lib.Wpf.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shared.Lib.Win.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Shared.Lib.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.OpenVR.Wrapper.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Content.Util.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Content.Shared.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Content.Creator.Logic.dll
%signtool% %signToolParam% %signfilesdir%\LeapVR.Content.Creator.Language.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Data.Infrastructure.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Enums.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Grpc.Base.Client.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Grpc.Base.Const.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Grpc.Messages.dll
%signtool% %signToolParam% %signfilesdir%\Pod.Grpc.Utilities.dll

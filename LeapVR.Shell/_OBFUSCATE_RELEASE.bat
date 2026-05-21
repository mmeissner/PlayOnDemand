@echo off

set confuserExe=..\_TOOLS\ConfuserEx_1.0.0.0\Confuser.CLI.exe
set confuserProjectFile=..\LeapVR.Shell.Build\LeapVR.Build.ConfuserEx.crproj
set sourceDir=bin\x64\Release_ShellClient
set sourceContentCreator=..\LeapVR.Content.Creator\bin\x64\Release_ShellClient
set obfuscateInput=..\LeapVR.Shell.Build\Obfuscation_Input
set obfuscatedOutput=..\LeapVR.Shell.Build\Obfuscation_Temp
set destDir=bin\x64\Release_OBFUSCATED
set symbolsFileName=symbols.map

color 87
echo [INFO ] Checking if Shell source directory exists...
IF NOT EXIST %sourceDir% (
    echo [ERROR] Source directory `%sourceDir%` does not exists. Cannot continue process.
    goto ERROR_EXIT
)

echo [INFO ] Checking if Content Creator source directory exists...
IF NOT EXIST %sourceContentCreator% (
    echo [ERROR] Source directory `%sourceContentCreator%` does not exists. Cannot continue process.
    goto ERROR_EXIT
)

echo [INFO ] Checking if destination directories exists...
IF EXIST %destDir% (
    echo [INFO ] Removing `%destDir%` directory...
    rd %destDir% /S /Q
    IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%destDir%` directory. Error code %errorlevel%.
		goto ERROR_EXIT
    )
)
echo [INFO ] Checking if output directories exists...
IF EXIST %obfuscatedOutput% (
    echo [INFO ] Removing `%obfuscatedOutput%` directory...
    rd %obfuscatedOutput% /S /Q
	IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%obfuscatedOutput%` directory. Error code %errorlevel%.
		goto ERROR_EXIT
    )
)

echo [INFO ] Checking if input directories exists...
IF EXIST %obfuscateInput% (
    echo [INFO ] Removing `%obfuscateInput%` directory...
    rd %obfuscateInput% /S /Q
	IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%obfuscateInput%` directory. Error code %errorlevel%.
		goto ERROR_EXIT
    )
)


echo [INFO ] Checking if `%symbolsFileName%` file exists...
IF EXIST %destDir%\..\%symbolsFileName% (
    echo [INFO ] Removing `%destDir%\..\%symbolsFileName%` file...
    del /F /Q %destDir%\..\%symbolsFileName%
    IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%destDir%\..\%symbolsFileName%` directory. Error code %errorlevel%.
		goto ERROR_EXIT
    )
)

echo [INFO ] Copy Content Creator Source to Obfuscation Dir
xcopy %sourceContentCreator% %obfuscateInput% /E /I /Y /Q

echo [INFO ] Copy Shell Source to Obfuscation Dir
xcopy %sourceDir% %obfuscateInput% /E /I /Y /Q

echo [INFO ] Applying obfuscation...
%confuserExe% -n %confuserProjectFile%

IF NOT EXIST %obfuscatedOutput% (
    echo [ERROR] Obfuscation has not created `%obfuscatedOutput%` directory. Has it failed?
	goto ERROR_EXIT
)


echo [INFO ] Recreating the output directory...
md %destDir%

echo [INFO ] Copying Unbfuscated files to destination directory...
xcopy %obfuscateInput% %destDir% /E /I /Y /Q

echo [INFO ] Copying obfuscated files to destination directory... (overwrite)
xcopy %obfuscatedOutput% %destDir% /E /I /Y /Q
IF %errorlevel% NEQ 0 (
    echo [ERROR] Copying of unobfuscated files from `%obfuscatedOutput%` to `%destDir%` failed. Error code %errorlevel%.
	goto ERROR_CLEANUP_EXIT
)


echo [INFO ] Removing `%obfuscatedOutput%` directory due to cleanup reasons...
rd %obfuscatedOutput% /S /Q
IF %errorlevel% NEQ 0 (
	echo [WARN ] Cannot remove `%obfuscatedOutput%` directory. Error code %errorlevel%.
)

echo [INFO ] Removing `%obfuscateInput%` directory due to cleanup reasons...
rd %obfuscatedOutput% /S /Q
IF %errorlevel% NEQ 0 (
	echo [WARN ] Cannot remove `%obfuscateInput%` directory. Error code %errorlevel%.
)

color 27
echo [INFO ] All done.
pause
color
exit /B 0

:ERROR_CLEANUP_EXIT
IF EXIST %destDir% (
	echo [INFO ] Removing `%destDir%` directory due to cleanup reasons...
	rd %destDir% /S /Q
	IF %errorlevel% NEQ 0 (
		echo [ERROR] Cannot remove `%destDir%` directory. Error code %errorlevel%.
	)
)

IF EXIST %obfuscatedDir% (
	echo [INFO ] Removing `%obfuscatedDir%` directory due to cleanup reasons...
	rd %obfuscatedDir% /S /Q
	IF %errorlevel% NEQ 0 (
		echo [ERROR] Cannot remove `%obfuscatedDir%` directory. Error code %errorlevel%.
	)
)
goto ERROR_EXIT

:ERROR_EXIT
color 47
echo [ERROR] Script exits due to error.
pause
color
exit /B -1
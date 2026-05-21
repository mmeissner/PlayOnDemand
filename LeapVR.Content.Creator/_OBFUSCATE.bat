@echo off

set confuserExe=..\_TOOLS\ConfuserEx_1.0.0.0\Confuser.CLI.exe
set confuserProjectFile=LeapVR.Content.Creator.ConfuserEx.crproj
set sourceDir=bin\Release
set obfuscatedDir=bin\_OBFUSCATED_OUT
set destDir=bin\_OBFUSCATED
set fileCntExpected=15
set symbolsFileName=symbols.map

color 87
echo [INFO ] Checking if source directory exists...
IF NOT EXIST %sourceDir% (
    echo [ERROR] Source directory `%sourceDir%` does not exists. Cannot continue process.
    goto ERROR_EXIT
)

echo [INFO ] Checking if output directories exists...
IF EXIST %destDir% (
    echo [INFO ] Removing `%destDir%` directory...
    rd %destDir% /S /Q
    IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%destDir%` directory. Error code %errorlevel%.
		goto ERROR_EXIT
    )
)
IF EXIST %obfuscatedDir% (
    echo [INFO ] Removing `%obfuscatedDir%` directory...
    rd %obfuscatedDir% /S /Q
	IF %errorlevel% NEQ 0 (
        echo [ERROR] Cannot remove `%obfuscatedDir%` directory. Error code %errorlevel%.
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

echo [INFO ] Applying obfuscation...
%confuserExe% -n %confuserProjectFile%

IF NOT EXIST %obfuscatedDir% (
    echo [ERROR] Obfuscation has not created `%obfuscatedDir%` directory. Has it failed?
	goto ERROR_EXIT
)

echo [INFO ] Checking obfuscation results...
for /f %%A in ('xcopy %obfuscatedDir% %destDir% /E /I /Y /L ^| find /v /c ""') do set fileCnt=%%A
set /A fileCnt=%fileCnt%-1
echo [INFO ] Obfuscation generated %fileCnt% file(s).
IF %fileCnt% NEQ %fileCntExpected% (
    echo [ERROR] Obfuscation provided not expected ammount of files - %fileCnt% provided, %fileCntExpected% expected. Has it failed?
    goto ERROR_CLEANUP_EXIT
)
echo [INFO ] Obfuscation test passed. Assuming files obfuscated correctly.

echo [INFO ] Recreating the output directory...
md %destDir%

echo [INFO ] Copying unobfuscated files to destination directory...
xcopy %sourceDir% %destDir% /E /I /Y /Q
IF %errorlevel% NEQ 0 (
    echo [ERROR] Copying of unobfuscated files from `%sourceDir%` to `%destDir%` failed. Error code %errorlevel%.
	goto ERROR_CLEANUP_EXIT
)

echo [INFO ] Copying obfuscated files to destination directory (overriding)...
xcopy %obfuscatedDir% %destDir% /E /I /Y /Q
IF %errorlevel% NEQ 0 (
    echo [ERROR] Copying of obfuscated files from `%obfuscatedDir%` to `%destDir%` failed. Error code %errorlevel%.
    goto ERROR_CLEANUP_EXIT
)

echo [INFO ] Removing `%symbolsFileName%` file from destination directory...
copy /B /Y %destDir%\%symbolsFileName% %destDir%\..\%symbolsFileName%
del /F /Q %destDir%\%symbolsFileName%
IF EXIST %destDir%\%symbolsFileName% (
    echo [ERROR] Removing of `%destDir%\%symbolsFileName%` file failed. Error code %errorlevel%.
	goto ERROR_CLEANUP_EXIT
)

echo [INFO ] Removing `*.pdb` file from destination directory...
del /F /Q %destDir%\*.pdb
IF %errorlevel% NEQ 0 (
    echo [ERROR] Removing of `%destDir%\*.pdb` file failed. Error code %errorlevel%.
	goto ERROR_CLEANUP_EXIT
)

echo [INFO ] Removing `%obfuscatedDir%` directory due to cleanup reasons...
rd %obfuscatedDir% /S /Q
IF %errorlevel% NEQ 0 (
	echo [WARN ] Cannot remove `%obfuscatedDir%` directory. Error code %errorlevel%.
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
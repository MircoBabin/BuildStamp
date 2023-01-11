@echo off
setlocal
cls

cd /D "%~dp0"


set sz_exe=C:\Program Files\7-Zip\7z.exe
if exist "%sz_exe%" goto build

set sz_exe=C:\Program Files (x86)\7-Zip\7z.exe
if exist "%sz_exe%" goto build

echo !!! 7-Zip 18.06 - 7z.exe not found
pause
goto:eof

:build 
echo 7-Zip 18.06: %sz_exe%

del Release_version.txt >nul 2>&1

"%~dp0..\bin\Release\BuildStamp.exe" output-version --outputfilename "%~dp0Release_version.txt"
if errorlevel 1 goto :error_version
set /p BuildStampReleaseVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

"%~dp0..\bin\Debug\BuildStamp.exe" output-version --outputfilename "%~dp0Release_version.txt"
if errorlevel 1 goto :error_version
set /p BuildStampDebugVersion=< Release_version.txt
del Release_version.txt >nul 2>&1

if "%BuildStampReleaseVersion%" == "%BuildStampDebugVersion%" goto build_zip
echo.
echo Release version: %BuildStampReleaseVersion%
echo Debug version..: %BuildStampDebugVersion%
echo.
echo !!! Versions do not match.
pause
goto :eof

:append_filename
    set filenames=%filenames% "%~dp0..\bin\Release\%~1"
goto:eof

:build_zip
echo.
echo Release version: %BuildStampReleaseVersion%
echo.
echo.


del /q "Release\*" >nul 2>&1

set filenames=
rem BuildStamp.exe filenames
del Release_filenames.txt >nul 2>&1
"%~dp0..\bin\Release\BuildStamp.exe" output-installationFilenames --outputfilename "%~dp0Release_filenames.txt"
if errorlevel 1 goto error_installationfilenames
for /f "tokens=* delims=" %%a in (Release_filenames.txt) do call :append_filename "%%a"
del Release_filenames.txt >nul 2>&1

"%sz_exe%" a -tzip -mx7 "Release\BuildStamp-%BuildStampReleaseVersion%.zip" %filenames%
"%sz_exe%" a -tzip -mx7 "Release\BuildStamp-%BuildStampReleaseVersion%-debugpack.zip" "%~dp0..\bin"

echo.
echo.
echo Created "Release\BuildStamp-%BuildStampReleaseVersion%.zip"
echo Created "Release\BuildStamp-%BuildStampReleaseVersion%-debugpack.zip" 

rem https://github.com/MircoBabin/BuildStamp/releases/latest/download/release.download.zip.url-location
rem Don't output trailing newline (CRLF)
<NUL >"Release\release.download.zip.url-location" set /p="https://github.com/MircoBabin/BuildStamp/releases/download/%BuildStampReleaseVersion%/BuildStamp-%BuildStampReleaseVersion%.zip"

echo.
echo Created "Release\release.download.zip.url-location" 
echo.

pause
goto :eof

:error_version
echo.
echo !!! Error retrieving version

pause
goto :eof

:error_installationfilenames
echo.
echo !!! Error retrieving installation filenames

pause
goto :eof

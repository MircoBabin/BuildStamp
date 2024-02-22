@echo off
    setlocal
    set "BuildStampTemplateLanguage=pascal"
    set "BuildStampVersionFilename1=%~dp0Version1.pas"
    set "BuildStampVersionFilename2=%~dp0Version2.pas"
    
    set "BuildStampTemplateFilename=%~dp0VersionInfo.Pascal.rc"
    
    call "%~dp0Utils\Utils.test-versioninfo.bat" "%~f0"
    if errorlevel 1 exit /b 1
    
    rem Done    
    exit /b 0

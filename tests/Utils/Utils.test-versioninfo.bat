@echo off
    rem set "BuildStampTemplateLanguage=pascal"
    rem set "BuildStampVersionFilename1=%~dp0Version1.pas"
    rem set "BuildStampVersionFilename1=%~dp0Version2.pas"
    rem set "BuildStampTemplateFilename=%~dp0VersionInfo.Pascal.rc"
    rem call "%~dp0Utils\Utils.test-versioninfo.bat" "%~f0"

    call "%~dp0Utils.test-setup.bat" "%1"

    rem --outputfilename
    copy /y "%BuildStampTemplateFilename%" "%BuildStampExe_TestOutput%.input" >nul 2>&1
    set "BuildStampExe_TestExpected=%BuildStampExe_TestExpected1%"
    del /q "%BuildStampExe_TestOutput%.stamped" >nul 2>&1
    "%BuildStampExe%" stamp-versioninfo --versionfilename "%BuildStampVersionFilename1%" --language %BuildStampTemplateLanguage% --filename "%BuildStampExe_TestOutput%.input" --outputfilename "%BuildStampExe_TestOutput%.stamped" > "%BuildStampExe_TestOutput%.output" 2>&1
    if errorlevel 1 goto failed
    if not exist "%BuildStampExe_TestOutput%.stamped" goto failed1
    
    set "BuildStampExe_TestOutput_Saved=%BuildStampExe_TestOutput%"
    set "BuildStampExe_TestOutput=%BuildStampExe_TestOutput%.stamped"
    call "%~dp0Utils.diff.bat"
    if errorlevel 1 exit /b 1
    set "BuildStampExe_TestOutput=%BuildStampExe_TestOutput_Saved%"
    
    rem BUILDSTAMP:BEGINSTAMP as template
    rem BUILDSTAMP:ENDSTAMP
    set "BuildStampExe_TestExpected=%BuildStampExe_TestExpected1%"
    "%BuildStampExe%" stamp-versioninfo --versionfilename "%BuildStampVersionFilename1%" --language %BuildStampTemplateLanguage% --filename "%BuildStampExe_TestOutput%" > "%BuildStampExe_TestOutput%.output" 2>&1
    if errorlevel 1 goto failed
    call "%~dp0Utils.diff.bat"
    if errorlevel 1 exit /b 1

    rem BUILDSTAMP:BEGINSTAMP replaced with BUILDSTAMP:BEGINTEMPLATE and BUILDSTAMP:ENDTEMPLATE
    rem BUILDSTAMP:ENDSTAMP
    set "BuildStampExe_TestExpected=%BuildStampExe_TestExpected2%"
    "%BuildStampExe%" stamp-versioninfo --versionfilename "%BuildStampVersionFilename2%" --language %BuildStampTemplateLanguage% --filename "%BuildStampExe_TestOutput%" > "%BuildStampExe_TestOutput%.output" 2>&1
    if errorlevel 1 goto failed
    call "%~dp0Utils.diff.bat"
    if errorlevel 1 exit /b 1
    
    exit /b 0

:failed    
    echo BuildStamp.exe failed
    type "%BuildStampExe_TestOutput%.output"
    exit /b 1    

:failed1
    echo Outputfile not written
    exit /b 1    
    
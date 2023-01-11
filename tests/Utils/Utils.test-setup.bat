@echo off
    set "BuildStampExe=%~dp0..\..\bin\Debug\BuildStamp.exe"
    set "BuildStampExeCompiletime1=--datetime 1975-09-12T23:30:00+02:00"
    set "BuildStampExeCompiletime2=--datetime 2023-01-11T19:19:19Z"
    
    if "%BuildStampTemplateFilename%" == "" goto :eof
    if not exist "%BuildStampTemplateFilename%" goto :eof
    
    call :SetTestBaseName "%BuildStampTemplateFilename%"

    set "BuildStampExe_TestOutput=%tmp%\BuildStamp.%BuildStampExe_TestBaseName%"
    set "BuildStampExe_TestExpected1=%~dp0..\Expectations\%BuildStampExe_TestBaseName%.expected1"
    set "BuildStampExe_TestExpected2=%~dp0..\Expectations\%BuildStampExe_TestBaseName%.expected2"
    
    copy /y "%BuildStampTemplateFilename%" "%BuildStampExe_TestOutput%" >nul 2>&1
    goto :eof
    
:SetTestBaseName
    set "BuildStampExe_TestBaseName=%~nx1"
    goto :eof    

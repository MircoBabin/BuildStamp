@echo off
    if not exist "%BuildStampExe_TestOutput%" echo Error: Output file does not exist %BuildStampExe_TestOutput%
    if not exist "%BuildStampExe_TestExpected%" echo Error: Expected file does not exist %BuildStampExe_TestExpected%
    
:diffEqual
    call :errorlevel_99
    fc /b "%BuildStampExe_TestOutput%" "%BuildStampExe_TestExpected%" >nul 2>&1
    if errorlevel 1 goto errorMustBeEqual
    
    exit /b 0

:errorMustBeEqual
    echo Error: output %BuildStampExe_TestOutput% does not match expectation %BuildStampExe_TestExpected%
    exit /b 1

:errorlevel_99
    exit /b 99    

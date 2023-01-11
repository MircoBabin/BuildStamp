@echo off
    setlocal
    set "BuildStampTemplateLanguage=pascal"

    rem Source with UTF-8 BOM (modern IDE like Delphi Alexandria)    
    set "BuildStampTemplateFilename=%~dp0Compiled.pas"
    
    call "%~dp0Utils\Utils.test-language.bat" "%~f0"
    if errorlevel 1 exit /b 1
    
    rem Source without UTF-8 BOM (via e.g. NotePad++)
    set "BuildStampTemplateFilename=%~dp0CompiledNoBom.pas"
    
    call "%~dp0Utils\Utils.test-language.bat" "%~f0"
    if errorlevel 1 exit /b 1

    rem Done    
    exit /b 0

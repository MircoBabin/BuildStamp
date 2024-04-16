[![Github All Releases](https://img.shields.io/github/downloads/MircoBabin/BuildStamp/total)](https://github.com/MircoBabin/BuildStamp/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/MircoBabin/BuildStamp/blob/master/LICENSE.md)

# BuildStamp
BuildStamp is a compilation tool. Use BuildStamp in the Pre-build event to inject compilation date/time and adjust the versionumber in the VersionInfo resource file.

- It stamps the compilation date/time into a source file
- It updates the VersionInfo.rc resource file with a versionumber listed in a sourcefile.
- It can also digitally sign any executable. The codesign certificate can be on disk or in [KeePass](https://keepass.info/ "KeePass").

# Download binary
For Windows (.NET framework 4), [the latest version can be found here](https://github.com/MircoBabin/BuildStamp/releases/latest "Latest Version").

Download the zip and unpack it somewhere.
*For digitally signing using certificates stored in [KeePass](https://keepass.info/ "KeePass") also install [KeePassCommander](https://github.com/MircoBabin/KeePassCommander "KeePassCommander").*

The minimum .NET framework required is 4.0.

*For unattended automatic installation scripts, read the section "Automatic installation scripts" lower down the page.*

# Help

Execute **BuildStamp.exe** without parameters to view the help.

```
BuildStamp version 2.3
https://github.com/MircoBabin/BuildStamp - MIT license

BuildStamp is a compilation tool.
It stamps the compilation date/time into a source file, using the Pre-build event.
It can also digitally sign any executable. The codesign certificate can be on disk or in KeePass.



----------------------------------------------------
----------------------------------------------------
---                                              ---
--- Stamp compilation date/time into source file ---
---                                              ---
----------------------------------------------------
----------------------------------------------------
Syntax: BuildStamp.exe stamp --filename <source-filename> --language <language>
                             {--outputfilename <output-filename>}
                             {--datetime yyyy-mm-ddThh:mm:ss+HH:MM}
                             {--launchdebugger}
- With --language the programming language is specified.
  Supported languages: "CSharp", "Pascal".
- When --outputfilename is ommitted, the <source-filename> will be overwritten.
- With --datetime <ISO 8601> (e.g. "1975-09-12T23:30:00+02:00") the 'current time' can be provided.
  When ommitted the current time from the system clock will be used.
- When the debug switch --launchdebugger is encountered, a request to launch the debugger is started.

<source-filename> has to contain:
// <BUILDSTAMP:BEGINSTAMP>
    Inside <BUILDSTAMP:COMPILEDATE> is replaced with yyyy-mm-dd in local time.
    Inside <BUILDSTAMP:COMPILETIME> is replaced with hh:mm:ss in local time.
    Inside <BUILDSTAMP:COMPILEDATETIME> is replaced with a full ISO-8601 time.
    Inside <BUILDSTAMP:COMPILEDATE-UTC> is replaced with yyyy-mm-dd in UTC time.
    Inside <BUILDSTAMP:COMPILETIME-UTC> is replaced with hh:mm:ss in UTC time.
    Inside <BUILDSTAMP:COMPILEDATETIME-UTC> is replaced with a full ISO-8601 time with timezone Z (UTC).
// <BUILDSTAMP:ENDSTAMP>

e.g. for Pascal source: BuildStamp.exe stamp --filename c:\...\Compiled.pas --language pascal
unit Compiled;
interface
// <BUILDSTAMP:BEGINSTAMP>
const COMPILEDATE = '<BUILDSTAMP:COMPILEDATE>';
const COMPILETIME = '<BUILDSTAMP:COMPILETIME>';
// <BUILDSTAMP:ENDSTAMP>
implementation
end.

It is recommended for the <source-filename> to only contain BuildStamp metadata.
And no other metadata like versionnumber, buildnumber, copyright, etc.
Because adding other metadata does not play well with version control (Git).
----------------------------------------------------
----------------------------------------------------
---                                              ---
--- Stamp VersionInfo resource file (.rc)        ---
---                                              ---
----------------------------------------------------
----------------------------------------------------
Syntax: BuildStamp.exe stamp-versioninfo --versionfilename <versionSource-filename> --language <language>
                             --filename <ResourceCompiler-filename.rc>
                             {--outputfilename <output-filename>}
                             {--launchdebugger}
- With --language the programming language of <versionSource-filename> is specified.
- When --outputfilename is ommitted, the <ResourceCompiler-filename.rc> will be overwritten.
- When the debug switch --launchdebugger is encountered, a request to launch the debugger is started.

<versionSource-filename> has to contain the version string between 2 markers. E.g. for Pascal source:
unit Version;
interface
const VersionString =
    {<BUILDSTAMP:BEGINVERSION>}
    '4.076'
    {<BUILDSTAMP:ENDVERSION>};
implementation
end.

<ResourceCompiler-filename.rc> has to contain the version number.
// <BUILDSTAMP:BEGINSTAMP>
    Inside <BUILDSTAMP:VERSION_4PARTS_COMMA_SEPARATED> is replaced with "major,minor,patch,build".
    Inside <BUILDSTAMP:VERSION_4PARTS_POINT_SEPARATED> is replaced with "major.minor.patch.build".
    Inside <BUILDSTAMP:VERSION_FULL> is replaced with the full version inside <versionSource-filename>.
// <BUILDSTAMP:ENDSTAMP>

e.g. for Pascal resource file: BuildStamp.exe stamp-versioninfo --versionfilename c:\...\Version.pas --language pascal --filename c:\...\VersionInfo.rc
VersionInfo.rc contents:
// <BUILDSTAMP:BEGINSTAMP>
1 VERSIONINFO
FILEVERSION <BUILDSTAMP:VERSION_4PARTS_COMMA_SEPARATED>
PRODUCTVERSION <BUILDSTAMP:VERSION_4PARTS_COMMA_SEPARATED>
FILEOS 0x4
FILETYPE 0x1
{
BLOCK "StringFileInfo"
{
    BLOCK "040904E4"
    {
        VALUE L"CompanyName", L"My Company\000"
        VALUE L"FileDescription", L"My Program\000"
        VALUE L"FileVersion", L"<BUILDSTAMP:VERSION_4PARTS_POINT_SEPARATED>>\000"
        VALUE L"LegalCopyright", L"(c) My Company\000"
        VALUE L"ProductName", L"My Program\000"
        VALUE L"ProductVersion", L"<BUILDSTAMP:VERSION_FULL>\000"
        VALUE L"ProgramID", L"MyProgram\000" // Delphi specific. Must be the projectname, e.g. MyProgram.dproj
    }
}
BLOCK "VarFileInfo"
{
    VALUE "Translation", 0x0409 0x04E4
}
}
// <BUILDSTAMP:ENDSTAMP>



---------------------------------
---------------------------------
---                           ---
--- Digitally sign executable ---
---                           ---
---------------------------------
---------------------------------
Syntax: BuildStamp.exe sign --filename <filename.exe>
                            {--certificate <code-signing-certificate.pfx>}
                            {--certificate-password <password for code-signing-certificate.pfx>}
                            {--keepasscommander-path <path>} like c:\KeePass\Plugins
                            {--keepass-certificate-title <title>} like "My Code Signing Certificate"
                            {--keepass-certificate-attachment <attachmentname>} like "certificate.p12"
                            {--keepass-certificate-password <fieldname>} like "Certificate Password". When omitted the default password field is used.
                            {--sign-with-authenticode-timestamp-url <url>} like http://timestamp.digicert.com
                            {--sign-with-sha256-rfc3161-timestamp-url <url>} like http://timestamp.digicert.com
                            {--launchdebugger}

Digitally signs <filename.exe>.

With --certificate and --certificate-password the certificate is read from a file on disk.
With --keepasscommander-path and --keepass-certificate-... the certificate is retrieved from the KeePass password store. The KeePass plugin KeepassCommander https://github.com/MircoBabin/KeePassCommander is used for retrieval.
Attention: --keepass-certificate-attachment references an attachmentname in the KeePass entry with title <--keepass-certificate-title>.
Attention: --keepass-certificate-password references a fieldname in the KeePass entry with title <--keepass-certificate-title>, and must not provide the real password.



---------------
---------------
---         ---
--- License ---
---         ---
---------------
---------------
BuildStamp
MIT license

Copyright (c) 2023 Mirco Babin

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

```

# Documentation

- [Delphi documentation](docs/Delphi/README.md "Delphi documentation")
- [Digitally sign documentation](docs/DigitallySign.md "Digitally sign documentation")

# Why
Delphi doesn't have a good way to embed the compilation date/time into the executable. There is a solution reading the linker timestamp from the PE Header of the executable. But that PE Header solution is too complex and low-level in my opinion. 
Delphi also lacks updating the Version Info resource in a usable way, when the versionnumber is already present in a sourcefile.

So I wrote BuildStamp for injecting the compilation date/time and adjusting the Version Info via Pre-build event. The modified source file is then compiled into the executable.

The Microsoft provided [signtool.exe](https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool "signtool.exe") can only be officially installed with the "Microsoft Windows Software Development Kit (SDK)". 
I wanted a standalone executable that can be distributed standalone. And I wanted the ability to store the code signing certificate in [KeePass](https://keepass.info/ "KeePass"). That's why I added the "sign" command.

# Automatic installation scripts
For unattended installation scripts the following flow can be used for the latest version:

1) Download https://github.com/MircoBabin/BuildStamp/releases/latest/download/release.download.zip.url-location
2) Read the text of this file into **latest-download-url**. The file only contains an url, so the encoding is ASCII. *The encoding UTF-8 may also be used to read the file, because ASCII is UTF-8 encoding.*
3) Download the zip from the **latest-download-url** to local file **BuildStamp.zip**. *Each release carries the version number in the filename. To prevent not knowing the downloaded filename, download to a fixed local filename.*
4) Unpack the downloaded **BuildStamp.zip**.

# Contributions
Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md "contributing") before making any contribution!

# License
[The license is MIT.](LICENSE.md "license")






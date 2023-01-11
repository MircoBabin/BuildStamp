[![Github All Releases](https://img.shields.io/github/downloads/MircoBabin/BuildStamp/total)](https://github.com/MircoBabin/BuildStamp/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/MircoBabin/BuildStamp/blob/master/LICENSE.md)

# BuildStamp
BuildStamp is a compilation tool. It stamps the compilation date/time into a source file. Use BuildStamp in the Pre-build event to inject compilation date/time.

# Download binary
For Windows (.NET framework 4), [the latest version can be found here](https://github.com/MircoBabin/BuildStamp/releases/latest "Latest Version").

Download the zip and unpack it somewhere.

The minimum .NET framework required is 4.0.

*For unattended automatic installation scripts, read the section "Automatic installation scripts" lower down the page.*

# Help

Execute **BuildStamp.exe** without parameters to view the help.

```
BuildStamp version 1.0
https://github.com/MircoBabin/BuildStamp - MIT license

BuildStamp is a compilation tool. It stamps the compilation date/time into a source file.
Use BuildStamp in the Pre-build event to inject compilation date/time.

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
Commit the initial <source-filename> and afterwards .gitignore it.

```

# Documentation

- [Delphi documentation](docs/Delphi/README.md "Delphi documentation")

# Why
Delphi doesn't have a good way to embed the compilation date/time into the executable. There is a solution reading the linker timestamp from the PE Header of the executable. But that PE Header solution is too complex and low-level in my opinion.

So I wrote BuildStamp for injecting the compilation date/time via Pre-build event into a source file. The modified source file is then compiled into the executable.

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






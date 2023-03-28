using System.Text;

namespace BuildStamp
{
    public class CommandShowHelp : ICommand
    {
        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            output.WriteOutputLine("BuildStamp version " + args.Version.Major + "." + args.Version.Minor);

            output.WriteOutputLine("https://github.com/MircoBabin/BuildStamp - MIT license");

            output.WriteOutputLine();

            output.WriteOutputLine("BuildStamp is a compilation tool.");
            output.WriteOutputLine("It stamps the compilation date/time into a source file, using the Pre-build event.");
            output.WriteOutputLine("It can also digitally sign any executable. The codesign certificate can be on disk or in KeePass.");

            output.WriteOutputLine();
            output.WriteOutputLine();
            output.WriteOutputLine();

            output.WriteOutputLine("----------------------------------------------------");
            output.WriteOutputLine("----------------------------------------------------");
            output.WriteOutputLine("---                                              ---");
            output.WriteOutputLine("--- Stamp compilation date/time into source file ---");
            output.WriteOutputLine("---                                              ---");
            output.WriteOutputLine("----------------------------------------------------");
            output.WriteOutputLine("----------------------------------------------------");

            output.WriteOutputLine("Syntax: BuildStamp.exe stamp --filename <source-filename> --language <language>");
            output.WriteOutputLine("                             {--outputfilename <output-filename>}");
            output.WriteOutputLine("                             {--datetime yyyy-mm-ddThh:mm:ss+HH:MM}");
            output.WriteOutputLine("                             {--launchdebugger}");

            output.WriteOutputLine("- With --language the programming language is specified.");
            StringBuilder sb = new StringBuilder();
            sb.Append("  Supported languages: ");
            foreach(var name in args.Languages.Names)
            {
                sb.Append('"');
                sb.Append(name);
                sb.Append("\", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(".");
            output.WriteOutputLine(sb.ToString());

            output.WriteOutputLine("- When --outputfilename is ommitted, the <source-filename> will be overwritten.");
            output.WriteOutputLine("- With --datetime <ISO 8601> (e.g. \"1975-09-12T23:30:00+02:00\") the 'current time' can be provided.");
            output.WriteOutputLine("  When ommitted the current time from the system clock will be used.");
            output.WriteOutputLine("- When the debug switch --launchdebugger is encountered, a request to launch the debugger is started.");

            output.WriteOutputLine();

            output.WriteOutputLine("<source-filename> has to contain:");
            output.WriteOutputLine("// <BUILDSTAMP:BEGINSTAMP>");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILEDATE> is replaced with yyyy-mm-dd in local time.");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILETIME> is replaced with hh:mm:ss in local time.");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILEDATETIME> is replaced with a full ISO-8601 time.");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILEDATE-UTC> is replaced with yyyy-mm-dd in UTC time.");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILETIME-UTC> is replaced with hh:mm:ss in UTC time.");
            output.WriteOutputLine("    Inside <BUILDSTAMP:COMPILEDATETIME-UTC> is replaced with a full ISO-8601 time with timezone Z (UTC).");
            output.WriteOutputLine("// <BUILDSTAMP:ENDSTAMP>");

            output.WriteOutputLine();

            output.WriteOutputLine("e.g. for Pascal source: BuildStamp.exe stamp --filename c:\\...\\Compiled.pas --language pascal");
            output.WriteOutputLine("unit Compiled;");
            output.WriteOutputLine("interface");
            output.WriteOutputLine("// <BUILDSTAMP:BEGINSTAMP>");
            output.WriteOutputLine("const COMPILEDATE = '<BUILDSTAMP:COMPILEDATE>';");
            output.WriteOutputLine("const COMPILETIME = '<BUILDSTAMP:COMPILETIME>';");
            output.WriteOutputLine("// <BUILDSTAMP:ENDSTAMP>");
            output.WriteOutputLine("implementation");
            output.WriteOutputLine("end.");

            output.WriteOutputLine();

            output.WriteOutputLine("It is recommended for the <source-filename> to only contain BuildStamp metadata.");
            output.WriteOutputLine("And no other metadata like versionnumber, buildnumber, copyright, etc."); 
            output.WriteOutputLine("Because adding other metadata does not play well with version control (Git).");

            output.WriteOutputLine();
            output.WriteOutputLine();
            output.WriteOutputLine();
            output.WriteOutputLine("---------------------------------");
            output.WriteOutputLine("---------------------------------");
            output.WriteOutputLine("---                           ---");
            output.WriteOutputLine("--- Digitally sign executable ---");
            output.WriteOutputLine("---                           ---");
            output.WriteOutputLine("---------------------------------");
            output.WriteOutputLine("---------------------------------");

            output.WriteOutputLine("Syntax: BuildStamp.exe sign --filename <filename.exe>");
            output.WriteOutputLine("                            {--certificate <code-signing-certificate.pfx>}");
            output.WriteOutputLine("                            {--certificate-password <password for code-signing-certificate.pfx>}");
            output.WriteOutputLine("                            {--keepasscommander-path <path>} like c:\\KeePass\\Plugins");
            output.WriteOutputLine("                            {--keepass-certificate-title <title>} like \"My Code Signing Certificate\"");
            output.WriteOutputLine("                            {--keepass-certificate-attachment <attachmentname>} like \"certificate.p12\"");
            output.WriteOutputLine("                            {--keepass-certificate-password <fieldname>} like \"Certificate Password\". When omitted the default password field is used.");
            output.WriteOutputLine("                            {--sign-with-authenticode-timestamp-url <url>} like http://timestamp.digicert.com");
            output.WriteOutputLine("                            {--sign-with-sha256-rfc3161-timestamp-url <url>} like http://timestamp.digicert.com");
            output.WriteOutputLine("                            {--launchdebugger}");
            output.WriteOutputLine();
            output.WriteOutputLine("Digitally signs <filename.exe>.");
            output.WriteOutputLine();
            output.WriteOutputLine("With --certificate and --certificate-password the certificate is read from a file on disk.");
            output.WriteOutputLine("With --keepasscommander-path and --keepass-certificate-... the certificate is retrieved from the KeePass password store. The KeePass plugin KeepassCommander https://github.com/MircoBabin/KeePassCommander is used for retrieval.");
            output.WriteOutputLine("Attention: --keepass-certificate-attachment references an attachmentname in the KeePass entry with title <--keepass-certificate-title>.");
            output.WriteOutputLine("Attention: --keepass-certificate-password references a fieldname in the KeePass entry with title <--keepass-certificate-title>, and must not provide the real password.");

            output.WriteOutputLine();
            output.WriteOutputLine();
            output.WriteOutputLine();
            output.WriteOutputLine("---------------");
            output.WriteOutputLine("---------------");
            output.WriteOutputLine("---         ---");
            output.WriteOutputLine("--- License ---");
            output.WriteOutputLine("---         ---");
            output.WriteOutputLine("---------------");
            output.WriteOutputLine("---------------");
            output.WriteOutputLine("BuildStamp");
            output.WriteOutputLine("MIT license");
            output.WriteOutputLine();
            output.WriteOutputLine("Copyright (c) 2023 Mirco Babin");
            output.WriteOutputLine();
            output.WriteOutputLine("Permission is hereby granted, free of charge, to any person");
            output.WriteOutputLine("obtaining a copy of this software and associated documentation");
            output.WriteOutputLine("files (the \"Software\"), to deal in the Software without");
            output.WriteOutputLine("restriction, including without limitation the rights to use,");
            output.WriteOutputLine("copy, modify, merge, publish, distribute, sublicense, and/or sell");
            output.WriteOutputLine("copies of the Software, and to permit persons to whom the");
            output.WriteOutputLine("Software is furnished to do so, subject to the following");
            output.WriteOutputLine("conditions:");
            output.WriteOutputLine();
            output.WriteOutputLine("The above copyright notice and this permission notice shall be");
            output.WriteOutputLine("included in all copies or substantial portions of the Software.");
            output.WriteOutputLine();
            output.WriteOutputLine("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND,");
            output.WriteOutputLine("EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES");
            output.WriteOutputLine("OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND");
            output.WriteOutputLine("NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT");
            output.WriteOutputLine("HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,");
            output.WriteOutputLine("WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING");
            output.WriteOutputLine("FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR");
            output.WriteOutputLine("OTHER DEALINGS IN THE SOFTWARE.");

            return ProgramExitCode.ShowHelp;
        }
    }
}

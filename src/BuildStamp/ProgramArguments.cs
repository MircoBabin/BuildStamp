using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace BuildStamp
{
    public class ProgramArguments
    {
        public enum CommandType { ShowHelp, StampFile, OutputVersion, OutputInstallationFiles, Unknown }

        public Version Version { get; set; }
        public CommandType Command { get; set; }
        public Languages Languages { get; set; }

        public string FilenameToStamp { get; set; }
        public ILanguage FilenameToStampLanguage { get; }
        public string FilenameToOutput { get; set; }

        public DateTime Now { get; set; }

        public ProgramArguments(Assembly main, string[] args, Languages languages)
        {
            /* stamp
             * output-version
             * output-installationfilenames
             * --launchdebugger
             * --filename "c:\projects\...\version.template.pas"
             * --language "pascal"
             * --datetime "1975-09-12T23:30:00+02:00"
             * --outputfilename "c:\projects\...\version.pas"
             */

            Languages = languages;
            Version = main.GetName().Version;
            Now = DateTime.Now;

            Command = CommandType.Unknown;
            FilenameToStamp = string.Empty;
            FilenameToStampLanguage = null;
            FilenameToOutput = string.Empty;

            if (args.Length == 0)
            {
                Command = CommandType.ShowHelp;
                return;
            }

            int i = 0;
            while (i < args.Length)
            {
                string cmdname = args[i].Trim().ToLowerInvariant();
                if (cmdname == "stamp")
                {
                    Command = CommandType.StampFile;
                }
                else if (cmdname == "output-version")
                {
                    Command = CommandType.OutputVersion;
                }
                else if (cmdname == "output-installationfilenames")
                {
                    Command = CommandType.OutputInstallationFiles;
                }
                else if (cmdname == "--filename")
                {
                    i++;
                    if (i < args.Length) FilenameToStamp = args[i].Trim();
                }
                else if (cmdname == "--outputfilename")
                {
                    i++;
                    if (i < args.Length) FilenameToOutput = args[i].Trim();
                }
                else if (cmdname == "--language")
                {
                    i++;
                    if (i < args.Length) FilenameToStampLanguage = languages.getForName(args[i]);
                }
                else if (cmdname == "--datetime")
                {
                    i++;
                    if (i < args.Length)
                    {
                        string cmdvalue = args[i].Trim();
                        try
                        {
                            Now = DateTime.Parse(cmdvalue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                            switch(Now.Kind)
                            {
                                case DateTimeKind.Local:
                                case DateTimeKind.Utc:
                                    break;

                                default:
                                    throw new Exception("Please supply ISO-8601 timezone information, the string was parsed as " + Now.Kind + ".");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("--datetime \"" + cmdvalue + "\" is not valid ISO-8601. " + ex.Message);
                        }
                    }
                }
                else if (cmdname == "--launchdebugger")
                {
                    System.Diagnostics.Debugger.Launch();
                }

                i++;
            }

            if (Command == CommandType.StampFile)
            {
                if (string.IsNullOrEmpty(FilenameToOutput)) FilenameToOutput = FilenameToStamp;
            }
        }
    }
}

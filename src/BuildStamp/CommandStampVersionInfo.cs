using System;
using System.Collections.Generic;
using System.IO;

namespace BuildStamp
{
    public class CommandStampVersionInfo : StampBase, ICommand
    {
        public class Version
        {
            public string FullVersion { get; set; }
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }
            public int Build { get; set; }

            public Version(string full)
            {
                // 4
                // 4-debug
                // 4.076
                // 4.076-debug
                // 4.76.3.1234-debug
                FullVersion = full.Trim();

                string[] versionparts;
                var p = FullVersion.LastIndexOf('-');
                if (p>0)
                    versionparts = FullVersion.Substring(0, p).Split('.');
                else
                    versionparts = FullVersion.Split('.');

                Major = Convert.ToInt32(versionparts[0].Trim().TrimStart('0').PadLeft(1, '0'));
                if (versionparts.Length >= 2)
                    Minor = Convert.ToInt32(versionparts[1].Trim().TrimStart('0').PadLeft(1, '0'));
                else
                    Minor = 0;

                if (versionparts.Length >= 3)
                    Patch = Convert.ToInt32(versionparts[2].Trim().TrimStart('0').PadLeft(1, '0'));
                else
                    Patch = 0;

                if (versionparts.Length >= 4)
                    Build = Convert.ToInt32(versionparts[3].Trim().TrimStart('0').PadLeft(1, '0'));
                else
                    Build = 0;
            }

            public string as4Parts(char Separator)
            {
                return Major.ToString() + Separator + Minor.ToString() + Separator + Patch.ToString() + Separator + Build.ToString();
            }
        }

        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            output.WriteOutputLine("Sourcefile containing version string: \"" + args.VersionFilename + "\"");

            if (!File.Exists(args.VersionFilename))
            {
                output.WriteOutputLine("Sourcefile does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            var versionSourceEncoding = new FileEncodingDetector(args.VersionFilename);
            if (versionSourceEncoding.Encoding == null)
            {
                output.WriteOutputLine("Sourcefile encoding is unknown. File has no Byte Order Mark (BOM) for UTF-8 / UTF-16.");
                return ProgramExitCode.UnknownFileEncoding;
            }
            output.WriteOutputLine("Sourcefile encoding: " + versionSourceEncoding.Type);

            if (args.ProgrammingLanguage == null)
            {
                output.WriteOutputLine("Sourcefile programming language is unknown.");
                return ProgramExitCode.UnknownLanguage;
            }
            output.WriteOutputLine("Sourcefile language: " + args.ProgrammingLanguage.Name);

            var versionSource = File.ReadAllText(args.VersionFilename, versionSourceEncoding.Encoding);
            var version = GetVersion(versionSource, args.ProgrammingLanguage);
            output.WriteOutputLine("Version: " + version.FullVersion);
            output.WriteOutputLine("Version-major: " + version.Major);
            output.WriteOutputLine("Version-minor: " + version.Minor);
            output.WriteOutputLine("Version-patch: " + version.Patch);
            output.WriteOutputLine("Version-build: " + version.Build);

            output.WriteOutputLine("Resource file: \"" + args.FilenameToStamp + "\"");
            if (!File.Exists(args.FilenameToStamp))
            {
                output.WriteOutputLine("Resource file does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            var source = File.ReadAllText(args.FilenameToStamp, versionSourceEncoding.Encoding);
            var stampTemplate = GetStampTemplate(source);
            var replaced = ReplaceStamp(source, new LanguageResourceCompiler(), stampTemplate, version);

            output.WriteOutputLine("Output file: \"" + args.FilenameToOutput + "\"");
            var outputEncoding = new FileEncodingDetector(FileEncodingDetector.BomType.UTF8WithoutBom);
            File.WriteAllText(args.FilenameToOutput, replaced, outputEncoding.Encoding);

            return ProgramExitCode.Success;
        }

        private Version GetVersion(string source, ILanguage Language)
        {
            const string versionBeginMark = "<BUILDSTAMP:BEGINVERSION>";
            const string versionEndMark = "<BUILDSTAMP:ENDVERSION>";

            /*
            Assume:
            {<BUILDSTAMP:BEGINVERSION>} 
            '4.076' 
            {<BUILDSTAMP:ENDVERSION>};
            */
            

            string versionstring = ContentsBetween(source, versionBeginMark, versionEndMark).Trim();

            //strip leading/trailing string marker (' or ")
            if (versionstring.Length >= 2)
                versionstring = versionstring.Substring(1, versionstring.Length - 2);

            return new Version(versionstring);
        }

        private string ReplaceStamp(string contents, ILanguage Language,
            Template stampTemplate, Version version)
        {
            // <BUILDSTAMP:VERSION_4PARTS_COMMA_SEPARATED>
            // <BUILDSTAMP:VERSION_FULL>

            Dictionary<string, string> Replacements = new Dictionary<string, string>()
            {
                { "BUILDSTAMP:VERSION_4PARTS_COMMA_SEPARATED", Language.EscapeString(version.as4Parts(',')) },
                { "BUILDSTAMP:VERSION_4PARTS_POINT_SEPARATED", Language.EscapeString(version.as4Parts('.')) },
                { "BUILDSTAMP:VERSION_FULL", Language.EscapeString(version.FullVersion) },
            };

            return TemplateReplaceStamp(contents, Language,
                stampTemplate, Replacements);
        }

    }
}

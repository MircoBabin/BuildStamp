using System;
using System.Collections.Generic;
using System.IO;

namespace BuildStamp
{
    public class CommandStampFile : StampBase, ICommand
    {
        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            output.WriteOutputLine("File: \"" + args.FilenameToStamp + "\"");

            if (!File.Exists(args.FilenameToStamp))
            {
                output.WriteOutputLine("File does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            var encoding = new FileEncodingDetector(args.FilenameToStamp);
            if (encoding.Encoding == null)
            {
                output.WriteOutputLine("File encoding is unknown. File has no Byte Order Mark (BOM) for UTF-8 / UTF-16.");
                return ProgramExitCode.UnknownFileEncoding;
            }
            output.WriteOutputLine("Encoding: " + encoding.Type);

            if (args.ProgrammingLanguage == null)
            {
                output.WriteOutputLine("File programming language is unknown.");
                return ProgramExitCode.UnknownLanguage;
            }
            output.WriteOutputLine("Language: " + args.ProgrammingLanguage.Name);
            output.WriteOutputLine("Compilation date: " + args.Now.ToLocalTime().ToString("O"));

            var source = File.ReadAllText(args.FilenameToStamp, encoding.Encoding);
            var stampTemplate = GetStampTemplate(source);
            var replaced = ReplaceStamp(source, args.ProgrammingLanguage, stampTemplate, args.Now);

            output.WriteOutputLine("Output file: \"" + args.FilenameToOutput + "\"");
            File.WriteAllText(args.FilenameToOutput, replaced, encoding.Encoding);

            return ProgramExitCode.Success;
        }

        private string ReplaceStamp(string contents, ILanguage Language,
            Template stampTemplate, DateTime CompilationTime)
        {
            // <BUILDSTAMP:COMPILEDATE>
            // <BUILDSTAMP:COMPILETIME>
            // <BUILDSTAMP:COMPILEDATETIME>
            // <BUILDSTAMP:COMPILEDATE-UTC>
            // <BUILDSTAMP:COMPILETIME-UTC>
            // <BUILDSTAMP:COMPILEDATETIME-UTC>

            DateTime LocalCompilationTime = CompilationTime.ToLocalTime();
            string LocalCompileDate = Language.EscapeString(LocalCompilationTime.ToString("yyyy-MM-dd"));
            string LocalCompileTime = Language.EscapeString(LocalCompilationTime.ToString("HH:mm:ss"));
            string LocalCompileDateTime = Language.EscapeString(LocalCompilationTime.ToString("O"));

            DateTime UtcCompilationTime = CompilationTime.ToUniversalTime();
            string UtcCompileDate = Language.EscapeString(UtcCompilationTime.ToString("yyyy-MM-dd"));
            string UtcCompileTime = Language.EscapeString(UtcCompilationTime.ToString("HH:mm:ss"));
            string UtcCompileDateTime = Language.EscapeString(UtcCompilationTime.ToString("O"));

            Dictionary<string, string> Replacements = new Dictionary<string, string>()
            {
                { "BUILDSTAMP:COMPILEDATE", LocalCompileDate},
                { "BUILDSTAMP:COMPILETIME", LocalCompileTime},
                { "BUILDSTAMP:COMPILEDATETIME", LocalCompileDateTime},
                { "BUILDSTAMP:COMPILEDATE-UTC", UtcCompileDate},
                { "BUILDSTAMP:COMPILETIME-UTC", UtcCompileTime},
                { "BUILDSTAMP:COMPILEDATETIME-UTC", UtcCompileDateTime},
            };

            return TemplateReplaceStamp(contents, Language, 
                stampTemplate, Replacements);
        }
    }
}

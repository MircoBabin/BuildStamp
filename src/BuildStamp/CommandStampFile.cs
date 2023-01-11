using System;
using System.IO;

namespace BuildStamp
{
    public class CommandStampFile : ICommand
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

            if (args.FilenameToStampLanguage == null)
            {
                output.WriteOutputLine("File programming language is unknown.");
                return ProgramExitCode.UnknownLanguage;
            }
            output.WriteOutputLine("Language: " + args.FilenameToStampLanguage.Name);
            output.WriteOutputLine("Compilation date: " + args.Now.ToLocalTime().ToString("O"));

            var source = File.ReadAllText(args.FilenameToStamp, encoding.Encoding);
            var stampTemplate = GetStampTemplate(source);
            var replaced = ReplaceStamp(source, args.FilenameToStampLanguage, stampTemplate, args.Now);

            File.WriteAllText(args.FilenameToOutput, replaced, encoding.Encoding);

            return ProgramExitCode.Success;
        }

        private const string templateBeginMark = "<BUILDSTAMP:BEGINTEMPLATE>";
        private const string templateEndMark = "<BUILDSTAMP:ENDTEMPLATE>";

        const string stampBeginMark = "<BUILDSTAMP:BEGINSTAMP>";
        const string stampEndMark = "<BUILDSTAMP:ENDSTAMP>";

        private const string markerPrefix = "<BUILDSTAMP:";
        private const string markerSuffix = ">";

        private int EndOfLine(string source, int p1)
        {
            while (p1 < source.Length)
            {
                if (source[p1] == '\n')
                {
                    break;
                }

                p1++;
            }

            return p1;
        }

        private int BeginOfLine(string source, int p2)
        {
            while (p2 >= 0)
            {
                if (source[p2] == '\n')
                {
                    p2++;
                    break;
                }

                p2--;
            }

            return p2;
        }

        private string ContentsBetween(string source, string beginMark, string endMark)
        {
            var p1 = source.IndexOf(beginMark);
            if (p1 < 0) return string.Empty;
            p1 = EndOfLine(source, p1);
            p1++;

            var p2 = source.IndexOf(endMark, p1);
            if (p2 < p1) return string.Empty;
            p2 = BeginOfLine(source, p2);
            p2--;
            if (p2 < p1) return string.Empty;

            return source.Substring(p1, p2 - p1 + 1);
        }

        private enum TemplateType { None, Template, Stamp }
        private struct Template
        {
            public TemplateType type;
            public string contents;
        }

        private Template GetStampTemplate(string source)
        {
            // <BUILDSTAMP:BEGINTEMPLATE>
            // ...
            // <BUILDSTAMP:ENDTEMPLATE>
            //
            // or
            //
            // <BUILDSTAMP:BEGINSTAMP>
            // ...
            // <BUILDSTAMP:ENDSTAMP>

            Template template;
            template.type = TemplateType.None;

            template.contents = ContentsBetween(source, templateBeginMark, templateEndMark);
            if (!string.IsNullOrEmpty(template.contents))
            {
                template.type = TemplateType.Template;
            }
            else
            {
                template.contents = ContentsBetween(source, stampBeginMark, stampEndMark);
                if (!string.IsNullOrEmpty(template.contents))
                {
                    template.type = TemplateType.Stamp;
                }
            }

            return template;
        }

        private string ReplaceStamp(string source, ILanguage Language, Template stampTemplate, DateTime CompilationTime)
        {
            // <BUILDSTAMP:BEGINSTAMP>
            // ...
            // <BUILDSTAMP:ENDSTAMP>

            int p1 = source.IndexOf(stampBeginMark);
            if (p1 < 0) return source;

            int p2 = source.IndexOf(stampEndMark, p1);
            if (p2 < p1) return source;

            p1 = BeginOfLine(source, p1);
            p2 = EndOfLine(source, p2);

            return 
                source.Substring(0, p1) + 
                Language.SingleLineCommentPrefix + stampBeginMark + Language.SingleLineCommentSuffix +

                Language.MultiLineCommentStart + 
                templateBeginMark + Language.EndOfLine +
                stampTemplate.contents +
                templateEndMark + Language.EndOfLine +
                Language.MultiLineCommentEnd +

                ReplaceTemplate(stampTemplate.contents, Language, CompilationTime) +

                Language.SingleLineCommentPrefix + stampEndMark + Language.SingleLineCommentSuffix +
                source.Substring(p2 + 1);
        }

        private string ReplaceTemplate(string contents, ILanguage Language, DateTime CompilationTime)
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

            while (true)
            {
                var p1 = contents.IndexOf(markerPrefix);
                if (p1 < 0) break;

                var p2 = contents.IndexOf(markerSuffix, p1);
                if (p2 < 0) break;

                string name = contents.Substring(p1 + 1, p2 - p1 - 1);
                string replacement = string.Empty;

                if (name == "BUILDSTAMP:COMPILEDATE") replacement = LocalCompileDate;
                else if (name == "BUILDSTAMP:COMPILETIME") replacement = LocalCompileTime;
                else if (name == "BUILDSTAMP:COMPILEDATETIME") replacement = LocalCompileDateTime;
                else if (name == "BUILDSTAMP:COMPILEDATE-UTC") replacement = UtcCompileDate;
                else if (name == "BUILDSTAMP:COMPILETIME-UTC") replacement = UtcCompileTime;
                else if (name == "BUILDSTAMP:COMPILEDATETIME-UTC") replacement = UtcCompileDateTime;

                contents = contents.Substring(0, p1) + replacement + contents.Substring(p2 + markerSuffix.Length);
            }

            return contents;
        }

    }
}

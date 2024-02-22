using System.Collections.Generic;

namespace BuildStamp
{
    public class StampBase
    {
        private const string templateBeginMark = "<BUILDSTAMP:BEGINTEMPLATE>";
        private const string templateEndMark = "<BUILDSTAMP:ENDTEMPLATE>";

        private const string stampBeginMark = "<BUILDSTAMP:BEGINSTAMP>";
        private const string stampEndMark = "<BUILDSTAMP:ENDSTAMP>";

        private const string markerPrefix = "<BUILDSTAMP:";
        private const string markerSuffix = ">";

        protected int EndOfLine(string source, int p)
        {
            while (p < source.Length)
            {
                if (source[p] == '\n')
                {
                    break;
                }

                p++;
            }

            return (p < source.Length ? p : source.Length - 1);
        }

        protected int BeginOfLine(string source, int p)
        {
            while (p >= 0)
            {
                if (source[p] == '\n')
                {
                    p++;
                    break;
                }

                p--;
            }

            return (p >= 0 ? p : 0);
        }

        protected string ContentsBetween(string source, string beginMark, string endMark)
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

        protected enum TemplateType { None, Template, Stamp }
        protected struct Template
        {
            public TemplateType type;
            public string contents;
        }

        protected Template GetStampTemplate(string source)
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

        protected string TemplateReplaceStamp(string source, ILanguage Language, 
            Template stampTemplate, Dictionary<string, string> templateReplacements)
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

                ReplaceTemplate(stampTemplate.contents, templateReplacements) +

                Language.SingleLineCommentPrefix + stampEndMark + Language.SingleLineCommentSuffix +
                (p2 + 1 < source.Length ? source.Substring(p2 + 1) : string.Empty);
        }

        private string ReplaceTemplate(string contents, Dictionary<string,string> templateReplacements)
        {
            while (true)
            {
                var p1 = contents.IndexOf(markerPrefix);
                if (p1 < 0) break;

                var p2 = contents.IndexOf(markerSuffix, p1);
                if (p2 < 0) break;

                string name = contents.Substring(p1 + 1, p2 - p1 - 1); // e.g. "BUILDSTAMP:COMPILEDATE"
                string replacement;
                if (!templateReplacements.TryGetValue(name, out replacement))
                {
                    replacement = string.Empty;
                }

                contents = contents.Substring(0, p1) + replacement + contents.Substring(p2 + markerSuffix.Length);
            }

            return contents;
        }

    }
}

using System;
using System.Text;

namespace BuildStamp
{
    class LanguageCSharp : ILanguage
    {
        public string Name { get { return "CSharp"; } }

        public string SingleLineCommentPrefix { get { return "// "; } }
        public string SingleLineCommentSuffix { get { return EndOfLine; } }

        public string MultiLineCommentStart { get { return "/*" + EndOfLine; } }
        public string MultiLineCommentEnd { get { return "*/" + EndOfLine; } }

        public string EndOfLine { get { return Environment.NewLine;  } }

        public string EscapeString(string value)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                int chno = (int)ch;
                if (chno == 9)
                {
                    result.Append("\\t");
                }
                else if (chno == 10)
                {
                    result.Append("\\n");
                }
                else if (chno == 13)
                {
                    result.Append("\\r");
                }
                else if (chno < 32 || chno == 127)
                {
                    result.Append("\\x");
                    result.Append(chno.ToString("x"));
                }
                else if (ch == '\\')
                {
                    result.Append("\\\\");
                }
                else if (ch == '"')
                {
                    result.Append("\\\"");
                }
                else
                {
                    result.Append(ch);
                }
            }

            return result.ToString();
        }
    }
}

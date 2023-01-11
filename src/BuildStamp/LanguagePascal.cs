using System;
using System.Text;

namespace BuildStamp
{
    class LanguagePascal : ILanguage
    {
        public string Name { get { return "Pascal"; } }

        public string SingleLineCommentPrefix { get { return "// "; } }
        public string SingleLineCommentSuffix { get { return EndOfLine; } }

        public string MultiLineCommentStart { get { return "{" + EndOfLine; } }
        public string MultiLineCommentEnd { get { return "}" + EndOfLine; } }

        public string EndOfLine { get { return Environment.NewLine;  } }

        public string EscapeString(string value)
        {
            const string delimiter = "'";

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                int chno = (int)ch;
                if (chno < 32 || chno == 127)
                {
                    result.Append(delimiter);
                    result.Append("+#");
                    result.Append(chno);
                    result.Append("+");
                    result.Append(delimiter);
                }
                else if (ch == '\'')
                {
                    result.Append("''");
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

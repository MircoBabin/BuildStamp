using System;
using System.Text;

namespace BuildStamp
{
    class LanguageResourceCompiler : ILanguage
    {
        public string Name { get { return "ResourceCompiler (.rc)"; } }

        public string SingleLineCommentPrefix { get { return "// "; } }
        public string SingleLineCommentSuffix { get { return EndOfLine; } }

        public string MultiLineCommentStart { get { return "/*" + EndOfLine; } }
        public string MultiLineCommentEnd { get { return "*/" + EndOfLine; } }

        public string EndOfLine { get { return Environment.NewLine; } }

        public string EscapeString(string value)
        {
            // Assuming the L"" unicode variant
            // L"\x0000"

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                int chno = (int)ch;
                if (chno < 32 || chno >= 127)
                {
                    result.Append("\\x");
                    result.Append(chno.ToString("x").PadLeft(4, '0'));
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

using System.Collections.Generic;

namespace BuildStamp
{
    public interface ILanguage
    {
        string Name { get; }

        string SingleLineCommentPrefix { get; }
        string SingleLineCommentSuffix { get; }

        string MultiLineCommentStart { get; }
        string MultiLineCommentEnd { get; }

        string EndOfLine { get; }

        string EscapeString(string value);
    }
}

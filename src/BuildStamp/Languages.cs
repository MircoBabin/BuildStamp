using System.Collections.Generic;

namespace BuildStamp
{
    public class Languages
    {
        /*
         * Add known languages to this _languages list.
         */

        private List<ILanguage> _languages = new List<ILanguage>()
        {
            new LanguageCSharp(),
            new LanguagePascal(),
        };

        /*
         * End of known languages.
         */

        public IEnumerable<string> Names 
        { 
            get { 
                foreach(var language in _languages)
                {
                    yield return language.Name;
                }
            } 
        }

        public ILanguage getForName(string name)
        {
            var lowercaseName = name.Trim().ToLowerInvariant();

            foreach(var language in _languages)
            {
                if (language.Name.Trim().ToLowerInvariant() == lowercaseName) return language;
            }

            return null;
        }
    }
}

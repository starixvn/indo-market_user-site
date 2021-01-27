using System.Collections.Generic;
using System.Linq;

namespace GameUser.Shared.Models
{
    public static class Languages
    {
        private static readonly string defaultLanguage = "en";
        private static readonly IList<string> supportedLanguages = new List<string> { "en", "id", "th" };

        public static string Convert(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return defaultLanguage;
            }

            var formatedLanguage = language.ToLowerInvariant().Substring(0, 2);
            var existInSupportedLanguages = supportedLanguages.Any(sl => sl.Equals(formatedLanguage));

            return existInSupportedLanguages ? formatedLanguage : defaultLanguage;
        }
    }
}
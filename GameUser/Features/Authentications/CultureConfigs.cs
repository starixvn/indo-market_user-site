using GameUser.Shared.Configurations;
using GameUser.Shared.Constants;
using System.Collections.Generic;

namespace GameUser.Features.Authentications
{
    public interface ICultureConfigs
    {
        public List<string> SupportedCultures { get; }
        public string DefaultCulture { get; }

        public string ConvertLanguage(string language);
    }

    public class CultureConfigs : ICultureConfigs
    {
        public CultureConfigs(IConfigReader configReader)
        {
            SupportedCultures = new List<string>(configReader.GetArrayValue<string[]>("SupportedCultures"));
            DefaultCulture = configReader.GetValue<string>("DefaultCulture");
        }

        public List<string> SupportedCultures { get; }
        public string DefaultCulture { get; }

        public string ConvertLanguage(string language)
        {
            if (language == Language.ChineseNewSimplifiedCode)
            {
                language = Language.ChineseSimplifiedCode;
            }

            if (string.IsNullOrWhiteSpace(language) || !SupportedCultures.Contains(language))
            {
                return DefaultCulture;
            }

            return language;
        }
    }
}
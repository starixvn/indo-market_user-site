using GameUser.Shared.Configurations;
using System.Collections.Generic;

namespace GameUser.Features.Authentications
{
    public interface ICultureConfigs
    {
        public List<string> SupportedCultures { get; }
        public string DefaultCulture { get; }
    }

    public class CultureConfigs : ICultureConfigs
    {
        public CultureConfigs(IConfigReader configReader)
        {
            SupportedCultures = new List<string>(configReader.GetArrayValue<string[]>("SupportedCultures"));
            DefaultCulture = configReader.GetValue<string>("DefaultCulture");
        }

        public List<string> SupportedCultures { get; }
        public string DefaultCulture { get ; }
    }
}

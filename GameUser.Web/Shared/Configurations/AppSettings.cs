using GameUser.Shared.Configurations;
using GameUser.Web.Shared.Models;
using System.IO;
using System.Reflection;

namespace GameUser.Web.Shared.Configurations
{
    public interface IAppSettings
    {
        string ApiHost { get; }

        string ResultUrl { get; }

        string StoreFilePath { get; }

        string Env { get; }

        string StoreFileFullPath();
    }

    public class AppSettings : IAppSettings
    {
        public AppSettings(IConfigReader configReader)
        {
            ApiHost = configReader.GetValue<string>(nameof(ApiHost));
            ResultUrl = configReader.GetValue<string>(nameof(ResultUrl));
            StoreFilePath = configReader.GetValue<string>(nameof(StoreFilePath));
            Env = configReader.GetValue<string>(nameof(Env));            
        }

        public string ResultUrl { get; }

        public string ApiHost { get; }

        public string StoreFilePath { get; }

        public string StoreFileFullPath()
        {
            return IsDevelopmentEnv() ? Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), StoreFilePath) : StoreFilePath;            
        }
        public string Env { get; }

        public bool IsDevelopmentEnv()
        {
            return Env == Constant.DEV;
        }
    }
}

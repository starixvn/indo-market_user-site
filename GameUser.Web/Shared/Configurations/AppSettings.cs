using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
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
        private readonly IConfiguration configuration;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            ApiHost = GetValue<string>(nameof(ApiHost));
            ResultUrl = GetValue<string>(nameof(ResultUrl));
            StoreFilePath = GetValue<string>(nameof(StoreFilePath));
            Env = GetValue<string>(nameof(Env));            
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
            return Env == Constant.DevEnv;
        }

        public T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}

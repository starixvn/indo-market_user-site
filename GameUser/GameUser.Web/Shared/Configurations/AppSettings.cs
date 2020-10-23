using GameUser.Web.Shared.IdentityServer;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace GameUser.Web.Shared.Configurations
{
    public interface IAppSettings
    {
        string ApiHost { get; }
        IdentityServerAuth IdentityServerAuth { get; }
        public string ResultUrl { get; }
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            ApiHost = GetValue<string>(nameof(ApiHost));
            IdentityServerAuth = configuration.GetSection(nameof(IdentityServerAuth)).Get<IdentityServerAuth>();
            ResultUrl = GetValue<string>(nameof(ResultUrl));
        }

        public string ResultUrl { get; }

        public string ApiHost { get; }

        public IdentityServerAuth IdentityServerAuth { get; }

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

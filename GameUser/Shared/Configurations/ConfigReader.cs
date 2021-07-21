using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace GameUser.Shared.Configurations
{
    public interface IConfigReader
    {
        T GetValue<T>(string key);

        T GetArrayValue<T>(string key);
    }

    public class ConfigReader : IConfigReader
    {
        private readonly IConfiguration configuration;

        public ConfigReader(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public T GetArrayValue<T>(string key)
        {
            try
            {
                var value = configuration.GetSection(key).Get<T>();
                return value;
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
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
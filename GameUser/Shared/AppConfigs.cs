using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace GameUser.Shared
{
    public class AppConfigs
    {
        private readonly IConfiguration configuration;

        public AppConfigs(IConfiguration configuration)
        {
            this.configuration = configuration;
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

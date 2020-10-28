using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace GameUser.Shared
{
    public interface IAppConfigs
    {
        public string Server { get; }

        public int Port { get; }

        public bool Security { get; }

        public string SenderName { get; }

        public string SenderEmail { get; }

        public string RecieverEmail { get; set; }

        public string Account { get; }

        public string Password { get; }
    }

    public class AppConfigs : IAppConfigs
    {
        private readonly IConfiguration configuration;

        public AppConfigs(IConfiguration configuration)
        {
            this.configuration = configuration;

            Server = GetValue<string>("Email:Server");
            Port = GetValue<int>("Email:Port");
            Security = GetValue<bool>("Email:Security");
            SenderName = GetValue<string>("Email:SenderName");
            SenderEmail = GetValue<string>("Email:SenderEmail");
            RecieverEmail = GetValue<string>("Email:RecieverEmail");
            Account = GetValue<string>("Email:Account");
            Password = GetValue<string>("Email:Password");
        }

        public string Server { get; }

        public int Port { get; }

        public bool Security { get; }

        public string SenderName { get; }

        public string SenderEmail { get; }

        public string RecieverEmail { get; set; }

        public string Account { get; }

        public string Password { get; }

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

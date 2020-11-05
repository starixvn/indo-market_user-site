namespace GameUser.Features.Emails
{
    using GameUser.Shared;
    using GameUser.Shared.Configurations;
    using Microsoft.Extensions.Configuration;

    public interface IEmailConfigs
    {
        string Server { get; }

        int Port { get; }

        bool Security { get; }

        string SenderName { get; }

        string SenderEmail { get; }

        string RecieverEmail { get; set; }

        string Account { get; }

        string Password { get; }
    }

    public class EmailConfigs : IEmailConfigs
    {
        public EmailConfigs(IConfigReader configReader) 
        {           
            Server = configReader.GetValue<string>("Email:Server");
            Port = configReader.GetValue<int>("Email:Port");
            Security = configReader.GetValue<bool>("Email:Security");
            SenderName = configReader.GetValue<string>("Email:SenderName");
            SenderEmail = configReader.GetValue<string>("Email:SenderEmail");
            RecieverEmail = configReader.GetValue<string>("Email:RecieverEmail");
            Account = configReader.GetValue<string>("Email:Account");
            Password = configReader.GetValue<string>("Email:Password");
        }

        public string Server { get; }

        public int Port { get; }

        public bool Security { get; }

        public string SenderName { get; }

        public string SenderEmail { get; }

        public string RecieverEmail { get; set; }

        public string Account { get; }

        public string Password { get; }        
    }
}

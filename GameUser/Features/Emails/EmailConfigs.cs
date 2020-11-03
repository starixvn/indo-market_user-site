namespace GameUser.Features.Emails
{
    using GameUser.Shared;
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

    public class EmailConfigs : AppConfigs, IEmailConfigs
    {
        public EmailConfigs(IConfiguration configuration): base(configuration)
        {           
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
    }
}

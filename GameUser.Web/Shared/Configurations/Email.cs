namespace GameUser.Web.Shared.Configurations
{
    public class Email
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public bool Security { get; set; }

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string RecieverEmail { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }
    }
}

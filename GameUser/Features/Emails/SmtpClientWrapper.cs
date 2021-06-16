using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace GameUser.Features.Emails
{
    public interface ISmtpClientWrapper : IDisposable
    {
        bool IsConnected { get; }

        Task AuthenticateAsync(string userName, string password);

        Task ConnectAsync(string host, int port, bool useSsl);

        Task SendAsync(MimeMessage message);

        Task DisconnectAsync(bool quit);
    }

    public sealed class SmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly SmtpClient smtpClient;

        public SmtpClientWrapper(SmtpClient smtpClient)
        {
            this.smtpClient = smtpClient;
        }

        public Task AuthenticateAsync(string userName, string password) => smtpClient.AuthenticateAsync(userName, password);

        public Task ConnectAsync(string host, int port, bool useSsl) => smtpClient.ConnectAsync(host, port, useSsl);

        public Task DisconnectAsync(bool quit) => smtpClient.DisconnectAsync(quit);

        public bool IsConnected => smtpClient.IsConnected;

        public Task SendAsync(MimeMessage message) => smtpClient.SendAsync(message);

        public void Dispose()
        {
            smtpClient.Dispose();
        }
    }
}
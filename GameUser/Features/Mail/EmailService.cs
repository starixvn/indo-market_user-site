using MimeKit;
using NETCore.MailKit;
using NETCore.MailKit.Core;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Features.Mail
{
    public interface IAppEmailService
    {
        Task SendAsync(MimeMessage message);

        Task SendAsync(EmailRequest emailRequest);
    }

    public  class EmailService : NETCore.MailKit.Core.EmailService, IAppEmailService
    {
        private readonly IMailKitProvider mailKitProvider;

        public EmailService(IMailKitProvider provider) : base(provider)
        {
            mailKitProvider = provider;
        }

        public async Task SendAsync(MimeMessage message)
        {
            message.From.Add(MailboxAddress.Parse(mailKitProvider.Options.SenderEmail));

            using (var emailClient = mailKitProvider.SmtpClient)
            {
                if (!emailClient.IsConnected)
                {
                    await emailClient.AuthenticateAsync(mailKitProvider.Options.Account,
                    mailKitProvider.Options.Password);
                    await emailClient.ConnectAsync(mailKitProvider.Options.Server,
                    mailKitProvider.Options.Port, mailKitProvider.Options.Security);
                }

                await emailClient.SendAsync(message);
                await emailClient.DisconnectAsync(true);
            }
        }

        public async Task SendAsync(EmailRequest emailRequest)
        {
            var builder = new BodyBuilder { HtmlBody = emailRequest.Body };

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.To.Add(MailboxAddress.Parse(emailRequest.ToAddress));
            mimeMessage.Subject = emailRequest.Subject;
            
            if (emailRequest.Attachments != null && emailRequest.Attachments.Any())
            {
                foreach (var attachment in emailRequest.Attachments)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await attachment.CopyToAsync(memoryStream);
                        builder.Attachments.Add(attachment.FileName, memoryStream.ToArray());
                    }
                }

            }
            mimeMessage.Body = builder.ToMessageBody();

            await SendAsync(mimeMessage);
        }
    }
}

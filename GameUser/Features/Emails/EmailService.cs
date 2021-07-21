using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;

namespace GameUser.Features.Emails
{
    public interface IEmailService
    {
        Task SendAsync(MimeMessage message);

        Task SendAsync(EmailRequest emailRequest);
    }

    public class EmailService : IEmailService
    {
        private readonly IMailKitWrapper mailKitWrapper;

        public EmailService(IMailKitWrapper mailKitWrapper)
        {
            this.mailKitWrapper = mailKitWrapper;
        }

        public async Task SendAsync(MimeMessage message)
        {
            message.From.Add(MailboxAddress.Parse(mailKitWrapper.Options.SenderEmail));

            using (var emailClient = mailKitWrapper.GetSmtpClient())
            {
                if (!emailClient.IsConnected)
                {
                    await emailClient.AuthenticateAsync(mailKitWrapper.Options.Account, mailKitWrapper.Options.Password);
                    await emailClient.ConnectAsync(
                        mailKitWrapper.Options.Server,
                        mailKitWrapper.Options.Port,
                        mailKitWrapper.Options.Security);
                }

                await emailClient.SendAsync(message);
                await emailClient.DisconnectAsync(true);
            }
        }

        public async Task SendAsync(EmailRequest emailRequest)
        {
            var mimeMessage = new MimeMessage();
            var builder = new BodyBuilder { TextBody = emailRequest.Body };

            var mailToAddress = emailRequest.ToAddress.Split(new[] { ',', ';' });
            var mailToList = new InternetAddressList();
            if (mailToAddress != null && mailToAddress.Any())
            {
                foreach (var mailto in mailToAddress)
                {
                    mailToList.Add(MailboxAddress.Parse(mailto));
                }
            }
            mimeMessage.To.AddRange(mailToList);

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
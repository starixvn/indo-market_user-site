using GameUser.Shared.Wrapper;
using NETCore.MailKit;
using NETCore.MailKit.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameUser.Features.Emails
{
    public interface IMailKitWrapper
    {
        ISmtpClientWrapper GetSmtpClient();
        MailKitOptions Options { get; }
    }

    public class MailKitWrapper : IMailKitWrapper
    {
        private readonly IMailKitProvider mailKitProvider;

        public MailKitWrapper(IMailKitProvider mailKitProvider)
        {
            this.mailKitProvider = mailKitProvider;
        }

        public MailKitOptions Options => mailKitProvider.Options;

        public ISmtpClientWrapper GetSmtpClient()
        {
            return new SmtpClientWrapper(mailKitProvider.SmtpClient);
        }
    }
}

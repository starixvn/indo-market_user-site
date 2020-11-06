using FluentAssertions;
using GameUser.Features.Emails;
using GameUser.Shared.Wrapper;
using Microsoft.AspNetCore.Http;
using MimeKit;
using NETCore.MailKit.Infrastructure.Internal;
using NSubstitute;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.Emails
{
    public class EmailServiceTests
    {
        private readonly IMailKitWrapper mailKit;
        private readonly ISmtpClientWrapper emailClient;

        public EmailServiceTests()
        {
            this.mailKit = Substitute.For<IMailKitWrapper>();
            mailKit.Options.Returns(new MailKitOptions
            {
                SenderEmail = "abc",
                Account = "asf",
                Password = "ass",
                Server = "server",
                Port = 123,
                Security = false
            });
            this.emailClient = Substitute.For<ISmtpClientWrapper>();
            mailKit.GetSmtpClient().Returns(emailClient);
        }

        [Fact]
        public async Task SendAsync_WhenConnected_SendEmail()
        {
            // Arrange            
            emailClient.IsConnected.Returns(true);
            var message = new MimeMessage();
            var service = new EmailService(mailKit);

            // Act
            await service.SendAsync(message);

            // Assert
            await emailClient.DidNotReceive().AuthenticateAsync(mailKit.Options.Account, mailKit.Options.Password);
            await emailClient.DidNotReceive().ConnectAsync(mailKit.Options.Server, mailKit.Options.Port, mailKit.Options.Security);
            await emailClient.Received().SendAsync(message);
            await emailClient.Received().DisconnectAsync(true);
        }

        [Fact]
        public async Task SendAsync_WhenNotConnected_ConnectAndSendEmail()
        {
            // Arrange
            emailClient.IsConnected.Returns(false);
            var message = new MimeMessage();
            var service = new EmailService(mailKit);

            // Act
            await service.SendAsync(message);

            // Assert
            await emailClient.Received().AuthenticateAsync(mailKit.Options.Account, mailKit.Options.Password);
            await emailClient.Received().ConnectAsync(mailKit.Options.Server, mailKit.Options.Port, mailKit.Options.Security);
            await emailClient.Received().SendAsync(message);
            await emailClient.Received().DisconnectAsync(true);
        }

        [Fact]
        public async Task SendAsync_WhenCalled_BuildEmailAndSend()
        {
            // Arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");
            var files = new List<IFormFile> { file, file };
            var emailRequest = new EmailRequest
            {
                Body = "body xx",
                ToAddress = "address1;address2",
                Subject = "subject",
                Attachments = files
            };

            var service = new EmailService(mailKit);

            MimeMessage message = null;
            await emailClient.SendAsync(Arg.Do<MimeMessage>(x => message = x));

            // Act
            await service.SendAsync(emailRequest);

            // Assert
            message.Subject.Should().Be(emailRequest.Subject);
            message.To[0].ToString().Should().Be("address1");
            message.To[1].ToString().Should().Be("address2");
            (message.BodyParts.ToList()[0] as TextPart).Text.Should().Be("body xx");
            (message.BodyParts.ToList()[1] as TextPart).IsAttachment.Should().BeTrue();
            (message.BodyParts.ToList()[2] as TextPart).IsAttachment.Should().BeTrue();
        }
    }
}

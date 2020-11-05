using FluentAssertions;
using GameUser.Features.Emails;
using GameUser.Shared.Configurations;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.Emails
{
    public class EmailConfigsTests
    {
        private readonly IConfigReader configReader;

        public EmailConfigsTests() {
            this.configReader = Substitute.For<IConfigReader>();
        }

        [Fact]
        public void EmailConfigs_WhenInit_ReadEmailConfigs()
        {
            // Arrange
            configReader.GetValue<string>("Email:Server").Returns("email server");
            configReader.GetValue<int>("Email:Port").Returns(123);
            configReader.GetValue<bool>("Email:Security").Returns(true);
            configReader.GetValue<string>("Email:SenderName").Returns("sender name");
            configReader.GetValue<string>("Email:SenderEmail").Returns("sender email");
            configReader.GetValue<string>("Email:ReceiverEmail").Returns("receiver email");
            configReader.GetValue<string>("Email:Account").Returns("account");
            configReader.GetValue<string>("Email:Password").Returns("password");

            // Act
            var result = new EmailConfigs(configReader);

            // Assert
            result.Server.Should().Be("email server");
            result.Port.Should().Be(123);
            result.Security.Should().Be(true);
            result.SenderName.Should().Be("sender name");
            result.SenderEmail.Should().Be("sender email");
            result.ReceiverEmail.Should().Be("receiver email");
            result.Account.Should().Be("account");
            result.Password.Should().Be("password");
        }
    }
}

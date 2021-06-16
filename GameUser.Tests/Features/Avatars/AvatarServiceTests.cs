using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.Avatars;
using NSubstitute;
using Sentry;
using Xunit;

namespace GameUser.Tests.Features.Avatars
{
    public class AvatarServiceTests
    {
        private readonly ISentryClient sentryClient;

        public AvatarServiceTests()
        {
            this.sentryClient = Substitute.For<ISentryClient>();
        }

        [Fact]
        public async Task GetAvatar_WhenError_ReturnDefaultAvatar()
        {
            // Arrange
            var service = new AvatarService(sentryClient);

            // Act
            var result = await service.GetAvatar("123");

            // Assert
            sentryClient.ReceivedWithAnyArgs().CaptureException(default);
            result.TrimX.Should().Be(1);
            result.TrimY.Should().Be(1);
        }

        [Fact]
        public async Task GetAvatar_WhenNotError_ReturnFoundAvatar()
        {
            // Arrange
            var service = new AvatarService(sentryClient);

            // Act
            var result = await service.GetAvatar("11");

            // Assert
            result.TrimX.Should().Be(1);
            result.TrimY.Should().Be(261);
        }
    }
}
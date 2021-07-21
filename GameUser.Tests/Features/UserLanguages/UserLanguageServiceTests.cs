using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.Authentications;
using GameUser.Features.UserLanguages;
using GameUser.Features.UserLanguages.Models;
using GameUser.Features.UserSessions;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.UserLanguages
{
    public class UserLanguageServiceTests
    {
        private readonly IUserLanguageService service;
        private readonly IUserSessionService userSessionService;
        private readonly ICultureConfigs cultureConfigs;

        public UserLanguageServiceTests()
        {
            userSessionService = Substitute.For<IUserSessionService>();
            cultureConfigs = Substitute.For<ICultureConfigs>();
            service = new UserLanguageService(userSessionService, cultureConfigs);
        }

        [Fact]
        public async Task ChangeLanguage_CanNotChangeUserSession_ReturnEmptySessionId()
        {
            // Arrange
            var request = new ChangeLanguageResquest
            {
                SessionId = "id",
                Language = "EN"
            };

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns("EN");
            userSessionService.GetUserSession(Arg.Any<string>()).Returns(new UserSessionEntity());
            userSessionService.ChangeSessionId(Arg.Any<UserSessionEntity>()).Returns(string.Empty);

            // Act
            var result = await service.ChangeLanguage(request);

            // Assert
            result.SessionId.Should().BeEmpty();
            result.Language.Should().Be("EN");
        }

        [Fact]
        public async Task ChangeLanguage_CanNotChangeUserSession_ReturnNormalSessionId()
        {
            // Arrange
            var request = new ChangeLanguageResquest
            {
                SessionId = "id",
                Language = "EN"
            };

            var session = new UserSessionEntity
            {
                Language = "IN"
            };
            var newSessionId = "newId";

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns("EN");
            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.ChangeSessionId(Arg.Any<UserSessionEntity>()).Returns(newSessionId);
            userSessionService
                .When(x => x.UpdateSessionLanguage(Arg.Any<string>(), Arg.Any<string>()))
                .Do(x =>
                {
                    session.Language = "EN";
                });

            // Act
            var result = await service.ChangeLanguage(request);

            // Assert
            result.SessionId.Should().Be(newSessionId);
            result.Language.Should().Be("EN");
        }
    }
}
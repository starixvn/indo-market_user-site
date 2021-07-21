using System;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications;
using GameUser.Features.Authentications.Models;
using GameUser.Features.UserInfos;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Wrappers;
using NSubstitute;
using Sentry;
using Xunit;

namespace GameUser.Tests.Features.Authentications
{
    public class AuthenticationTests
    {
        private readonly IApi api;
        private readonly IUserService userService;
        private readonly IUserSessionService userSessionService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ICultureConfigs cultureConfigs;
        private readonly IUserInfoService userInfoService;
        private readonly AuthenticationService service;

        public AuthenticationTests()
        {
            var sentry = Substitute.For<ISentryClient>();
            var sessionSettings = new UserSessionSettings { SessionDuration = 10 };

            api = Substitute.For<IApi>();
            userService = Substitute.For<IUserService>();
            userSessionService = Substitute.For<IUserSessionService>();
            dateTimeProvider = Substitute.For<IDateTimeProvider>();
            cultureConfigs = Substitute.For<ICultureConfigs>();
            userInfoService = Substitute.For<IUserInfoService>();

            service = new AuthenticationService(api, userService, userSessionService, dateTimeProvider, cultureConfigs, sentry, sessionSettings, userInfoService);
        }

        [Fact]
        public async Task SignIn_VerifyTokenFailure_ReturnSignInFailure()
        {
            // Arrange
            var verifyResult = new VerificationTokenResponse { IsSuccessful = false };
            var request = new VerificationTokenRequest();

            api.VerifyToken(Arg.Any<VerificationTokenRequest>()).Returns(verifyResult);

            // Act
            var result = await service.SignIn(request);
            // Assert
            result.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public async Task SignIn_WhenCalled_ReturnSignInSuccess()
        {
            // Arrange
            UserEntity undefinedUser = null;
            UserSessionEntity undefinedSession = null;
            var now = DateTime.Now;

            var verifyResult = new VerificationTokenResponse
            {
                IsSuccessful = true,
                ClientId = "clientId",
                Currency = "currency",
                MemberId = 1,
                MemberName = "name",
                MemberKey = "memberKey",
                IsCash = true,
                BrandName = "brandName",
                SiteName = "siteName",
                Market = "market",
                Language = "Language"
            };
            var request = new VerificationTokenRequest
            {
                BrowserUserAgent = "userAgent",
                Ip = "ip",
                Token = "token"
            };

            api.VerifyToken(Arg.Any<VerificationTokenRequest>()).Returns(verifyResult);

            dateTimeProvider.Now().Returns(now);

            userService.GetUser(Arg.Any<string>()).Returns(undefinedUser);
            userService.CreateUser(Arg.Any<UserEntity>()).Returns(x => x.Arg<UserEntity>());

            userSessionService.GetUserSessionByToken(Arg.Any<string>()).Returns(undefinedSession);
            userSessionService.CreateUserSession(Arg.Any<UserSessionEntity>()).Returns(x => x.Arg<UserSessionEntity>());

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.SignIn(request);

            // Assert
            result.IsAuthenticated.Should().BeTrue();
            result.Username.Should().Be(verifyResult.MemberKey.ToLower());
            result.Language.Should().Be(verifyResult.Language);
            result.Currency.Should().Be(verifyResult.Currency);
            result.ExpiredIn.Should().Be(now.AddMinutes(10));
        }
    
        [Fact]
        public async Task Refresh_WhenCall_ReturnRecheckInSuccess()
        {
            // Arrange
            var session = new UserSessionEntity {
                BrowserUserAgent = "userAgent"
            };
            var now = DateTime.Now;
            var request = new RefreshSessionRequest
            {
                BrowserUserAgent = "userAgent"
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.IsExpiredSession(Arg.Any<UserSessionEntity>()).Returns(false);

            dateTimeProvider.Now().Returns(now);

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.Refresh(request);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.ExpiredIn.Should().Be(now.AddMinutes(10));
        }

        [Fact]
        public async Task Refresh_BrowserUserAgentIsNotMatch_ReturnRecheckInFailure()
        {
            // Arrange
            var session = new UserSessionEntity
            {
                BrowserUserAgent = "userAgent"
            };
            var now = DateTime.Now;
            var request = new RefreshSessionRequest();

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.IsExpiredSession(Arg.Any<UserSessionEntity>()).Returns(false);

            dateTimeProvider.Now().Returns(now);

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.Refresh(request);

            // Assert
            result.IsSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task Refresh_SessionIsExpired_ReturnRecheckInFailure()
        {
            // Arrange
            var session = new UserSessionEntity
            {
                BrowserUserAgent = "userAgent"
            };
            var now = DateTime.Now;
            var request = new RefreshSessionRequest
            {
                BrowserUserAgent = "userAgent"
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.IsExpiredSession(Arg.Any<UserSessionEntity>()).Returns(true);

            dateTimeProvider.Now().Returns(now);

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.Refresh(request);

            // Assert
            result.IsSuccessful.Should().BeFalse();
        }

        [Theory]
        [InlineData("", false, true)]
        [InlineData("userAgent", false, false)]
        [InlineData("userAgent", true, true)]
        public async Task Verify_WithInputData_ReturnFailure(string userAgent, bool tokenExpired, bool userExisted)
        {
            // Arrange
            var session = new UserSessionEntity
            {
                BrowserUserAgent = "userAgent"
            };
            var now = DateTime.Now;
            var request = new VerificationSessionRequest
            {
                BrowserUserAgent = userAgent
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.IsExpiredSession(Arg.Any<UserSessionEntity>()).Returns(tokenExpired);
            userInfoService.IsUserExisted(Arg.Any<string>()).Returns(userExisted);

            dateTimeProvider.Now().Returns(now);

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.Verify(request);

            // Assert
            result.IsSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_WhenCalled_ReturnSussess()
        {
            // Arrange
            var session = new UserSessionEntity
            {
                BrowserUserAgent = "userAgent"
            };
            var now = DateTime.Now;
            var request = new VerificationSessionRequest
            {
                BrowserUserAgent = "userAgent"
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            userSessionService.IsExpiredSession(Arg.Any<UserSessionEntity>()).Returns(false);
            userInfoService.IsUserExisted(Arg.Any<string>()).Returns(true);

            dateTimeProvider.Now().Returns(now);

            cultureConfigs.ConvertLanguage(Arg.Any<string>()).Returns(x => x.Arg<string>());
            // Act
            var result = await service.Verify(request);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Session.Should().Be(session);
        }
    }
}
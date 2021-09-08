using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.MiddleSite;
using GameUser.Features.UserInfos;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NSubstitute;
using Sentry;
using Xunit;

namespace GameUser.Tests.Features.UserInfos
{
    public class UserInfoServiceTests
    {
        private readonly IGenericRepository<UserInfoEntity> repository;
        private readonly IUserSessionService userSessionService;
        private readonly IUserService userService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IMiddleSiteService middleSiteService;
        private readonly IMemoryCache memoryCache;
        private readonly UserInfoService service;
        private readonly ISentryClient sentryClient;
        private readonly IApi api;

        public UserInfoServiceTests()
        {
            var settings = new UserInfoSettings
            {
                CacheTimeout = 10,
                CurrencyRegex = "",
                NicknamePrefix = ""
            };

            memoryCache = Create.MockedMemoryCache();
            repository = Substitute.For<IGenericRepository<UserInfoEntity>>();
            userSessionService = Substitute.For<IUserSessionService>();
            userService = Substitute.For<IUserService>();
            dateTimeProvider = Substitute.For<IDateTimeProvider>();
            middleSiteService = Substitute.For<IMiddleSiteService>();
            sentryClient = Substitute.For<ISentryClient>();
            api = Substitute.For<IApi>();
            service = new UserInfoService(api, memoryCache, settings, repository, userSessionService, dateTimeProvider, userService, middleSiteService, RandomFunc, sentryClient);
        }

        [Fact]
        public async Task GetUserInfo_WhenCalled_ReturnUserInfEntity()
        {
            // Arrange
            var entity = new UserInfoEntity
            {
                AvatarId = 1,
                BrandName = "VG",
                Currency = "IN",
                IsCash = false,
                Name = "Name",
                Red = 0,
                UserId = "id"
            };

            await memoryCache.GetOrCreateAsync(entity.UserId, (cache) => Task.FromResult(entity));

            // Act
            var result = await service.GetUserInfo(entity.UserId);

            // Assert
            result.Should().Be(entity);
        }

        [Fact]
        public async Task IsUserExisted_UserSessionIsNull_ReturnFalse()
        {
            // Arrange
            var sessionId = "id";

            UserSessionEntity session = null;

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);

            // Act
            var result = await service.IsUserExisted(sessionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserExisted_UserIsNull_ReturnFalse()
        {
            // Arrange
            var sessionId = "id";
            var userId = "id";

            UserInfoEntity userInfo = null;
            UserSessionEntity session = new UserSessionEntity
            {
                UserId = userId
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            await memoryCache.GetOrCreateAsync(userId, (cache) => Task.FromResult(userInfo));

            // Act
            var result = await service.IsUserExisted(sessionId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserExisted_WhenCalled_ReturnTrue()
        {
            // Arrange
            var sessionId = "id";
            var userId = "id";
            var userInfo = new UserInfoEntity
            {
                AvatarId = 1,
                BrandName = "VG",
                Currency = "IN",
                IsCash = false,
                Name = "Name",
                Red = 0,
                UserId = "id"
            };

            UserSessionEntity session = new UserSessionEntity
            {
                UserId = userId
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            await memoryCache.GetOrCreateAsync(userId, (cache) => Task.FromResult(userInfo));

            // Act
            var result = await service.IsUserExisted(sessionId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserInfoCacheFactory_FoundUserInfo_ReturnUserInfoAndChangeCache()
        {
            // Arrange
            var userInfo = new UserInfoEntity
            {
                AvatarId = 1,
                BrandName = "VG",
                Currency = "IN",
                IsCash = false,
                Name = "Name",
                Red = 0,
                UserId = "id"
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);

            var cacheEntry = new CacheEntryFake(userInfo.UserId, memoryCache);

            // Act
            var result = await service.UserInfoCacheFactory(cacheEntry);

            // Assert
            result.Should().Be(userInfo);
            cacheEntry.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task UserInfoCacheFactory_WhenNotFoundUserInfo_ReturnNull()
        {
            // Arrange
            UserInfoEntity userInfo = null;
            var userId = "key";

            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);

            var cacheEntry = new CacheEntryFake(userId, memoryCache);

            // Act
            var result = await service.UserInfoCacheFactory(cacheEntry);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UserInfoCacheFactory_WhenCalled_ReturnStatus()
        {
            // Arrange
            var nickname = "nickname";
            var now = DateTime.Now;

            UserInfoEntity userInfo = null;
            var session = new UserSessionEntity
            {
                MemberId = 123,
                UserId = "userId",
                Currency = "IN",
                SessionId = "id"
            };

            var user = new UserEntity
            {
                Local = new LocalUserEntity
                {
                    IsCash = false,
                    BrandName = "VG",
                    Currency = "IN"
                }
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(session);
            middleSiteService.IsNicknameExisted(Arg.Any<string>()).Returns(false);
            userService.GetUser(Arg.Any<string>()).Returns(user);
            repository.InsertOneAsync(Arg.Any<UserInfoEntity>()).Returns(Task.FromResult(userInfo));
            dateTimeProvider.Now().Returns(now);

            // Act
            var result = await service.CreateUserInfo(session.SessionId, nickname);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Message.Should().BeNullOrEmpty();
        }

        private static int RandomFunc(int min, int max) => min + max;

        [Fact]
        public async Task UpdateUserAvatar_WhenCalled_ReturnChangedAvatarSuccess()
        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "sessionId",
                UserId = "userId"
            };

            var newAvatarId = 2;

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);

            //Act
            var result = await service.UpdateUserAvatar(userSession.SessionId, newAvatarId);

            //Assert
            result.IsSuccessful.Should().BeTrue();
            result.AvatarId.Should().Be(newAvatarId);
            result.Message.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAvatar_WhenUserSessionIsNull_ReturnNotSuccessfulResult()
        {
            var sessionId = "id";

            UserSessionEntity userSession = null;

            var newAvatarId = 2;

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);

            var result = await service.UpdateUserAvatar(sessionId, newAvatarId);

            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Be("Error changing avatar");
        }

        [Fact]
        public async Task GetUserInfoLobby_WhenCalled_ReturnSuccessfulResult()

        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "id",
            };

            var userInfo = new UserInfoEntity
            {
                AvatarId = 1,
                IsCash = false,
                Name = "Name",
                UserId = "id"
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);
            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);

            //Act
            var result = await service.GetUserInfoLobby(userSession.SessionId);

            //Assert
            result.IsSuccessful.Should().BeTrue();
            result.Message.Should().BeNull();
            result.AvatarId.Should().Be(userInfo.AvatarId);
            result.Nickname.Should().Be(userInfo.Name);
            result.Language.Should().Be(userSession.Language);
            result.IsCash.Should().Be(userInfo.IsCash);
        }

        [Fact]
        public async Task GetUserInfoLobby_WhenUserInfoIsNull_ReturnNotSuccessfulResult()
        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "id",
                Language = "en"
            };

            UserInfoEntity userInfo = null;

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);
            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);

            //Act
            var result = await service.GetUserInfoLobby(userSession.SessionId);

            //Assert
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Be("Error retrieving data");
        }

        [Fact]
        public async Task GetUserBalance_WhenCalled_ReturnsSuccessfulResult()
        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "id",
                UserId = "userId"
            };

            var userInfo = new UserInfoEntity
            {
                Currency = "IN",
                Name = "Name",
                UserId = "userId"
            };

            var succesfulResponse = new MemberApiResponse<GetBalanceResponse>
            {
                Result = new GetBalanceResponse
                {
                    Balance = 1995,
                    ErrorCode = 0,
                    ErrorDescription = "",
                    ErrorMessage = "",
                }
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);
            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);
            api.GetBalance(Arg.Any<GetBalanceRequest>()).Returns(succesfulResponse);

            //Act
            var result = await service.GetUserBalance(userSession.SessionId);


            //Assert
            result.Balance.Should().Be(succesfulResponse.Result.Balance);
            result.Currency.Should().Be(userInfo.Currency);
            result.IsSuccessful.Should().BeTrue();
            result.Message.Should().BeNull();
        }

        [Fact]
        public async Task GetUserBalance_WhenUserSessionIsNull_ReturnsUnsuccesfulResult()
        {
            //Arrange
            var sessionId = "id";

            UserSessionEntity userSession = null;

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);

            //Act
            var result = await service.GetUserBalance(sessionId);

            //Assert
            result.Message.Should().Be("Error retrieving data");
            result.IsSuccessful.Should().BeFalse();
            result.Balance.Should().Be(0);
            result.Currency.Should().BeNull();
        }

        [Fact]
        public async Task GetUserBalance_WhenCallingGetBalanceApiFailed_ReturnsUnsuccesfulResult()
        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "id",
                UserId = "userId"
            };

            var userInfo = new UserInfoEntity
            {
                Currency = "IN",
                Name = "Name",
                UserId = "userId"
            };

            var nullResponse = new MemberApiResponse<GetBalanceResponse>
            {
                Result = null
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);
            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);
            api.GetBalance(Arg.Any<GetBalanceRequest>()).Returns(nullResponse);

            //Act
            var result = await service.GetUserBalance(userSession.SessionId);


            //Assert
            result.Message.Should().Be("Cannot update your account balance. Error Code: SB1902");
            result.IsSuccessful.Should().BeFalse();
            result.Balance.Should().Be(0);
            result.Currency.Should().Be(userInfo.Currency);
        }

        [Fact]
        public async Task GetUserBalance_WhenGetBalanceApiReturnsErrors_ReturnsUnsuccesfulResult()
        {
            //Arrange
            var userSession = new UserSessionEntity
            {
                SessionId = "id",
                UserId = "userId"
            };

            var userInfo = new UserInfoEntity
            {
                Currency = "IN",
                Name = "Name",
                UserId = "userId",
                Red = 1995,
            };

            var unsuccesfulResponse = new MemberApiResponse<GetBalanceResponse>
            {
                Result = new GetBalanceResponse
                {
                    ErrorCode = 1,
                    ErrorDescription = "errorDescription",
                    ErrorMessage = "errorMessage"
                }
            };

            userSessionService.GetUserSession(Arg.Any<string>()).Returns(userSession);
            repository.FindOneAsync(Arg.Any<Expression<Func<UserInfoEntity, bool>>>()).Returns(userInfo);
            api.GetBalance(Arg.Any<GetBalanceRequest>()).Returns(unsuccesfulResponse);

            //Act
            var result = await service.GetUserBalance(userSession.SessionId);

            //Assert
            result.Message
                .Should().Contain(unsuccesfulResponse.Result.ErrorMessage);
            result.Message
                .Should().Contain($"{JsonConvert.SerializeObject(unsuccesfulResponse.Result.ErrorCode)}");
            result.IsSuccessful.Should().BeFalse();
            result.Balance.Should().Be((decimal)userInfo.Red);
            result.Currency.Should().Be(userInfo.Currency);
        }
    }
}
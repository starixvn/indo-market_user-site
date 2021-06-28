using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.MiddleSite;
using GameUser.Features.UserInfos;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
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

        public UserInfoServiceTests()
        {
            var settings = new UserInfoSettings
            {
                CacheTimeout = 10
            };

            memoryCache = Create.MockedMemoryCache();
            repository = Substitute.For<IGenericRepository<UserInfoEntity>>();
            userSessionService = Substitute.For<IUserSessionService>();
            userService = Substitute.For<IUserService>();
            dateTimeProvider = Substitute.For<IDateTimeProvider>();
            middleSiteService = Substitute.For<IMiddleSiteService>();
            service = new UserInfoService(memoryCache, settings, repository, userSessionService, dateTimeProvider, userService, middleSiteService, RandomFunc);
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
                    BrandName = "VG"
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
        }

        private static int RandomFunc(int min, int max) => min + max;
    }
}
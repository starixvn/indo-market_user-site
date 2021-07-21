using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.UserSessions
{
    public class UserSessionServiceTests
    {
        private readonly UserSessionService service;
        private readonly IGenericRepository<UserSessionEntity> repository;
        private readonly IMemoryCache cache;

        public UserSessionServiceTests()
        {
            var settings = new UserSessionSettings { SessionDuration = 10 };

            cache = Create.MockedMemoryCache();
            repository = Substitute.For<IGenericRepository<UserSessionEntity>>();
            service = new UserSessionService(cache, repository, settings);
        }

        [Fact]
        public async Task GetUserSession_WhenCalled_ReturnUserSession()
        {
            // Arrange
            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "session",
                ReCheckInTime = DateTime.Now,
                SSOTime = DateTime.Now,
                SSOToken = "token",
                OWSeq = "ow seq",
                UserName = "user name",
                MemberId = 12,
                Ip = "ip",
                BrowserUserAgent = "agent",
                SiteId = "site id",
                UserId = "user id",
                Language = "language",
                Currency = "currency",
                ClientId = "client id",
                MemberName = "member name",
                Version = 11
            };

            await cache.GetOrCreateAsync(userSessionEntity.SessionId, (cache) => Task.FromResult(userSessionEntity));

            // Act
            var result = await service.GetUserSession("session");

            // Assert
            result.Should().Be(userSessionEntity);
        }

        [Fact]
        public async Task GetUserSessionByToken_WhenCalled_ReturnUserSession()
        {
            // Arrange
            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "session",
                ReCheckInTime = DateTime.Now,
                SSOTime = DateTime.Now,
                SSOToken = "token",
                OWSeq = "ow seq",
                UserName = "user name",
                MemberId = 12,
                Ip = "ip",
                BrowserUserAgent = "agent",
                SiteId = "site id",
                UserId = "user id",
                Language = "language",
                Currency = "currency",
                ClientId = "client id",
                MemberName = "member name",
                Version = 11
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<UserSessionEntity, bool>>>()).Returns(userSessionEntity);

            // Act
            var result = await service.GetUserSessionByToken("token");

            // Assert
            result.Should().Be(userSessionEntity);
        }

        [Fact]
        public async Task CreateUserSession_WhenCalled_ReturnCreatedSession()
        {
            // Arrange
            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "session",
                ReCheckInTime = DateTime.Now,
                SSOTime = DateTime.Now,
                SSOToken = "token",
                OWSeq = "ow seq",
                UserName = "user name",
                MemberId = 12,
                Ip = "ip",
                BrowserUserAgent = "agent",
                SiteId = "site id",
                UserId = "user id",
                Language = "language",
                Currency = "currency",
                ClientId = "client id",
                MemberName = "member name",
                Version = 11
            };

            repository.InsertOneAsync(Arg.Any<UserSessionEntity>()).Returns(Task.FromResult(userSessionEntity));
            await cache.GetOrCreateAsync(userSessionEntity.SessionId, (cache) => Task.FromResult(userSessionEntity));

            // Act
            var result = await service.CreateUserSession(userSessionEntity);

            // Assert
            result.Should().Be(userSessionEntity);
        }

        [Fact]
        public async Task SessionCacheFactory_SessionIsExpired_ReturnSessionAndNotChangeCache()
        {
            // Arrange
            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "session",
                ReCheckInTime = DateTime.UtcNow.AddMinutes(-20),
                SSOTime = DateTime.UtcNow,
                SSOToken = "token",
                OWSeq = "ow seq",
                UserName = "user name",
                MemberId = 12,
                Ip = "ip",
                BrowserUserAgent = "agent",
                SiteId = "site id",
                UserId = "user id",
                Language = "language",
                Currency = "currency",
                ClientId = "client id",
                MemberName = "member name",
                Version = 11
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<UserSessionEntity, bool>>>()).Returns(userSessionEntity);
            var cacheEntry = new CacheEntryFake(string.Empty, cache);

            // Act
            var result = await service.SessionCacheFactory(cacheEntry);

            // Assert
            result.Should().Be(userSessionEntity);
        }

        [Fact]
        public async Task SessionCacheFactory_SessionIsExpired_ReturnSessionAndChangeCache()
        {
            // Arrange
            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "session",
                ReCheckInTime = DateTime.UtcNow,
                SSOTime = DateTime.UtcNow,
                SSOToken = "token",
                OWSeq = "ow seq",
                UserName = "user name",
                MemberId = 12,
                Ip = "ip",
                BrowserUserAgent = "agent",
                SiteId = "site id",
                UserId = "user id",
                Language = "language",
                Currency = "currency",
                ClientId = "client id",
                MemberName = "member name",
                Version = 11
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<UserSessionEntity, bool>>>()).Returns(userSessionEntity);
            var cacheEntry = new CacheEntryFake(string.Empty, cache);

            // Act
            var result = await service.SessionCacheFactory(cacheEntry);

            // Assert
            result.Should().Be(userSessionEntity);
            cacheEntry.AbsoluteExpirationRelativeToNow.Should().BePositive();
        }

        [Fact]
        public async Task SessionCacheFactory_SessionIsNull_ReturnNull()
        {
            // Arrange
            UserSessionEntity userSessionEntity = null;

            repository.FindOneAsync(Arg.Any<Expression<Func<UserSessionEntity, bool>>>()).Returns(userSessionEntity);
            var cacheEntry = new CacheEntryFake(string.Empty, cache);

            // Act
            var result = await service.SessionCacheFactory(cacheEntry);

            // Assert
            result.Should().BeNull();
        }
    }
}
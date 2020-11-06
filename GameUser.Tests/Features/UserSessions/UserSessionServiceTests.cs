using AutoFixture;
using FluentAssertions;
using GameUser.Features.UserSessions;
using GameUser.Shared.Repositories;
using NSubstitute;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.UserSessions
{
    public class UserSessionServiceTests
    {
        [Fact]
        public async Task GetUserSession_WhenCalled_FindUserSession()
        {
            // Arrange
            var repository = Substitute.For<IGenericRepository<UserSessionEntity>>();
            var service = new UserSessionService(repository);

            var userSessionEntity = new UserSessionEntity
            {
                SessionId = "sesss ",
                ReCheckInTime = DateTime.Now
            };
            repository.FindOneAsync(Arg.Any<Expression<Func<UserSessionEntity, bool>>>()).Returns(userSessionEntity);

            // Act
            var result = await service.GetUserSession("abc");

            // Assert
            result.Should().Be(userSessionEntity);
        }
    }
}

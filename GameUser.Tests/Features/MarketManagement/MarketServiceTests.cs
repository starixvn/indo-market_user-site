using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.MarketManagement;
using GameUser.Features.UserInfos;
using GameUser.Shared.Database;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.MarketManagement
{
    public class MarketServiceTests
    {
        private readonly IMarketService service;
        private readonly IGenericRepository<MarketEntity> repository;
        private readonly IUserInfoService userInfoService;

        public MarketServiceTests()
        {
            repository = Substitute.For<IGenericRepository<MarketEntity>>();
            userInfoService = Substitute.For<IUserInfoService>();

            service = new MarketService(repository, userInfoService);
        }

        [Fact]
        public async Task GetMarketByCurrency_WhenCalled_ReturnMarketEntity()
        {
            // Arrange
            var entity = new MarketEntity
            {
                Currencies = new List<string> { "INDO", "THAI" },
                Enabled = true,
                MarketName = "WORLD"
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<MarketEntity, bool>>>()).Returns(entity);

            // Act
            var result = await service.GetMarketByCurrency("WORLD");

            // Assert
            result.Should().Equals(entity);
        }

        [Fact]
        public async Task GetMarketByUser_WhenCalled_ReturnMarketEntity()
        {
            // Arrange
            var market = new MarketEntity
            {
                Currencies = new List<string> { "INDO", "THAI" },
                Enabled = true,
                MarketName = "WORLD"
            };
            var userInfo = new UserInfoEntity
            {
                AvatarId = 1,
                BrandName = "VG",
                Currency = "INDO",
                IsCash = true,
                Name = "USER",
                JoinedOn = new DateTime(),
                Red = 0,
                Type = false,
                UID = 1,
                UserId = "user_id"
            };

            userInfoService.GetUserInfo(Arg.Any<string>()).Returns(userInfo);
            repository.FindOneAsync(Arg.Any<Expression<Func<MarketEntity, bool>>>()).Returns(market);

            // Act
            var result = await service.GetMarketByUser("user_id");

            // Assert
            result.Should().Equals(market);
        }
    }
}
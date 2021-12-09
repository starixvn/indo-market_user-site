namespace GameUser.Tests.Features.FavoriteGame
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using GameUser.Features.GameExtraConfigs;
    using GameUser.Shared.Database;
    using NSubstitute;
    using Xunit;
    public class GameExtraConfigServiceTests
    {
        private readonly IGameExtraConfigService service;
        private readonly IGenericRepository<GameExtraConfigEntity> repository;

        public GameExtraConfigServiceTests()
        {
            repository = Substitute.For<IGenericRepository<GameExtraConfigEntity>>();
            service = new GameExtraConfigService(repository);
        }

        [Fact]
        public async Task GetFavoriteGames_WhenCalled_ReturnEntity()
        {
            // Arrange
            var entity = new GameExtraConfigEntity
            {
                Type = "excluded-site-name",
                GameId = 10,
                Value = "Nex3, Nex2"
            };
            var extraConfigs = new List<GameExtraConfigEntity>();

            extraConfigs.Add(entity);
            repository.FindAsync(Arg.Any<Expression<Func<GameExtraConfigEntity, bool>>>()).Returns(extraConfigs);

            // Act
            var result = await service.GetGameExtraConfigs();

            // Asserts
            result.FirstOrDefault().Should().Equals(entity);
        }
    }
}
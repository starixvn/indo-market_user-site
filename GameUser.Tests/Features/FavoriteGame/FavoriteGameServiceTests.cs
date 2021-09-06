using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.FavoriteGame;
using GameUser.Shared.Database;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.FavoriteGame
{
    public class FavoriteGameServiceTests
    {
        private readonly IFavoriteGameService service;
        private readonly IGenericRepository<FavoriteGameEntity> repository;

        public FavoriteGameServiceTests()
        {
            repository = Substitute.For<IGenericRepository<FavoriteGameEntity>>();
            service = new FavoriteGameService(repository);
        }

        [Fact]
        public async Task AddFavoriteGame_WhenCalled_CreateNewRecordToDB()
        {
            // Arrange
            FavoriteGameEntity nullEntity = null;

            repository.FindOneAsync(Arg.Any<Expression<Func<FavoriteGameEntity, bool>>>()).Returns(nullEntity);

            // Act
            await service.AddFavoriteGame("user_id", 1);

            // Asserts
            await repository.Received().UpdateOneAsync(Arg.Any<Expression<Func<FavoriteGameEntity, bool>>>(), Arg.Any<UpdateDefinition<FavoriteGameEntity>>());
        }

        [Fact]
        public async Task GetFavoriteGames_WhenCalled_ReturnEntity()
        {
            // Arrange
            var entity = new FavoriteGameEntity
            {
                Games = new List<int> { 1, 2, 3 },
                UserId = "user_id"
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<FavoriteGameEntity, bool>>>()).Returns(entity);

            // Act
            var result = await service.GetFavoriteGames("user_id");

            // Asserts
            result.Should().Be(entity);
        }

        [Fact]
        public async Task GetFavoriteGames_NotExistInDB_ReturnEntity()
        {
            // Arrange
            var userId = "user_id";
            FavoriteGameEntity nullEntity = null;

            repository.FindOneAsync(Arg.Any<Expression<Func<FavoriteGameEntity, bool>>>()).Returns(nullEntity);

            // Act
            var result = await service.GetFavoriteGames("user_id");

            // Asserts
            result.UserId.Should().Be(userId);
            result.Games.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveFavoriteGame_WhenCalled_UpdateDB()
        {
            // Act
            await service.RemoveFavoriteGame("user_id", 1);

            // Asserts
            await repository.Received().UpdateOneAsync(Arg.Any<Expression<Func<FavoriteGameEntity, bool>>>(), Arg.Any<UpdateDefinition<FavoriteGameEntity>>());
        }
    }
}
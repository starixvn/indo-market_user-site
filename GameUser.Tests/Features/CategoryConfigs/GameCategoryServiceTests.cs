using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.CategoryConfigs;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Models;
using GameUser.Features.CategoryConfigs.Services;
using GameUser.Features.FavoriteGame;
using GameUser.Features.MarketManagement;
using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using NSubstitute;
using Sentry;
using Xunit;

namespace GameUser.Tests.Features.CategoryConfigs
{
    public class GameCategoryServiceTests
    {
        private readonly GameCategoryService service;
        private readonly IFavoriteGameService favoriteGameService;
        private readonly IMarketCategoryService marketCategoryService;
        private readonly IMarketService marketService;
        private readonly IMemoryCache memoryCache;
        private readonly CategoryConfigSettings settings;

        public GameCategoryServiceTests()
        {
            var sentry = Substitute.For<ISentryClient>();

            settings = new CategoryConfigSettings();
            favoriteGameService = Substitute.For<IFavoriteGameService>();
            marketCategoryService = Substitute.For<IMarketCategoryService>();
            marketService = Substitute.For<IMarketService>();
            memoryCache = Create.MockedMemoryCache();
            service = new GameCategoryService(favoriteGameService, marketCategoryService, marketService, memoryCache, settings, sentry);
        }

        [Fact]
        public async Task GetConfigs_MarketIsNull_ReturnAnEmptyList()
        {
            // Arrange
            var userId = "user_id";

            // Act
            var result = await service.GetConfigs(userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConfigs_CategoryIsEmtpy_ReturnOnlyFavoriteCategory()
        {
            // Arrange
            var userId = "user_id";
            var categories = new List<CategoryConfigModel>();
            var market = new MarketEntity
            {
                MarketName = "Test",
                Enabled = true,
                Id = new ObjectId()
            };
            var favoriteGames = new FavoriteGameEntity
            {
                Games = Enumerable.Range(1, 10).ToArray(),
                UserId = userId
            };

            await memoryCache.GetOrCreateAsync(market.Id.ToString(), (cache) => Task.FromResult(categories));
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);

            // Act
            var result = await service.GetConfigs(userId);

            // Assert
            result.Count.Should().Be(1);
            result.FirstOrDefault().Category.Key.Should().Be("favoriteGames");
            result.FirstOrDefault().Games.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConfigs_CategoryHasData_ReturnListOfCateogry()
        {
            // Arrange
            var userId = "user_id";
            var categories = new List<CategoryConfigModel>();
            var market = new MarketEntity
            {
                MarketName = "Test",
                Enabled = true,
                Id = new ObjectId()
            };
            var favoriteGames = new FavoriteGameEntity
            {
                Games = Enumerable.Range(1, 10).ToArray(),
                UserId = userId
            };
            var games = new List<GameIconEntity>();
            var category = new CategoryEntity
            {
                Key = "key",
                Name = "Name",
                SortOrder = 1
            };
            var game = new GameIconEntity
            {
                GameId = 1,
                SortOrder = 1,
                IconSize = "medium",
                Name = "Name",
                Tag = "NoTag"
            };

            games.Add(game);
            categories.Add(new CategoryConfigModel(category, games));

            await memoryCache.GetOrCreateAsync(market.Id.ToString(), (cache) => Task.FromResult(categories));
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);

            // Act
            var result = await service.GetConfigs(userId);

            // Assert
            result.Count.Should().Be(2);
            result.FirstOrDefault().Category.Key.Should().Be("key");
        }

        [Fact]
        public async Task CacheFactory_ExceptionWhileGettingCategory_ReturnEmtpyList()
        {
            // Arrange
            CacheEntryFake cacheEntry = new CacheEntryFake(string.Empty, memoryCache);

            marketCategoryService
                .When(x => x.GetConfigs(Arg.Any<string>()))
                .Do(x => { throw new Exception(); });

            // Act
            IEnumerable<CategoryConfigModel> result = await service.CacheFactory(cacheEntry);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task CacheFactory_WhenCalled_ReturnListOfCategory(int cacheTimeout)
        {
            // Arrange
            CacheEntryFake cacheEntry = new CacheEntryFake(string.Empty, memoryCache);
            var categories = new List<CategoryConfigModel>();
            var games = new List<GameIconEntity>();
            var category = new CategoryEntity
            {
                Key = "key",
                Name = "Name",
                SortOrder = 1
            };
            var game = new GameIconEntity
            {
                GameId = 1,
                SortOrder = 1,
                IconSize = "medium",
                Name = "Name",
                Tag = "NoTag"
            };

            games.Add(game);
            categories.Add(new CategoryConfigModel(category, games));
            settings.CacheTimeout = cacheTimeout;

            marketCategoryService.GetConfigs(Arg.Any<string>()).Returns(categories);

            // Act
            IEnumerable<CategoryConfigModel> result = await service.CacheFactory(cacheEntry);

            // Assert
            result.ToList().Count.Should().Be(1);
        }
    }
}
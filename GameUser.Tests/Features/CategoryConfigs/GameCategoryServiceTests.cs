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
using GameUser.Features.GameExtraConfigs;
using GameUser.Features.MarketManagement;
using GameUser.Features.UserSessions;
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
        private readonly IGameExtraConfigService gameExtraConfigService;

        public GameCategoryServiceTests()
        {
            var sentry = Substitute.For<ISentryClient>();

            settings = new CategoryConfigSettings();
            favoriteGameService = Substitute.For<IFavoriteGameService>();
            marketCategoryService = Substitute.For<IMarketCategoryService>();
            marketService = Substitute.For<IMarketService>();
            memoryCache = Create.MockedMemoryCache();
            gameExtraConfigService = Substitute.For<IGameExtraConfigService>();
            service = new GameCategoryService(favoriteGameService, marketCategoryService, marketService, memoryCache, settings, sentry, gameExtraConfigService);
        }

        [Fact]
        public async Task GetConfigs_MarketIsNull_ReturnAnEmptyList()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false
            };

            // Act
            var result = await service.GetConfigs(userSession);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConfigs_CategoryIsEmtpy_ReturnOnlyFavoriteCategory()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false
            };

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
                UserId = userSession.UserId
            };

            await memoryCache.GetOrCreateAsync(market.Id.ToString(), (cache) => Task.FromResult(categories));
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);

            // Act
            var result = await service.GetConfigs(userSession);

            // Assert
            result.Count.Should().Be(1);
            result.FirstOrDefault().Category.Key.Should().Be("favoriteGames");
            result.FirstOrDefault().Games.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConfigs_CategoryHasData_ReturnListOfCateogry()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false,
                SiteName = "Nex3"
            };

            var categories = new List<CategoryConfigModel>();
            var market = new MarketEntity
            {
                MarketName = "Test",
                Enabled = true,
                Id = new ObjectId()
            };
            var userType = new UserType(market.Id.ToString(), false);
            var favoriteGames = new FavoriteGameEntity
            {
                Games = Enumerable.Range(1, 10).ToArray(),
                UserId = userSession.UserId
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

            var extraConfigs = new List<GameExtraConfigEntity>();
            var extraConfig = new GameExtraConfigEntity
            {
                GameId = 1,
                Value = "Nex1",
                Type = "excluded-site-name"
            };

            games.Add(game);
            extraConfigs.Add(extraConfig);
            categories.Add(new CategoryConfigModel(category, games));

            await memoryCache.GetOrCreateAsync(userType, (cache) => Task.FromResult(categories));
            gameExtraConfigService.GetGameExtraConfigs().Returns(extraConfigs);
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);
            marketCategoryService.GetConfigs(Arg.Any<string>(), Arg.Any<bool>()).Returns(categories);

            // Act
            var result = await service.GetConfigs(userSession);


            //Assert
            result.Count.Should().Be(2);
            result.FirstOrDefault().Category.Key.Should().Be("key");
        }

        [Fact]
        public async Task CacheFactory_ExceptionWhileGettingCategory_ReturnEmtpyList()
        {
            // Arrange
            CacheEntryFake cacheEntry = new CacheEntryFake(string.Empty, memoryCache);

            marketCategoryService
                .When(x => x.GetConfigs(Arg.Any<string>(), Arg.Any<bool>()))
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
            CacheEntryFake cacheEntry = new CacheEntryFake(new UserType("market_id", true), memoryCache);
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
            marketCategoryService.GetConfigs(Arg.Any<string>(), Arg.Any<bool>()).Returns(categories);
            // Act
            IEnumerable<CategoryConfigModel> result = await service.CacheFactory(cacheEntry);

            // Assert
            result.ToList().Count.Should().Be(1);
        }

        [Fact]
        public async Task GetConfigs_WithExcludeSiteName_ReturnListOfCateogry()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false,
                SiteName = "Nex3"
            };

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
                UserId = userSession.UserId
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

            var extraConfigs = new List<GameExtraConfigEntity>();
            var extraConfig = new GameExtraConfigEntity
            {
                GameId = 1,
                Value = "Nex3, Nex1",
                Type = "excluded-site-name"
            };

            games.Add(game);
            extraConfigs.Add(extraConfig);
            categories.Add(new CategoryConfigModel(category, games));

            await memoryCache.GetOrCreateAsync(new UserType(market.Id.ToString(), userSession.IsCash), (cache) => Task.FromResult(categories));
            gameExtraConfigService.GetGameExtraConfigs().Returns(extraConfigs);
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);
            marketCategoryService.GetConfigs(Arg.Any<string>(), Arg.Any<bool>()).Returns(categories);

            // Act
            var result = await service.GetConfigs(userSession);

            //Assert          
            result[0].Games.Equals(string.Empty);
            result.FirstOrDefault().Category.Key.Should().Be("key");
        }

        [Fact]
        public async Task GetConfigs_WithCreditType_ReturnListOfCateogry()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false,
                SiteName = "Nex3"
            };

            var market = new MarketEntity
            {
                MarketName = "Test",
                Enabled = true,
                Id = new ObjectId()
            };
            var userType = new UserType(market.Id.ToString(), false);
            var favoriteGames = new FavoriteGameEntity
            {
                Games = Enumerable.Range(1, 10).ToArray(),
                UserId = userSession.UserId
            };
            var games = new List<GameIconEntity>();

            var categories = GetConfigs();
            var extraConfigs = GetExtraConfigs();

            var expectedGame = new List<GameIconEntity> {
                new GameIconEntity
                {
                    GameId = 1,
                    IconSize = "small",
                    Name = "taixiu",
                    SortOrder = 2,
                    Tag = "noTag"
                },
                new GameIconEntity
                {
                    GameId = 30,
                    IconSize = "small",
                    Name = "fishprawncrabpro",
                    SortOrder = 2,
                    Tag = "noTag"
                }
            };

            await memoryCache.GetOrCreateAsync(userType, (cache) => Task.FromResult(categories));
            gameExtraConfigService.GetGameExtraConfigs().Returns(extraConfigs);
            favoriteGameService.GetFavoriteGames(Arg.Any<string>()).Returns(favoriteGames);
            marketService.GetMarketByUser(Arg.Any<string>()).Returns(market);
            marketCategoryService.GetConfigs(Arg.Any<string>(), Arg.Any<bool>()).Returns(categories);

            // Act
            var result = await service.GetConfigs(userSession);

            //Assert          
            result[0].Games.Equals(expectedGame);
        }

        private List<GameExtraConfigEntity> GetExtraConfigs()
        {
            var extraConfig = new GameExtraConfigEntity
            {
                GameId = 1,
                Value = "2",
                Type = "offered-customer-type"
            };
            var extraConfig2 = new GameExtraConfigEntity
            {
                GameId = 1,
                Value = null,
                Type = "excluded-site-name"
            };
            var extraConfig3 = new GameExtraConfigEntity
            {
                GameId = 7,
                Value = "3",
                Type = "offered-customer-type"
            };

            return new List<GameExtraConfigEntity> { extraConfig, extraConfig2, extraConfig3 };
        }

        private List<CategoryConfigModel> GetConfigs()
        {
            var category = new CategoryEntity
            {
                Id = new ObjectId(),
                Key = "1",
                Name = "All Games",
                SortOrder = 1
            };
            var games = GetGames();
            var config = new CategoryConfigModel(category, games);
            return new List<CategoryConfigModel> { config };
        }

        private IEnumerable<GameIconEntity> GetGames()
        {
            var game = new GameIconEntity
            {
                GameId = 1,
                IconSize = "small",
                Name = "taixiu",
                SortOrder = 2,
                Tag = "noTag"
            };

            var game2 = new GameIconEntity
            {
                GameId = 7,
                IconSize = "small",
                Name = "dragontiger",
                SortOrder = 2,
                Tag = "noTag"
            };

            var game3 = new GameIconEntity
            {
                GameId = 30,
                IconSize = "small",
                Name = "fishprawncrabpro",
                SortOrder = 2,
                Tag = "noTag"
            };

            return new List<GameIconEntity> { game, game2, game3 };
        }
    }
}
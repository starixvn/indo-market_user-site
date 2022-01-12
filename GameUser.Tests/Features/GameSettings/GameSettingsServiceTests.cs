
using Features.GameUser.GameServerApiComunicators;
using GameUser.Features.GameExtraConfigs;
using GameUser.Features.GameServerApiComunicators.Models;
using GameUser.Features.GameSettings;
using GameUser.Features.UserSessions;
using MemoryCache.Testing.NSubstitute;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using NSubstitute;
using Sentry;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.GameSettings
{
    public class GameSettingsServiceTests
    {
        private const string CacheKey = "GameSettings";

        private readonly IGameServerApi gameServerApi;
        private readonly IMemoryCache memoryCache;
        private readonly GameServerApiSettings gameServerApiSettings;
        private readonly IGameExtraConfigService gameExtraConfigService;
        private readonly GameSettingsService service;

        public GameSettingsServiceTests()
        {
            var sentry = Substitute.For<ISentryClient>();
            gameServerApi = Substitute.For<IGameServerApi>();
            gameServerApiSettings = new GameServerApiSettings();
            memoryCache = Create.MockedMemoryCache();
            gameExtraConfigService = Substitute.For<IGameExtraConfigService>();
            service = new GameSettingsService(gameServerApi, memoryCache, gameServerApiSettings, gameExtraConfigService);
        }

        [Fact]
        public async Task GetGameSettings_WithDepositType_ReturnAllGamesConfigurations()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false,
                Currency = "IN",
                SiteName = "Nex3"
            };
            var gameConfigs = GetGameConfigurations();

            var marketConfigs = new MarketConigurations
            {
                Currencies = new List<string> { "IN", "ID" },
                DefaultChip = 10,
                MarketName = "INDO",
                Rate = 1,
                Settings = gameConfigs
            };
            var allGamesConfigurations = new AllGamesConfigurations
            {
                Chips = new List<Chip>(),
                MarketConfigs = new List<MarketConigurations> { marketConfigs },
                SupportedGameByCurrency = GetSupportedGameByCurrency()
            };
            var expected = GetExpectedAllGameConfigurationsByDeposit();

            var gameExtraConfigs = GetGameExtraConfigs();
            await memoryCache.GetOrCreateAsync(CacheKey, (cache) => Task.FromResult(allGamesConfigurations));
            gameExtraConfigService.GetGameExtraConfigs().Returns(gameExtraConfigs);

            // Act
            var result = await service.GetGameSettings(userSession);

            // Assert
            result.MarketConfigs.Equals(expected.MarketConfigs);
            result.SupportedGameByCurrency.Equals(expected.SupportedGameByCurrency);
        }

        [Fact]
        public async Task GetGameSettings_WithCreditType_ReturnAllGamesConfigurations()
        {
            // Arrange
            var userSession = new UserSessionEntity
            {
                UserId = "user_id",
                IsCash = false,
                Currency = "IN",
                SiteName = "Nex3"
            };
            var gameConfigs  = GetGameConfigurations();

            var marketConfigs = new MarketConigurations
            {
                Currencies = new List<string> { "IN", "ID" },
                DefaultChip = 10,
                MarketName = "INDO",
                Rate = 1,
                Settings = gameConfigs,
            };
            var allGamesConfigurations = new AllGamesConfigurations
            {
                Chips = new List<Chip>(),
                MarketConfigs = new List<MarketConigurations> { marketConfigs },
                SupportedGameByCurrency = GetSupportedGameByCurrency()
            };
            var expected = GetExpectedAllGameConfigurationsByCredit();

            var gameExtraConfigs = GetGameExtraConfigs();
            await memoryCache.GetOrCreateAsync(CacheKey, (cache) => Task.FromResult(allGamesConfigurations));
            gameExtraConfigService.GetGameExtraConfigs().Returns(gameExtraConfigs);

            // Act
            var result = await service.GetGameSettings(userSession);

            // Assert
            result.MarketConfigs.Equals(expected.MarketConfigs);
            result.SupportedGameByCurrency.Equals(expected.SupportedGameByCurrency);
        }

        private Dictionary<string, IList<string>> GetSupportedGameByCurrency()
        {
            var supportedGames = new Dictionary<string, IList<string>>
            {
                { "1", new List<string> { "IN", "INDO", "THAI", "USS" } },
                { "7", new List<string> { "IN", "INDO", "THAI", "USS" } },
                { "39", new List<string> { "IN", "INDO", "THAI", "USS" } }
            };

            return supportedGames;
        }

        private AllGamesConfigurations GetExpectedAllGameConfigurationsByDeposit()
        {
            var gameConfig = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 39,
                GameName = "casinoholdempoker",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfigurations = new List<GameConfigurations> { gameConfig };
            var marketConfigs = new MarketConigurations
            {
                Currencies = new List<string> { "IN", "ID" },
                DefaultChip = 10,
                MarketName = "INDO",
                Rate = 1,
                Settings = gameConfigurations,
            };
            var supportedGames = new Dictionary<string, IList<string>>();
            supportedGames.Add("1", new List<string> { "INDO", "THAI", "USS" });
            supportedGames.Add("7", new List<string> { "INDO", "THAI", "USS" });
            supportedGames.Add("39", new List<string> { "IN", "INDO", "THAI", "USS" });

            var allGamesConfigurations = new AllGamesConfigurations
            {
                Chips = new List<Chip>(),
                MarketConfigs = new List<MarketConigurations> { marketConfigs },
                SupportedGameByCurrency = supportedGames
            };

            return allGamesConfigurations;
        }

        private AllGamesConfigurations GetExpectedAllGameConfigurationsByCredit()
        {
            var gameConfig = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 1,
                GameName = "taixiu",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfig2 = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 39,
                GameName = "casinoholdempoker",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfigurations = new List<GameConfigurations> { gameConfig, gameConfig2 };
            var marketConfigs = new MarketConigurations
            {
                Currencies = new List<string> { "IN", "ID" },
                DefaultChip = 10,
                MarketName = "INDO",
                Rate = 1,
                Settings = gameConfigurations,
            };
            var supportedGames = new Dictionary<string, IList<string>>();
            supportedGames.Add("1", new List<string> { "IN", "INDO", "THAI", "USS" });
            supportedGames.Add("7", new List<string> { "INDO", "THAI", "USS" });
            supportedGames.Add("39", new List<string> { "IN", "INDO", "THAI", "USS" });

            var allGamesConfigurations = new AllGamesConfigurations
            {
                Chips = new List<Chip>(),
                MarketConfigs = new List<MarketConigurations> { marketConfigs },
                SupportedGameByCurrency = supportedGames
            };

            return allGamesConfigurations;
        }

        private List<GameConfigurations> GetGameConfigurations()
        {
            var gameConfig = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 1,
                GameName = "taixiu",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfig2 = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 39,
                GameName = "casinoholdempoker",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfig3 = new GameConfigurations
            {
                Chips = new List<SimpleChip>(),
                GameId = 7,
                GameName = "dragon_tiger",
                IconSize = "small",
                SortOrder = 2,
            };

            var gameConfigurations = new List<GameConfigurations> { gameConfig, gameConfig2, gameConfig3 };
            return gameConfigurations;
        }

        private List<GameExtraConfigEntity> GetGameExtraConfigs()
        {
            var gameConfig = new GameExtraConfigEntity
            {              
                GameId = 1,
                Id = new ObjectId(),
                Type = "offered-customer-type",
                Value = "2"
            };

            var gameConfig2 = new GameExtraConfigEntity
            {
                GameId = 1,
                Id = new ObjectId(),
                Type = "excluded-site-name",
                Value = null
            };

            var gameConfig3 = new GameExtraConfigEntity
            {
                GameId = 7,
                Id = new ObjectId(),
                Type = "offered-customer-type",
                Value = "3"
            };

            var gameConfig4 = new GameExtraConfigEntity
            {
                GameId = 7,
                Id = new ObjectId(),
                Type = "excluded-site-name",
                Value = "Nex3"
            };

            var gameConfig5 = new GameExtraConfigEntity
            {
                GameId = 39,
                Id = new ObjectId(),
                Type = "offered-customer-type",
                Value = "1"
            };

            var gameConfig6 = new GameExtraConfigEntity
            {
                GameId = 39,
                Id = new ObjectId(),
                Type = "excluded-site-name",
                Value = "Nova, Nex"
            };

            var gameExtraConfigs = new List<GameExtraConfigEntity> { 
                gameConfig, 
                gameConfig2, 
                gameConfig3, 
                gameConfig4, 
                gameConfig5, 
                gameConfig6 
            };
            return gameExtraConfigs;
        }
    }
}
using System;
using System.Threading.Tasks;
using Features.GameUser.GameServerApiComunicators;
using GameUser.Features.GameExtraConfigs;
using GameUser.Features.GameServerApiComunicators.Models;
using GameUser.Features.UserSessions;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using GameUser.Shared.Helpers;

namespace GameUser.Features.GameSettings
{
    public interface IGameSettingsService
    {
        Task<AllGamesConfigurations> GetGameSettings(UserSessionEntity userSession);
    }

    public class GameSettingsService : IGameSettingsService
    {
        private const string CacheKey = "GameSettings";

        private readonly IGameServerApi gameServerApi;
        private readonly IMemoryCache memoryCache;
        private readonly GameServerApiSettings gameServerApiSettings;
        private readonly IGameExtraConfigService gameExtraConfigService;

        public GameSettingsService(IGameServerApi gameServerApi,
            IMemoryCache memoryCache,
            GameServerApiSettings gameServerApiSettings,
            IGameExtraConfigService gameExtraConfigService)
        {
            this.gameServerApi = gameServerApi;
            this.memoryCache = memoryCache;
            this.gameServerApiSettings = gameServerApiSettings;
            this.gameExtraConfigService = gameExtraConfigService;
        }

        public async Task<AllGamesConfigurations> GetGameSettings(UserSessionEntity userSession)
        {
            var result = await memoryCache.GetOrCreateAsync(CacheKey, cache =>
             {
                 cache.SetAbsoluteExpiration(TimeSpan.FromSeconds(gameServerApiSettings.GameSettingsCacheTimeout));

                 return gameServerApi.GetGameSettings();
             });

           
            await FilterMarketConfigs(result, userSession);
            FilterSupportedGameByCurrency(result, userSession.Currency);
            return result;
        }

        private static void FilterSupportedGameByCurrency(AllGamesConfigurations allGameConfigs, string currency)
        {
            var marketByCurrency = allGameConfigs.MarketConfigs.FirstOrDefault(m => m.Currencies.Any(c => Equals(c, currency)));
            var gameIds = marketByCurrency?.Settings.Select(x => x.GameId).Distinct();
            foreach (var game in allGameConfigs.SupportedGameByCurrency)
            {
                var anyGameSupportCurrency = gameIds?.Any(e => e.ToString() == game.Key);
                if (anyGameSupportCurrency == null || (bool)!anyGameSupportCurrency)
                {
                    game.Value.Remove(currency);
                }
            }
        }

        private async Task FilterMarketConfigs(AllGamesConfigurations allGameConfigs,
          UserSessionEntity userSession)
        {
            var gameExtraConfigs = await gameExtraConfigService.GetGameExtraConfigs();
            var gameExtraConfigsToList = gameExtraConfigs.ToList();
            foreach (var marketConfig in allGameConfigs.MarketConfigs)
            {
                marketConfig.Settings = marketConfig.Settings
                    .Where(game => !ExcludeGameHelper.ShouldExcludeGameBySiteName(game.GameId, gameExtraConfigsToList, userSession)
                                   && !ExcludeGameHelper.ShouldExcludeGameByCustomerType(game.GameId, gameExtraConfigsToList, userSession)).ToList();
            }
        }
    }
}
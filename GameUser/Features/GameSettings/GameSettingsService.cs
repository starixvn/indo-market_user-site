using System;
using System.Threading.Tasks;
using Features.GameUser.GameServerApiComunicators;
using GameUser.Features.GameServerApiComunicators.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GameUser.Features.GameSettings
{
    public interface IGameSettingsService
    {
        Task<AllGamesConfigurations> GetGameSettings();
    }

    public class GameSettingsService : IGameSettingsService
    {
        private const string CacheKey = "GameSettings";

        private readonly IGameServerApi gameServerApi;
        private readonly IMemoryCache memoryCache;
        private readonly GameServerApiSettings gameServerApiSettings;

        public GameSettingsService(IGameServerApi gameServerApi, IMemoryCache memoryCache, GameServerApiSettings gameServerApiSettings)
        {
            this.gameServerApi = gameServerApi;
            this.memoryCache = memoryCache;
            this.gameServerApiSettings = gameServerApiSettings;
        }

        public async Task<AllGamesConfigurations> GetGameSettings()
        {
            var result = await memoryCache.GetOrCreateAsync(CacheKey, cache =>
             {
                 cache.SetSlidingExpiration(TimeSpan.FromSeconds(gameServerApiSettings.GameSettingsCacheTimeout));

                 return gameServerApi.GetGameSettings();
             });

            return result;
        }
    }
}
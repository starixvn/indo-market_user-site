using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Models;
using GameUser.Features.CategoryConfigs.Services;
using GameUser.Features.FavoriteGame;
using GameUser.Features.GameExtraConfigs;
using GameUser.Features.MarketManagement;
using GameUser.Features.UserSessions;
using GameUser.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Sentry;

namespace GameUser.Features.CategoryConfigs
{
    public interface IGameCategoryService
    {
        Task<IList<CategoryConfigModel>> GetConfigs(UserSessionEntity userSession);
    }

    public class GameCategoryService : IGameCategoryService
    {
        private readonly IFavoriteGameService favoriteGameService;
        private readonly IMarketCategoryService marketCategoryService;
        private readonly IMarketService marketService;
        private readonly IMemoryCache memoryCache;
        private readonly CategoryConfigSettings settings;
        private readonly ISentryClient sentry;
        private readonly IGameExtraConfigService gameExtraConfigService;

        public GameCategoryService(
            IFavoriteGameService favoriteGameService,
            IMarketCategoryService marketCategoryService,
            IMarketService marketService,
            IMemoryCache memoryCache,
            CategoryConfigSettings settings,
            ISentryClient sentry,
            IGameExtraConfigService gameExtraConfigService)
        {
            this.favoriteGameService = favoriteGameService;
            this.marketCategoryService = marketCategoryService;
            this.marketService = marketService;
            this.memoryCache = memoryCache;
            this.settings = settings;
            this.sentry = sentry;
            this.gameExtraConfigService = gameExtraConfigService;
        }

        public async Task<IList<CategoryConfigModel>> GetConfigs(UserSessionEntity userSession)
        {
            var market = await marketService.GetMarketByUser(userSession.UserId);
            var result = new List<CategoryConfigModel>();

            if (market != null)
            {
                var configs = await GetConfigsByMarket(market.Id.ToString(), userSession.IsCash);
                await FilterCategoryConfigs(configs, userSession);
                var favoriteCateogryConfig = await GetFavoriteCategoryConfigs(configs, userSession.UserId);

                result.AddRange(configs);
                result.Add(favoriteCateogryConfig);
            }

            return result;
        }

        private async Task FilterCategoryConfigs(IEnumerable<CategoryConfigModel> configs, UserSessionEntity userSession)
        {
            var gameExtraConfigs = await gameExtraConfigService.GetGameExtraConfigs();
            var gameExtraConfigsToList = gameExtraConfigs.ToList();
            foreach (var config in configs)
            {
                config.Games = config.Games
                      .Where(game => !ExcludeGameHelper.ShouldExcludeGameBySiteName(game.GameId, gameExtraConfigsToList, userSession)
                                     && !ExcludeGameHelper.ShouldExcludeGameByCustomerType(game.GameId, gameExtraConfigsToList, userSession)).ToList();
            }
        }
      
        private Task<IEnumerable<CategoryConfigModel>> GetConfigsByMarket(string marketId, bool isCash)
            => memoryCache.GetOrCreateAsync(new UserType(marketId, isCash), CacheFactory);


        internal async Task<IEnumerable<CategoryConfigModel>> CacheFactory(ICacheEntry cache)
        {
            IEnumerable<CategoryConfigModel> result = new List<CategoryConfigModel>();

            try
            {
                IList<PropertyInfo> filterModelProps = new List<PropertyInfo>(cache.Key.GetType().GetProperties());
                var marketId = filterModelProps[0].GetValue(cache.Key, null);
                var isCash = (bool)filterModelProps[1].GetValue(cache.Key, null);
                result = await marketCategoryService.GetConfigs(marketId.ToString(), isCash);

                if (settings.CacheTimeout > 0)
                {
                    cache.SetAbsoluteExpiration(TimeSpan.FromMinutes(settings.CacheTimeout));
                }
                else
                {
                    cache.Dispose();
                };
            }
            catch (Exception exception)
            {
                sentry.CaptureException(exception);

                cache.Dispose();
            }

            return result;
        }

        private async Task<CategoryConfigModel> GetFavoriteCategoryConfigs(IEnumerable<CategoryConfigModel> configs, string userId)
        {
            const string favoriteKey = "favoriteGames";
            var allGames = new List<GameIconEntity>();

            foreach (var config in configs)
            {
                allGames.AddRange(config.Games);
            }

            var favoriteGames = await GetFavoriteGameConfigs(userId, allGames);
            var favoriteCategory = new CategoryEntity
            {
                Key = favoriteKey,
            };
            var favoriteCateogryConfig = new CategoryConfigModel(favoriteCategory, favoriteGames);

            return favoriteCateogryConfig;
        }

        private async Task<IList<GameIconEntity>> GetFavoriteGameConfigs(string userId, IList<GameIconEntity> allGames)
        {
            var favorite = await favoriteGameService.GetFavoriteGames(userId);
            var games = new List<GameIconEntity>();

            foreach (var gameId in favorite.Games)
            {
                var game = allGames.FirstOrDefault(game => game.GameId == gameId);

                if (game != null)
                {
                    const string defaultIconSize = "medium";

                    var favoriteGame = new GameIconEntity
                    {
                        GameId = gameId,
                        IconSize = defaultIconSize,
                        Name = game.Name,
                        SortOrder = games.Count,
                        Tag = string.Empty
                    };

                    games.Add(favoriteGame);
                }
            }

            return games;
        }
    }
}
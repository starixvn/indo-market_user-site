using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Models;
using GameUser.Features.CategoryConfigs.Services;
using GameUser.Features.FavoriteGame;
using GameUser.Features.MarketManagement;
using Microsoft.Extensions.Caching.Memory;
using Sentry;

namespace GameUser.Features.CategoryConfigs
{
    public interface IGameCategoryService
    {
        Task<IList<CategoryConfigModel>> GetConfigs(string userId);
    }

    public class GameCategoryService : IGameCategoryService
    {
        private readonly IFavoriteGameService favoriteGameService;
        private readonly IMarketCategoryService marketCategoryService;
        private readonly IMarketService marketService;
        private readonly IMemoryCache memoryCache;
        private readonly CategoryConfigSettings settings;
        private readonly ISentryClient sentry;

        public GameCategoryService(
            IFavoriteGameService favoriteGameService,
            IMarketCategoryService marketCategoryService,
            IMarketService marketService,
            IMemoryCache memoryCache,
            CategoryConfigSettings settings,
             ISentryClient sentry)
        {
            this.favoriteGameService = favoriteGameService;
            this.marketCategoryService = marketCategoryService;
            this.marketService = marketService;
            this.memoryCache = memoryCache;
            this.settings = settings;
            this.sentry = sentry;
        }

        public async Task<IList<CategoryConfigModel>> GetConfigs(string userId)
        {
            var market = await marketService.GetMarketByUser(userId);
            var result = new List<CategoryConfigModel>();

            if (market != null)
            {
                var configs = await GetConfigsByMarket(market.Id.ToString());
                var favoriteCateogryConfig = await GetFavoriteCategoryConfigs(configs, userId);
                
                result = configs?.ToList() ?? result;

                result.Add(favoriteCateogryConfig);
            }

            return result;
        }

        private Task<IEnumerable<CategoryConfigModel>> GetConfigsByMarket(string marketId)
            => memoryCache.GetOrCreateAsync(marketId, CacheFactory);

        internal async Task<IEnumerable<CategoryConfigModel>> CacheFactory(ICacheEntry cache)
        {
            IEnumerable<CategoryConfigModel> result = new List<CategoryConfigModel>();

            try
            {
                result = await marketCategoryService.GetConfigs(cache.Key.ToString());

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
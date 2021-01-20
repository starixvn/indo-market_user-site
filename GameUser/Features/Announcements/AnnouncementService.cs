using GameUser.Features.Annoucements;
using GameUser.Features.MarketManagement;
using GameUser.Shared.Database;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Features.Announcements
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementEntity>> GetMessages(string currency);
    }

    public class AnnouncementService : IAnnouncementService
    {
        private readonly IGenericRepository<AnnouncementEntity> announcementRepository;
        private readonly IGenericRepository<MarketEntity> marketRepository;
        private readonly IMemoryCache memoryCache;
        private readonly ISentryClient sentryClient;
        private readonly AnnouncementSettings settings;

        public AnnouncementService(
            IGenericRepository<AnnouncementEntity> announcementRepository,
            IGenericRepository<MarketEntity> marketRepository,
            IMemoryCache memoryCache,
            ISentryClient sentryClient,
            AnnouncementSettings settings)
        {
            this.announcementRepository = announcementRepository;
            this.marketRepository = marketRepository;
            this.memoryCache = memoryCache;
            this.sentryClient = sentryClient;
            this.settings = settings;
        }

        public async Task<IEnumerable<AnnouncementEntity>> GetMessages(string currency)
        {
            var cacheKey = currency.ToUpper();
            var market = await memoryCache.GetOrCreateAsync(cacheKey, async cache =>
            {
                cache.SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.AnnouncementCacheTimeout));

                var markets = await marketRepository.FilterByAsync(x => x.Enabled);
                var market = markets.Where(market => market.Currencies.Any(dbCurrency => dbCurrency.ToUpperInvariant() == currency.ToUpperInvariant())).FirstOrDefault();

                return market;
            });

            try
            {
                return await announcementRepository.FilterByAsync(x => x.Status && x.EnabledMarkets.Contains(market.Id.ToString()));
            }
            catch (Exception exception)
            {
                sentryClient.CaptureException(exception);
                var result = new List<AnnouncementEntity>();

                return result;
            }
        }
    }
}
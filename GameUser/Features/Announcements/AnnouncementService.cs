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
            var cacheKey = $"ANNOUCEMENT_{currency.ToUpper()}";

            var result = await memoryCache.GetOrCreateAsync(cacheKey, async cache =>
            {
                IEnumerable<AnnouncementEntity> announcements = new List<AnnouncementEntity>();
                cache.SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.AnnouncementCacheTimeout));

                try
                {
                    var market = await GetMarket(currency);

                    announcements = await announcementRepository.FilterByAsync(x => x.Status && x.EnabledMarkets.Contains(market.Id.ToString()));
                }
                catch (Exception exception)
                {
                    sentryClient.CaptureException(exception);
                }

                return announcements;
            });

            return result;
        }

        private async Task<MarketEntity> GetMarket(string currency)
        {
            var markets = await marketRepository.FilterByAsync(x => x.Enabled);
            var market = markets.Where(market => market.Currencies.Any(dbCurrency => dbCurrency.ToUpperInvariant() == currency.ToUpperInvariant())).FirstOrDefault();

            return market;
        }
    }
}
using GameUser.Features.Annoucements;
using GameUser.Features.MarketManagement;
using GameUser.Shared.Database;
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

        public AnnouncementService(
            IGenericRepository<AnnouncementEntity> announcementRepository,
            IGenericRepository<MarketEntity> marketRepository)
        {
            this.announcementRepository = announcementRepository;
            this.marketRepository = marketRepository;
        }

        public async Task<IEnumerable<AnnouncementEntity>> GetMessages(string currency)
        {
            var market = await this.marketRepository.FindOneAsync(x => x.Enabled && x.Currencies.Contains(currency.ToUpper()));

            return await this.announcementRepository.FilterByAsync(x => x.Status && x.EnabledMarkets.Contains(market.Id.ToString()));
        }
    }
}

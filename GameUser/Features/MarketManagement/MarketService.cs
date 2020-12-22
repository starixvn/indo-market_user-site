using GameUser.Shared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameUser.Features.MarketManagement
{
    public interface IMarketService
    {
        Task<MarketEntity> GetMarketByCurrency(string currency);
    }

    public class MarketService : IMarketService
    {
        private readonly IGenericRepository<MarketEntity> marketRepository;

        public MarketService(
            IGenericRepository<MarketEntity> marketRepository)
        {
            this.marketRepository = marketRepository;
        }

        public async Task<MarketEntity> GetMarketByCurrency(string currency)
        {
            return null;
        }
    }
}

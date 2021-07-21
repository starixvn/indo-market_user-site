using System.Threading.Tasks;
using GameUser.Shared.Database;

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
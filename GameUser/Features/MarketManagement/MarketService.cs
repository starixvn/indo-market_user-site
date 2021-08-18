using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.UserInfos;
using GameUser.Shared.Database;

namespace GameUser.Features.MarketManagement
{
    public interface IMarketService
    {
        Task<MarketEntity> GetMarketByCurrency(string currency);

        Task<MarketEntity> GetMarketByUser(string userId);
    }

    public class MarketService : IMarketService
    {
        private readonly IGenericRepository<MarketEntity> repository;
        private readonly IUserInfoService userInfoService;

        public MarketService(
            IGenericRepository<MarketEntity> repository,
            IUserInfoService userInfoService)
        {
            this.repository = repository;
            this.userInfoService = userInfoService;
        }

        public Task<MarketEntity> GetMarketByCurrency(string currency)
            => repository.FindOneAsync(entity => entity.Enabled && entity.Currencies.Contains(currency));

        public async Task<MarketEntity> GetMarketByUser(string userId)
        {
            var userInfo = await userInfoService.GetUserInfo(userId);
            var market = await GetMarketByCurrency(userInfo.Currency);

            return market;
        }
    }
}
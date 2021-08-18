using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.MarketManagement
{
    public static class MarketInit
    {
        public static void InitMarket(this IServiceCollection services)
        {
            services.AddSingleton<IMarketService, MarketService>();
        }
    }
}
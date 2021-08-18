using GameUser.Features.CategoryConfigs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.CategoryConfigs
{
    public static class CategoryConfigInit
    {
        public static void InitCategoryConfig(this IServiceCollection services)
        {
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<IMarketCategoryService, MarketCategoryService>();
            services.AddSingleton<ICategoryConfigService, CategoryConfigService>();
            services.AddSingleton<IGameCategoryService, GameCategoryService>();
        }
    }
}
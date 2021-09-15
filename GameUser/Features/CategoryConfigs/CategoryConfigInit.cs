using GameUser.Features.CategoryConfigs.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.CategoryConfigs
{
    public static class CategoryConfigInit
    {
        public static void InitCategoryConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new CategoryConfigSettings();

            configuration.GetSection(nameof(CategoryConfigSettings)).Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<IMarketCategoryService, MarketCategoryService>();
            services.AddSingleton<ICategoryConfigService, CategoryConfigService>();
            services.AddSingleton<IGameCategoryService, GameCategoryService>();
        }
    }
}
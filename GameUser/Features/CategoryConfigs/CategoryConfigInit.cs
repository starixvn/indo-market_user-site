using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.CategoryConfigs
{
    public static class CategoryConfigInit
    {
        public static void InitCategoryConfig(this IServiceCollection services)
        {
            services.AddSingleton<ICategoryConfigService, CategoryConfigService>();
        }
    }
}
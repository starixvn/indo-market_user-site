using GameUser.Web.Shared.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Web.Shared.Middlewares
{
    public static class AppSettingMiddleware
    {
        public static void AddAppSettings(this IServiceCollection services)
        {
            services.AddSingleton<IAppSettings, AppSettings>();
        }
    }
}

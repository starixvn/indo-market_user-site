using GameUser.Web.Shared.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Web.Shared.Middlewares
{
    public static class AppSettingMiddleware
    {
        public static void AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = new AppSettings(configuration);
            services.AddSingleton<IAppSettings>(appSettings);
        }
    }
}

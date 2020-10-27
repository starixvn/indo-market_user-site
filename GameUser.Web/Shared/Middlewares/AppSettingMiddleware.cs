using GameUser.Web.Shared.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Web.Shared.Middlewares
{
    public static class AppSettingMiddleware
    {
        public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var controlPanelSettings = new GameControlPanelSettings(
                configuration["GameControlPanel:Host"]);
            services.AddSingleton<IGameControlPanelSettings>(controlPanelSettings);
        }
    }
}

using GameUser.Features.BackendAuthentications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Features.GameUser.GameServerApiComunicators
{
    public static class GameServerApiInit
    {
        public static void InitGameServerApi(this IServiceCollection services, IConfiguration configuration)
        {
            var apiSettings = new GameServerApiSettings();
            var refitSettings = BackendAuthentication.GetRefitSettings();

            configuration.GetSection(nameof(GameServerApiSettings)).Bind(apiSettings);

            services.AddSingleton(apiSettings);
            services.AddSingleton(RestService.For<IGameServerApi>(apiSettings.BaseUrl, refitSettings));
        }
    }
}
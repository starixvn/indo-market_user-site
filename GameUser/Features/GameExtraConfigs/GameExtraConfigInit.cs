namespace GameUser.Features.GameExtraConfigs
{
    using Microsoft.Extensions.DependencyInjection;
    public static class GameExtraConfigInit
    {
        public static void InitGameExtraConfig(this IServiceCollection services)
        {
            services.AddSingleton<IGameExtraConfigService, GameExtraConfigService>();
        }
    }
}

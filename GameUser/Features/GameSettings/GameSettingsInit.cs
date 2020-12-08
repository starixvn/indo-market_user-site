using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.GameSettings
{
    public static class GameSettingsInit
    {
        public static void InitGameSettings(this IServiceCollection services)
        {
            services.AddScoped<IGameSettingsService, GameSettingsService>();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.FavoriteGame
{
    public static class FavoriteGameInit
    {
        public static void InitFavoriteGame(this IServiceCollection services)
        {
            services.AddSingleton<IFavoriteGameService, FavoriteGameService>();
        }
    }
}
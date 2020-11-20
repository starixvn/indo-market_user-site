using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Avatars
{
    public static class AvatarInit
    {
        public static void InitAvatar(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAvatarService, AvatarService>();
        }
    }
}

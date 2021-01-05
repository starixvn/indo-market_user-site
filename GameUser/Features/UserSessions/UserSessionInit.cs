using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.UserSessions
{
    public static class UserSessionInit
    {
        public static void InitUserSession(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IUserSessionService, UserSessionService>();            
        }
    }
}

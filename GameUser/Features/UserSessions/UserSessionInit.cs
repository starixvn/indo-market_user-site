using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.UserSessions
{
    public static class UserSessionInit
    {
        public static void InitUserSession(this IServiceCollection services, IConfiguration configuration)
        {
            var sessionSettings = new UserSessionSettings();

            configuration.GetSection(nameof(UserSessionSettings)).Bind(sessionSettings);

            services.AddSingleton(sessionSettings);
            services.AddSingleton<IUserSessionService, UserSessionService>();
        }
    }
}
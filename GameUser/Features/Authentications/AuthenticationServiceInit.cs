using GameUser.Shared.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Authentications
{
    public static class AuthenticationServiceInit
    {
        public static void InitAuthenticationService(this IServiceCollection services, IConfigReader configReader)
        {
            var cultureConfigs = new CultureConfigs(configReader);
            services.AddSingleton(typeof(ICultureConfigs), cultureConfigs);

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
        }
    }
}
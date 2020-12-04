using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Authentications
{
    public static class AuthenticationServiceInit
    {
        public static void InitAuthenticationService(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
        }
    }
}
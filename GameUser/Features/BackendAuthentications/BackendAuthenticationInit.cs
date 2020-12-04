using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.BackendAuthentications
{
    public static class BackendAuthenticationInit
    {
        public static void InitBackendAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var backendAuthenticationSettings = new BackendAuthenticationSettings();

            configuration.GetSection(nameof(BackendAuthenticationSettings)).Bind(backendAuthenticationSettings);

            services.AddSingleton(backendAuthenticationSettings);
            services.AddHttpClient<IBackendAuthenticationService, BackendAuthenticationService>();
            services.AddTransient<ProtectedApiBearerTokenHandler>();
        }
    }
}
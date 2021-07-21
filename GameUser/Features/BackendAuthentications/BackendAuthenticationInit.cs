using GameUser.Features.MiddleSite;
using GameUser.Shared.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.BackendAuthentications
{
    public static class BackendAuthenticationInit
    {
        public static void InitBackendAuthentication(this IServiceCollection services)
        {
            var dateTimeProvider = new DateTimeProvider();

            BackendAuthentication.SetDateTimeProvider(dateTimeProvider);

            services.AddSingleton<IAuthenticationTokenStore, AuthenticationTokenStore>();
        }

        public static IApplicationBuilder UseBackendAuthenticationTokenStore(this IApplicationBuilder app)
        {
            var provider = app.ApplicationServices;
            var middleSiteSettings = provider.GetService<MiddleSiteSettings>();
            var store = provider.GetService<IAuthenticationTokenStore>();

            store.AddIdentityServer(middleSiteSettings.AuthenticationSettings);

            return app;
        }
    }
}
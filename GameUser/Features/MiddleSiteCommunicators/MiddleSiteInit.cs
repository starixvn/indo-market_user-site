using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GameUser.Features.MiddleSite
{
    public static class MiddleSiteInit
    {
        public static void InitMiddleSite(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new MiddleSiteSettings();

            configuration.GetSection(nameof(MiddleSiteSettings)).Bind(settings);

            settings.AuthenticationSettings.Id = settings.BaseUrl;

            services.AddSingleton(settings);
            services.AddSingleton(RestService.For<IMiddleSiteApi>(settings.BaseUrl));
            services.AddSingleton<IMiddleSiteService, MiddleSiteService>();
        }
    }
}
using GameUser.Features.BackendAuthentications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GameUser.Features.ApiComminicators
{
    public static class ApiInit
    {
        public static void InitApi(this IServiceCollection services, IConfiguration configuration)
        {
            var apiSettings = new ApiSettings();
            var refitSettings = BackendAuthentication.GetRefitSettings();

            configuration.GetSection(nameof(ApiSettings)).Bind(apiSettings);

            services.AddSingleton(apiSettings);
            services.AddSingleton(RestService.For<IApi>(apiSettings.BaseUrl, refitSettings));
        }
    }
}
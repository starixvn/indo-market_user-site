using GameUser.Features.BackendAuthentications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.ApiComminicators
{
    public static class ApiInit
    {
        public static void InitApi(this IServiceCollection services, IConfiguration configuration)
        {
            var apiSettings = new ApiSettings();

            configuration.GetSection(nameof(ApiSettings)).Bind(apiSettings);

            services.AddSingleton(apiSettings);
            services.AddSingleton(apiSettings.Endpoints);
            services.AddScoped<IApiService, ApiService>();

            services
                .AddHttpClient<IApiService, ApiService>(httpClient =>
                {
                    httpClient.BaseAddress = apiSettings.BaseUrl;
                })
                .AddHttpMessageHandler<ProtectedApiBearerTokenHandler>();
        }
    }
}
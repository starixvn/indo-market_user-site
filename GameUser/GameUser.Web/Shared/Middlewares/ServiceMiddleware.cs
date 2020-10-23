using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Shared.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GameUser.Web.Shared.Middlewares
{
    public static class ServiceMiddleware
    {
        public static void AddServices(this IServiceCollection services, IAppSettings appSettings)
        {
            var refitSettings = new RefitSettings();

            services.AddSingleton(RestService.For<IFeedbackApi>(appSettings.ApiHost, refitSettings));
            services.AddScoped<IFeedbackService, FeedbackService>();
        }
    }
}

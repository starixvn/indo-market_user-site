using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Shared.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Web.Shared.Middlewares
{
    public static class ServiceMiddleware
    {
        public static void AddServices(this IServiceCollection services, IAppSettings appSettings)
        {
            services.AddScoped<IFeedbackService, FeedbackService>();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using UserSite.Api.Features.Feedbacks;
using UserSite.Database.Repositories;

namespace UserSite.Api._Shared.Middlewares
{
    public static class ServiceMiddleware
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IFeedbackService, FeedbackService>();
        }
    }
}

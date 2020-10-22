using GameUser.DataAccess.Repositories;
using GameUser.Features.Feedbacks;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Shared.Middlewares
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

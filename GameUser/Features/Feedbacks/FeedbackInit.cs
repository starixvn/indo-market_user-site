using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Feedbacks
{
    public static class FeedbackInit
    {
        public static void InitFeedback(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        }
    }
}

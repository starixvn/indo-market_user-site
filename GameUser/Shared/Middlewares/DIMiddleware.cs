namespace GameUser.Shared.Middlewares
{
    using GameUser.Features.Emails;
    using GameUser.Features.Feedbacks;
    using GameUser.Features.FileUploads;
    using GameUser.Features.UserSessions;
    using GameUser.Shared.Configurations;
    using GameUser.Shared.Repositories;
    using GameUser.Shared.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DIMiddleware
    {
        public static void ConfigGameUser(this IServiceCollection services, IConfiguration configuration)
        {
            var configReader = new ConfigReader(configuration);
            services.AddSingleton(typeof(IConfigReader), configReader);
            services.AddSharedServices();
            services.InitEmail(configReader);
            services.InitFeedback(configuration);
            services.InitFileUpload(configuration);
            services.InitUserSession(configuration);
        }

        public static void AddSharedServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddSingleton(typeof(ISystemIOWrapper), typeof(SystemIOWrapper));
            services.AddSingleton(typeof(IMailKitWrapper), typeof(MailKitWrapper));
        }
    }
}

using System.Linq;
using Features.GameUser.GameServerApiComunicators;
using GameUser.Features.Announcements;
using GameUser.Features.ApiComminicators;
using GameUser.Features.Authentications;
using GameUser.Features.Avatars;
using GameUser.Features.BackendAuthentications;
using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Features.GameSettings;
using GameUser.Features.MarketManagement;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Configurations;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using GameUser.Web.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GameUser.Web.Shared.Middlewares
{
    public static class GameUserMiddleware
    {
        public static void ConfigGameUser(this IServiceCollection services, IConfiguration configuration)
        {
            var configReader = new ConfigReader(configuration);

            services.SettingCors(configuration);
            services.AddSingleton(typeof(IConfigReader), configReader);
            services.AddSharedServices();
            services.InitEmail(configReader);
            services.InitFeedback(configuration);
            services.InitFileUpload(configuration);
            services.InitUserSession(configuration);
            services.InitAvatar(configuration);
            services.InitBackendAuthentication(configuration);
            services.InitApi(configuration);
            services.InitUser();
            services.InitAuthenticationService();
            services.InitGameServerApi(configuration);
            services.InitGameSettings();
            services.InitBackendAuthentication();
            services.InitAnnouncement(configuration);
        }

        public static void AddSharedServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddSingleton<ISystemIOWrapper, SystemIOWrapper>();
            services.AddSingleton<IMailKitWrapper, MailKitWrapper>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IHttpRequestWrapper, HttpRequestWrapper>();
        }

        public static void SettingCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = new CorsSettings();

            configuration.GetSection("Cors").Bind(corsSettings);

            services.AddCors(cors => cors.AddDefaultPolicy(options
            => options
                .WithOrigins(corsSettings.AllowUrls.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()));
        }
    }
}
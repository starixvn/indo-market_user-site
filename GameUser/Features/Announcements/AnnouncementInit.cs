using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Announcements
{
    public static class AnnouncementInit
    {
        public static void InitAnnouncement(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new AnnouncementSettings();

            configuration.GetSection(nameof(AnnouncementSettings)).Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<IAnnouncementService, AnnouncementService>();
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Announcements
{
    public static class AnnouncementInit
    {
        public static void InitAnnouncement(this IServiceCollection services)
        {
            services.AddScoped<IAnnouncementService, AnnouncementService>();
        }
    }
}

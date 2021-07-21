using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.UserLanguages
{
    public static class UserLanguageInit
    {
        public static void InitUserLanguage(this IServiceCollection services)
        {
            services.AddSingleton<IUserLanguageService, UserLanguageService>();
        }
    }
}
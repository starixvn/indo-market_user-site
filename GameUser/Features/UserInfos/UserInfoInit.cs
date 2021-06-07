using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.UserInfos
{
    public static class UserInfoInit
    {
        public static void InitUserInfo(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new UserInfoSettings();

            configuration.GetSection(nameof(UserInfoSettings)).Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<IUserInfoService, UserInfoService>();
        }
    }
}

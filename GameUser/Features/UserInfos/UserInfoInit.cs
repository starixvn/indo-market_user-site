using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.UserInfos
{
    public static class UserInfoInit
    {
        public static void InitUserInfo(this IServiceCollection services)
        {
            services.AddSingleton<IUserInfoService, UserInfoService>();
        }
    }
}

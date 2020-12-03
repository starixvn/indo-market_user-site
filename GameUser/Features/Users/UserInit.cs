using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.Users
{
    public static class UserInit
    {
        public static void InitUser(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }
    }
}
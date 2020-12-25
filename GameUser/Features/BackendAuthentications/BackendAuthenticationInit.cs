using GameUser.Shared.Wrapper;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.BackendAuthentications
{
    public static class BackendAuthenticationInit
    {
        public static void InitBackendAuthentication(this IServiceCollection services)
        {
            var dateTimeProvider = new DateTimeProvider();

            BackendAuthentication.SetDateTimeProvider(dateTimeProvider);
        }
    }
}
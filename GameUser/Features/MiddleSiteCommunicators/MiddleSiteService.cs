using System.Threading.Tasks;
using GameUser.Features.BackendAuthentications;

namespace GameUser.Features.MiddleSite
{
    public interface IMiddleSiteService
    {
        Task<bool> IsNicknameExisted(string nickname);
    }

    public class MiddleSiteService : IMiddleSiteService
    {
        private readonly IMiddleSiteApi api;
        private readonly IAuthenticationTokenStore tokenStore;
        private readonly MiddleSiteSettings settings;

        public MiddleSiteService(IMiddleSiteApi api, IAuthenticationTokenStore tokenStore, MiddleSiteSettings settings)
        {
            this.api = api;
            this.tokenStore = tokenStore;
            this.settings = settings;
        }

        public async Task<bool> IsNicknameExisted(string nickname)
        {
            var token = await tokenStore.GetToken(settings.BaseUrl);
            var result = await api.IsNicknameExisted(nickname, token);

            return result;
        }
    }
}
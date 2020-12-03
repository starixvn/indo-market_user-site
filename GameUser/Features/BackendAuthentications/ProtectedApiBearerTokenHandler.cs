using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace GameUser.Features.BackendAuthentications
{
    public class ProtectedApiBearerTokenHandler: DelegatingHandler
    {
        private readonly IBackendAuthenticationService identityService;

        public ProtectedApiBearerTokenHandler(IBackendAuthenticationService identityService)
        {
            this.identityService = identityService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await identityService.GetAccessToken();

            request.SetBearerToken(accessToken.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
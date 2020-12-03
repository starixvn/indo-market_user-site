using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace GameUser.Features.BackendAuthentications
{
    public interface IBackendAuthenticationService
    {
        Task<TokenResponse> GetAccessToken();
    }

    public class BackendAuthenticationService : IBackendAuthenticationService
    {
        private static TokenResponse TokenResponse;
        private static DateTimeOffset LastTokenRequestTime;

        private readonly HttpClient httpClient;
        private readonly BackendAuthenticationSettings backendAuthenticationSettings;

        public BackendAuthenticationService(HttpClient httpClient, BackendAuthenticationSettings backendAuthenticationSettings)
        {
            this.httpClient = httpClient;
            this.backendAuthenticationSettings = backendAuthenticationSettings;
        }

        public async Task<TokenResponse> GetAccessToken()
        {
            if (TokenResponse == null || LastTokenRequestTime.AddSeconds(TokenResponse.ExpiresIn) < DateTimeOffset.UtcNow.AddMinutes(1))
            {
                var request = new DiscoveryDocumentRequest
                {
                    Address = backendAuthenticationSettings.Address,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                };

                var disco = await httpClient.GetDiscoveryDocumentAsync(request);

                if (disco.IsError)
                {
                    throw new InvalidOperationException(disco.Error);
                }

                var newTokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = backendAuthenticationSettings.ClientId,
                    ClientSecret = backendAuthenticationSettings.ClientSecret,
                    Scope = backendAuthenticationSettings.Scope,
                });

                Interlocked.Exchange(ref TokenResponse, newTokenResponse);
                LastTokenRequestTime = DateTimeOffset.UtcNow;
            }

            return TokenResponse;
        }
    }
}
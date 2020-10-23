using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameUser.Web.Shared.IdentityServer
{
    public class IdentityServerAuth
    {
        public string Url { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string SiteId { get; set; }
    }

    public static class IdentityServerManager
    {
        private static HttpClient identityClient;
        private static readonly object lockObj = new object();
        private static IdentityServerAuth auth;
        private static TokenResponse tokenResponse;
        private static DateTime lastTokenRequest;

        public static void Initialize(IdentityServerAuth identityServerAuth)
        {
            auth = identityServerAuth;
        }

        public static async Task<string> GetAccessToken()
        {
            if (tokenResponse == null || lastTokenRequest.AddSeconds(tokenResponse.ExpiresIn) < DateTime.Now.AddMinutes(5))
            {
                var client = GetIdentityClient();

                var request = new DiscoveryDocumentRequest
                {
                    Address = auth.Url,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                };

                var disco = await client.GetDiscoveryDocumentAsync(request);

                if (disco.IsError)
                {
                    throw new InvalidOperationException(disco.Error);
                }

                tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = auth.ClientId,
                    ClientSecret = auth.ClientSecret,
                    Scope = auth.SiteId,
                });

                lastTokenRequest = DateTime.Now;
            }

            return tokenResponse.AccessToken;
        }

        private static HttpClient GetIdentityClient()
        {
            if (identityClient == null)
            {
                lock (lockObj)
                {
                    if (identityClient == null)
                    {
                        identityClient = new HttpClient();
                    }
                }
            }

            return identityClient;
        }
    }
}

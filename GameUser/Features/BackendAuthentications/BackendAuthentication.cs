using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GameUser.Features.BackendAuthentications
{
    public static class BackendAuthentication
    {
        private static HttpClient identityClient;
        private static BackendAuthenticationSettings backendAuthenticationSettings;
        private static readonly object lockObj = new object();
        private static TokenResponse tokenResponse;
        private static DateTime lastTokenRequest;

        public static void InitBackendAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            backendAuthenticationSettings = new BackendAuthenticationSettings();

            configuration.GetSection(nameof(BackendAuthenticationSettings)).Bind(backendAuthenticationSettings);

            services.AddSingleton(backendAuthenticationSettings);
        }

        public static RefitSettings GetRefitSettings()
        {
            return new RefitSettings
            {
                AuthorizationHeaderValueGetter = GetAccessToken
            };
        }

        public static async Task<string> GetAccessToken()
        {
            const int fiveMinutes = 5;
            var isTokenExpired = tokenResponse == null || lastTokenRequest.AddSeconds(tokenResponse.ExpiresIn) < DateTime.Now.AddMinutes(fiveMinutes);

            if (isTokenExpired)
            {
                var client = GetIdentityClient();

                var request = new DiscoveryDocumentRequest
                {
                    Address = backendAuthenticationSettings.Address,
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
                    ClientId = backendAuthenticationSettings.ClientId,
                    ClientSecret = backendAuthenticationSettings.ClientSecret,
                    Scope = backendAuthenticationSettings.ClientId,
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
                    identityClient ??= new HttpClient();
                }
            }

            return identityClient;
        }
    }
}
using GameUser.Shared.Wrappers;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameUser.Features.BackendAuthentications
{
    public interface IAuthenticationTokenStore
    {
        Task<string> GetToken(string id);

        void AddIdentityServer(AuthenticationSettings authenticationSettings);
    }

    public class AuthenticationTokenStore : IAuthenticationTokenStore
    {
        private static readonly object lockObject = new object();

        private static HttpClient httpClient;

        private readonly IDictionary<string, AuthenticationSettings> servers;
        private readonly IDictionary<string, AuthenticationToken> tokens;
        private readonly IDateTimeProvider dateTimeProvider;

        public AuthenticationTokenStore(IDateTimeProvider dateTimeProvider)
        {
            servers = new Dictionary<string, AuthenticationSettings>();
            tokens = new Dictionary<string, AuthenticationToken>();

            this.dateTimeProvider = dateTimeProvider;
        }

        public void AddIdentityServer(AuthenticationSettings authenticationSettings)
        {
            if (!servers.ContainsKey(authenticationSettings.Id))
            {
                servers.Add(authenticationSettings.Id, authenticationSettings);
            }
        }

        public Task<string> GetToken(string id)
        {
            var success = servers.TryGetValue(id, out AuthenticationSettings settings);

            if (success)
            {
                return GetToken(settings);
            }

            return Task.FromResult(string.Empty);
        }

        private async Task<string> GetToken(AuthenticationSettings settings)
        {
            var success = tokens.TryGetValue(settings.Id, out AuthenticationToken token);

            if (success && !IsTokenExpired(token))
            {
                return token.AccessToken;
            }

            await GetAccessTokenAndAddToStore(settings);

            return await GetToken(settings);
        }

        private async Task GetAccessTokenAndAddToStore(AuthenticationSettings settings)
        {
            var now = dateTimeProvider.Now();
            var client = GetHttpClient();
            var request = new DiscoveryDocumentRequest
            {
                Address = settings.Address,
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

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                Scope = settings.ClientId,
            });

            var token = new AuthenticationToken
            {
                AccessToken = $"{tokenResponse.TokenType} {tokenResponse.AccessToken}",
                ExporesIn = tokenResponse.ExpiresIn,
                LastRequest = now
            };

            tokens[settings.Id] = token;
        }

        private bool IsTokenExpired(AuthenticationToken token)
        {
            const int fiveMinutes = 5;
            var now = dateTimeProvider.Now();

            return token == null || token.LastRequest.AddSeconds(token.ExporesIn) < now.AddMinutes(fiveMinutes);
        }

        private static HttpClient GetHttpClient()
        {
            if (httpClient == null)
            {
                lock (lockObject)
                {
                    httpClient ??= new HttpClient();
                }
            }

            return httpClient;
        }
    }
}
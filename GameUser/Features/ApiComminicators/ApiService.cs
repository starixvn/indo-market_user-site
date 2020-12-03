using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;

namespace GameUser.Features.ApiComminicators
{
    public interface IApiService
    {
        Task<ApiResponse<T>> Post<T>(string endpoint, ApiRequest request) where T : ApiResult, new();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient httpClient;

        public ApiService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ApiResponse<T>> Post<T>(string endpoint, ApiRequest request) where T : ApiResult, new()
        {
            var response = await Post(endpoint, request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                return new ApiResponse<T>
                {
                    Result = JsonConvert.DeserializeObject<T>(responseContent)
                };
            }

            var failureResponse = await CreateErrorResponse<T>(response);

            SentrySdk.CaptureMessage($"Request to API {endpoint} failed at {DateTimeOffset.UtcNow} - {failureResponse.Result.Message}", SentryLevel.Warning);

            return failureResponse;
        }

        private Task<HttpResponseMessage> Post(string endpoint, ApiRequest request)
        {
            const string mediaType = "application/json";

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, mediaType);

            return httpClient.PostAsync(endpoint, content);
        }

        private static async Task<ApiResponse<T>> CreateErrorResponse<T>(HttpResponseMessage response) where T : ApiResult, new()
            => new ApiResponse<T>
            {
                IsSuccessful = false,
                Result = new T
                {
                    IsSuccessful = false,
                    Message = $"{response.StatusCode}={response.ReasonPhrase}-{await response.Content.ReadAsStringAsync()}"
                }
            };
    }
}
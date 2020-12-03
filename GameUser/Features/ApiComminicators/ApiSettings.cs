using System;

namespace GameUser.Features.ApiComminicators
{
    public class ApiSettings
    {
        public Uri BaseUrl { get; set; }

        public ApiEndpoints Endpoints { get; set; }
    }

    public class ApiEndpoints
    {
        public string VerifyToken { get; set; }
    }
}
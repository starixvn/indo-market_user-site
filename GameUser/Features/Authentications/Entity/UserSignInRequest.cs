using GameUser.Features.ApiComminicators;

namespace GameUser.Features.Authentications.Entity
{
    public class UserSignInRequest : ApiRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Token { get; set; }

        public string Ip { get; set; }
    }
}
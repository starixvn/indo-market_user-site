namespace GameUser.Features.Authentications.Models
{
    public class RefreshSessionRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Session { get; set; }
    }
}
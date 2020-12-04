namespace GameUser.Features.Authentications.Entity
{
    public class RefreshSessionRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Session { get; set; }
    }
}
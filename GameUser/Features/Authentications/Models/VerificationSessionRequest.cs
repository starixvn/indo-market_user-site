namespace GameUser.Features.Authentications.Models
{
    public class VerificationSessionRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Session { get; set; }
    }
}
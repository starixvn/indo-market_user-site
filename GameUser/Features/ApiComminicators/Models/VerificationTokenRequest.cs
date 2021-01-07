namespace GameUser.Features.ApiComminicators.Models
{
    public class VerificationTokenRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Token { get; set; }

        public string Ip { get; set; }
    }
}
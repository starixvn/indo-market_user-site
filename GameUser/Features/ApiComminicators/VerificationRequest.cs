namespace GameUser.Features.ApiComminicators
{
    public class VerificationRequest
    {
        public string BrowserUserAgent { get; set; }

        public string Token { get; set; }

        public string IP { get; set; }
    }
}
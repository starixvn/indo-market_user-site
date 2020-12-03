namespace GameUser.Features.ApiComminicators
{
    public class VerificationResponse: ApiResult
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; }

        public string SiteId { get; set; }

        public string ClientId { get; set; }

        public string Domain { get; set; }

        public string Language { get; set; }

        public string Currency { get; set; }

        public string MemberKey { get; set; }
    }
}
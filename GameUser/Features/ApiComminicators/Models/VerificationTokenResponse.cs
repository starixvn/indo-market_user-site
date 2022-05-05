namespace GameUser.Features.ApiComminicators.Models
{
    public class VerificationTokenResponse
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; }

        public string ClientId { get; set; }

        public string Domain { get; set; }

        public string Language { get; set; }

        public string Currency { get; set; }

        public string MemberKey { get; set; }

        public string Seq { get; set; }

        public string SiteId { get; set; }

        public bool IsSuccessful { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public bool IsCash { get; set; }

        public string Market { get; set; }

        public string SiteName { get; set; }

        public string BrandName { get; set; }

        public byte System { get; set; }

        public string OperatorToken { get; set; }
    }
}
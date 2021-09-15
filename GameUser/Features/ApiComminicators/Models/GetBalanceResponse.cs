namespace GameUser.Features.ApiComminicators.Models
{
    public class GetBalanceResponse 
    {
        public string Seq { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDescription { get; set; }

        public bool IsSuccessful() => ErrorCode == 0;

        public decimal Balance { get; set; }
    }
}

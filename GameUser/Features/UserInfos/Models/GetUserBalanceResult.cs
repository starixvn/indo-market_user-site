namespace GameUser.Features.UserInfos.Models
{
    public class GetUserBalanceResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string Message { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; }

        public GetUserBalanceResult(decimal balance, string currency)
        {
            IsSuccessful = true;

            Balance = balance;

            Currency = currency;
    }

        public GetUserBalanceResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }
    }
}
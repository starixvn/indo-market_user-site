namespace GameUser.Features.UserInfos.Models
{
    public class GetUserBalanceResult
    {
        public bool IsSuccessful { get; }

        public string Message { get; }

        public decimal Balance { get; }

        public string Currency { get; }

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
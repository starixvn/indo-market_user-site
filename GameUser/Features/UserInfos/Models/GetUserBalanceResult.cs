namespace GameUser.Features.UserInfos.Models
{
    public class GetUserBalanceResult
    {
        public bool IsSuccessful { get; } = true;

        public string Message { get; }

        public decimal Balance { get; }

        public string Currency { get; }

        public GetUserBalanceResult(decimal balance, string currency)
        {
            Balance = balance;

            Currency = currency;
        }

        public GetUserBalanceResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }

        public GetUserBalanceResult(string message, string currency)
        {
            IsSuccessful = false;

            Message = message;

            Currency = currency;
        }
    }
}
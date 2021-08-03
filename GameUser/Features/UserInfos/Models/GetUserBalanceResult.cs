namespace GameUser.Features.UserInfos.Models
{
    public class GetUserBalanceResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string Message { get; set; }

        public decimal Balance { get; set; }

        public GetUserBalanceResult(decimal balance)
        {
            IsSuccessful = true;


            Balance = balance;
        }

        public GetUserBalanceResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }
    }
}
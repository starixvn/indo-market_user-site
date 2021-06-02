namespace GameUser.Features.UserInfos.Models
{
    public class CreateUserInfoResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string Message { get; set; }

        public CreateUserInfoResult()
        {
        }

        public CreateUserInfoResult(string message)
        {
            IsSuccessful = false;
            Message = message;
        }
    }
}
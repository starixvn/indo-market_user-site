namespace GameUser.Features.UserInfos.Models
{
    public class ChangeUserAvatarResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string Message { get; set; }

        public ChangeUserAvatarResult()
        {
        }

        public ChangeUserAvatarResult(string message)
        {
            IsSuccessful = false;
            Message = message;
        }
    }
}
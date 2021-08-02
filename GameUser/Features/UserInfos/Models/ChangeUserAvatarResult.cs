namespace GameUser.Features.UserInfos.Models
{
    public class ChangeUserAvatarResult
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }

        public int AvatarId { get; set; }

        public ChangeUserAvatarResult(int avatarId)
        {
            IsSuccessful = true;

            AvatarId = avatarId;
        }

        public ChangeUserAvatarResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }
    }
}
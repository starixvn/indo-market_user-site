namespace GameUser.Features.UserInfos.Models
{
    public class ChangeUserAvatarResult
    {
        public bool IsSuccessful { get; }

        public string Message { get; }

        public int AvatarId { get; }

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
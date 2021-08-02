namespace GameUser.Features.UserInfos.Models
{
    public class ChangeUserAvatarResult
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }

        public int SavedAvatarId { get; set; }

        public ChangeUserAvatarResult(int savedAvatarId)
        {
            IsSuccessful = true;
            SavedAvatarId = savedAvatarId;
        }

        public ChangeUserAvatarResult(string message)
        {
            IsSuccessful = false;
            Message = message;
        }
    }
}
namespace GameUser.Features.UserInfos.Models
{
    public class GetUserInfoLobbyResult
    {
        public bool IsSuccessful { get; } = true;

        public string Message { get; }

        public int AvatarId { get; }

        public string Nickname { get; }

        public string Language { get; }

        public bool IsCash { get; }

        public GetUserInfoLobbyResult(
            int avatarId, string nickName, string language, bool isCash)
        {
            AvatarId = avatarId;

            Nickname = nickName;

            Language = language;

            IsCash = isCash;
        }

        public GetUserInfoLobbyResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }
    }
}
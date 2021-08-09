namespace GameUser.Features.UserInfos.Models
{
    public class GetUserInfoLobbyResult
    {
        public bool IsSuccessful { get; }

        public string Message { get; }

        public int AvatarId { get; }

        public string Nickname { get; }

        public GetUserInfoLobbyResult(int avatarId, string nickName)
        {
            IsSuccessful = true;

            AvatarId = avatarId;

            Nickname = nickName;
        }

        public GetUserInfoLobbyResult(string message)
        {
            IsSuccessful = false;

            Message = message;
        }
    }
}
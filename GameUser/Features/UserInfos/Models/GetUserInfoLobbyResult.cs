namespace GameUser.Features.UserInfos.Models
{
    public class GetUserInfoLobbyResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string Message { get; set; }

        public int AvatarId { get; set; }

        public string Nickname { get; set; } 

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

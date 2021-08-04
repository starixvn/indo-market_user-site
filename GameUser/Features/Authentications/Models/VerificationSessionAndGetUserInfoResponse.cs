using GameUser.Features.UserInfos;

namespace GameUser.Features.Authentications.Models
{
    public class VerificationSessionAndGetUserInfoResponse : VerificationSessionResult
    {
        public UserInfoEntity UserInfo { get; set; }
    }
}

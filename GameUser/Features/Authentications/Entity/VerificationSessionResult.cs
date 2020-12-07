using GameUser.Features.UserSessions;

namespace GameUser.Features.Authentications.Entity
{
    public class VerificationSessionResult
    {
        public bool IsSuccessful { get; set; }

        public UserSessionEntity Session { get; set; }
    }
}
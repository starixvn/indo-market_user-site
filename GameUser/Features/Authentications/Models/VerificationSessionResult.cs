using GameUser.Features.UserSessions;

namespace GameUser.Features.Authentications.Models
{
    public class VerificationSessionResult
    {
        public bool IsSuccessful { get; set; }

        public UserSessionEntity Session { get; set; }
    }
}
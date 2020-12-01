using GameUser.Shared.Database;
using System.Threading.Tasks;

namespace GameUser.Features.UserSessions
{
    public interface IUserSessionService
    {
        Task<UserSessionEntity> GetUserSession(string sessionId);

        Task<UserInfoEntity> GetUserInfo(string sessionId);

        Task<UserInfoEntity> GetUserInfo(UserSessionEntity userSession);

        Task<UserSessionEntity> GetUserSessionByToken(string token);

        Task CreateUserSession(UserSessionEntity userSession);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly IGenericRepository<UserSessionEntity> userSessionRepository;
        private readonly IGenericRepository<UserInfoEntity> userInfoRepository;

        public UserSessionService(
            IGenericRepository<UserSessionEntity> userSessionRepository,
            IGenericRepository<UserInfoEntity> userInfoRepository)
        {
            this.userSessionRepository = userSessionRepository;
            this.userInfoRepository = userInfoRepository;
        }

        public async Task CreateUserSession(UserSessionEntity userSession)
        {
            await this.userSessionRepository.InsertOneAsync(userSession);
        }

        public async Task<UserInfoEntity> GetUserInfo(string sessionId)
        {
            var userSession = await GetUserSession(sessionId);
            var userInfo = await userInfoRepository.FindOneAsync(x => x.UserId == userSession.UserId);
            return userInfo;
        }

        public async Task<UserInfoEntity> GetUserInfo(UserSessionEntity userSession)
        {
            return await userInfoRepository.FindOneAsync(x => x.UserId == userSession.UserId);
        }

        public async Task<UserSessionEntity> GetUserSession(string sessionId)
        {
            return await this.userSessionRepository.FindOneAsync(x => x.SessionId == sessionId);
        }

        public async Task<UserSessionEntity> GetUserSessionByToken(string token)
        {
            return await this.userSessionRepository.FindOneAsync(x => x.SSOToken == token);
        }
    }
}

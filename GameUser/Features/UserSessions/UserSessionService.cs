using System;
using System.Threading.Tasks;
using GameUser.Shared.Database;
using MongoDB.Driver;

namespace GameUser.Features.UserSessions
{
    public interface IUserSessionService
    {
        Task<UserSessionEntity> GetUserSession(string sessionId);

        Task<UserInfoEntity> GetUserInfo(string sessionId);

        Task<UserInfoEntity> GetUserInfo(UserSessionEntity userSession);

        Task<UserSessionEntity> GetUserSessionByToken(string token);

        Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession);

        Task ReCheckIn(string sessionId, DateTime time);
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

        public async Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession)
        {
            await userSessionRepository.InsertOneAsync(userSession);

            var newUserSession = await GetUserSession(userSession.SessionId);

            return newUserSession;
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
            return await userSessionRepository.FindOneAsync(x => x.SessionId == sessionId);
        }

        public async Task<UserSessionEntity> GetUserSessionByToken(string token)
        {
            return await userSessionRepository.FindOneAsync(x => x.SSOToken == token);
        }

        public async Task ReCheckIn(string sessionId, DateTime time)
        {
            var update = Builders<UserSessionEntity>.Update
                .Set(session => session.ReCheckInTime, time);

            await userSessionRepository.UpdateOneAsync(session => session.SessionId == sessionId, update);
        }
    }
}
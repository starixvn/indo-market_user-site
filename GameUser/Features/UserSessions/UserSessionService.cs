using GameUser.Shared.Database;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace GameUser.Features.UserSessions
{
    public interface IUserSessionService
    {
        Task<UserSessionEntity> GetUserSession(string sessionId);

        Task<UserSessionEntity> GetUserSessionByToken(string token);

        Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession);

        Task ReCheckIn(string sessionId, DateTime time);

        bool IsExpiredSession(UserSessionEntity userSession);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly IGenericRepository<UserSessionEntity> userSessionRepository;
        private readonly UserSessionSettings sessionSettings;
        private readonly IMemoryCache memoryCache;

        public UserSessionService(
            IMemoryCache memoryCache,
            IGenericRepository<UserSessionEntity> userSessionRepository,
            UserSessionSettings userSessionSettings)
        {
            this.userSessionRepository = userSessionRepository;
            this.memoryCache = memoryCache;
            this.sessionSettings = userSessionSettings;
        }

        public async Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession)
        {
            await userSessionRepository.InsertOneAsync(userSession);

            var newUserSession = await GetUserSession(userSession.SessionId);

            return newUserSession;
        }

        public async Task<UserSessionEntity> GetUserSession(string sessionId)
        {
            var session = await memoryCache.GetOrCreateAsync(sessionId, async cache =>
            {
                var currentSession = await GetUserSessionFromDB(sessionId);

                if (IsExpiredSession(currentSession))
                {
                    cache.Dispose();
                }
                else
                {
                    var timeout = currentSession.ReCheckInTime.AddMinutes(sessionSettings.SessionDuration);
                    var relative = timeout.Subtract(currentSession.ReCheckInTime);

                    cache.SetAbsoluteExpiration(relative);
                }

                return currentSession;

            });

            return session;
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

        public bool IsExpiredSession(UserSessionEntity userSession)
        {
            if (userSession != null)
            {
                var isExpired = userSession.ReCheckInTime.AddMinutes(sessionSettings.SessionDuration) < DateTime.UtcNow;

                return isExpired;
            }

            return true;
        }

        private Task<UserSessionEntity> GetUserSessionFromDB(string sessionId) => userSessionRepository.FindOneAsync(x => x.SessionId == sessionId);
    }
}
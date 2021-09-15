using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

[assembly: InternalsVisibleTo("GameUser.Tests")]

namespace GameUser.Features.UserSessions
{
    public interface IUserSessionService
    {
        Task<UserSessionEntity> GetUserSession(string sessionId);

        Task<UserSessionEntity> GetUserSessionByToken(string token);

        Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession);

        Task<UserSessionEntity> ReCheckIn(string sessionId, DateTime time);

        bool IsExpiredSession(UserSessionEntity userSession);

        Task<string> ChangeSessionId(UserSessionEntity session);

        Task UpdateSessionLanguage(string sessionId, string language);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly IGenericRepository<UserSessionEntity> userSessionRepository;
        private readonly UserSessionSettings sessionSettings;
        private readonly IMemoryCache memoryCache;
        private readonly IDateTimeProvider dateTimeProvider;

        public UserSessionService(
            IMemoryCache memoryCache,
            IGenericRepository<UserSessionEntity> userSessionRepository,
            UserSessionSettings userSessionSettings,
            IDateTimeProvider dateTimeProvider)
        {
            this.userSessionRepository = userSessionRepository;
            this.memoryCache = memoryCache;
            this.sessionSettings = userSessionSettings;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<UserSessionEntity> CreateUserSession(UserSessionEntity userSession)
        {
            await userSessionRepository.InsertOneAsync(userSession);

            var newUserSession = await GetUserSession(userSession.SessionId);

            return newUserSession;
        }

        public async Task<UserSessionEntity> GetUserSession(string sessionId)
        {
            var session = await memoryCache.GetOrCreateAsync(sessionId, SessionCacheFactory);

            return session;
        }

        public async Task<UserSessionEntity> GetUserSessionByToken(string token)
        {
            return await userSessionRepository.FindOneAsync(x => x.SSOToken == token);
        }

        public async Task<UserSessionEntity> ReCheckIn(string sessionId, DateTime time)
        {
            var update = Builders<UserSessionEntity>.Update
                .Set(session => session.ReCheckInTime, time);

            await userSessionRepository.UpdateOneAsync(session => session.SessionId == sessionId, update);
            memoryCache.Remove(sessionId);

            return await GetUserSession(sessionId);
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

        private void RemoveOldSession(IEnumerable<string> ids)
        {
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    memoryCache.Remove(id);
                }
            }
        }

        public async Task<string> ChangeSessionId(UserSessionEntity session)
        {
            if (!IsExpiredSession(session))
            {
                var newSessionId = Guid.NewGuid().ToString();
                var oldIds = session.OldSessionIds != null ? session.OldSessionIds.ToList() : new List<string>();

                oldIds.Add(session.SessionId);

                var update = Builders<UserSessionEntity>.Update.Set(session => session.OldSessionIds, oldIds).Set(session => session.SessionId, newSessionId);

                await UpdateUserSession(session.SessionId, update);

                return newSessionId;
            }

            return string.Empty;
        }

        public async Task UpdateSessionLanguage(string sessionId, string language)
        {
            var update = Builders<UserSessionEntity>.Update.Set(session => session.Language, language);

            await UpdateUserSession(sessionId, update);
        }

        protected async Task<UserSessionEntity> UpdateUserSession(string sessionId, UpdateDefinition<UserSessionEntity> updater)
        {
            await userSessionRepository.UpdateOneAsync(session => session.SessionId == sessionId, updater);

            var session = await GetUserSessionFromDB(sessionId);

            return session;
        }

        internal async Task<UserSessionEntity> SessionCacheFactory(ICacheEntry cache)
        {
            var currentSession = await GetUserSessionFromDB(cache.Key.ToString());

            if (IsExpiredSession(currentSession))
            {
                cache.Dispose();
            }
            else
            {
                var timeout = currentSession.ReCheckInTime.AddMinutes(sessionSettings.SessionDuration);
                var relative = timeout.Subtract(dateTimeProvider.UtcNow());

                RemoveOldSession(currentSession.OldSessionIds);

                cache.SetAbsoluteExpiration(relative);
            }

            return currentSession;
        }
    }
}
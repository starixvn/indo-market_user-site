using GameUser.Shared.Repositories;
using System.Threading.Tasks;

namespace GameUser.Features.UserSessions
{
    public interface IUserSessionService
    {
        Task<UserSessionEntity> GetUserSession(string sessionId);
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly IGenericRepository<UserSessionEntity> userSessionRepository;

        public UserSessionService(IGenericRepository<UserSessionEntity> userSessionRepository)
        {
            this.userSessionRepository = userSessionRepository;
        }

        public async Task<UserSessionEntity> GetUserSession(string sessionId)
        {
            return await this.userSessionRepository.FindOneAsync(x => x.SessionId == sessionId);
        }
    }
}

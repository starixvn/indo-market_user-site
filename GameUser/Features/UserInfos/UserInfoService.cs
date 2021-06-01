using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using System;
using System.Threading.Tasks;

namespace GameUser.Features.UserInfos
{
    public interface IUserInfoService
    {
        Task<UserInfoEntity> GetUserInfo(string userId);

        Task<bool> IsUserExisted(string sessionId);

        Task<UserInfoEntity> CreateUserInfo(string sessionId, string nickname);
    }

    public class UserInfoService : IUserInfoService
    {
        private readonly IGenericRepository<UserInfoEntity> repository;
        private readonly IUserSessionService userSessionService;
        private readonly IUserService userService;
        private readonly IDateTimeProvider dateTimeProvider;

        public UserInfoService(IGenericRepository<UserInfoEntity> repository, IUserSessionService userSessionService, IDateTimeProvider dateTimeProvider, IUserService userService)
        {
            this.repository = repository;
            this.userSessionService = userSessionService;
            this.dateTimeProvider = dateTimeProvider;
            this.userService = userService;
        }

        public async Task<UserInfoEntity> CreateUserInfo(string sessionId, string nickname)
        {
            var session = await userSessionService.GetUserSession(sessionId);
            var localUser = await userService.GetUser(session.UserName);
            var userInfo = new UserInfoEntity
            {
                UID = session.MemberId,
                UserId = session.UserId,
                Name = nickname,
                JoinedOn = dateTimeProvider.Now(),
                Currency = session.Currency,
                AvatarId = GetAvatarId(),
                IsCash = localUser.Local.IsCash,
                BrandName = localUser.Local.BrandName,
                Red = 0,
                Type = false
            };

            await repository.InsertOneAsync(userInfo);

            return userInfo;
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            var userInfo = await repository.FindOneAsync(info => info.UserId == userId);

            return userInfo;
        }

        public async Task<bool> IsUserExisted(string sessionId)
        {
            bool result = false;
            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession != null && !string.IsNullOrEmpty(userSession.UserId))
            {
                var user = await GetUserInfo(userSession.UserId);

                result = user != null;
            }

            return result;
        }


        private int GetAvatarId()
        {
            var random = new Random();

            return random.Next(1, 40);
        }
    }
}
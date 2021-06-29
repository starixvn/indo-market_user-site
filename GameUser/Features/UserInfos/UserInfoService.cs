using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameUser.Features.MiddleSite;
using GameUser.Features.UserInfos.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;

[assembly: InternalsVisibleTo("GameUser.Tests")]

namespace GameUser.Features.UserInfos
{
    public interface IUserInfoService
    {
        Task<UserInfoEntity> GetUserInfo(string userId);

        Task<bool> IsUserExisted(string sessionId);

        Task<CreateUserInfoResult> CreateUserInfo(string sessionId, string nickname);
    }

    public class UserInfoService : IUserInfoService
    {
        private const int maximumAvatarId = 40;

        private readonly IGenericRepository<UserInfoEntity> repository;
        private readonly IUserSessionService userSessionService;
        private readonly IUserService userService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IMiddleSiteService middleSiteService;
        private readonly IMemoryCache memoryCache;
        private readonly UserInfoSettings settings;
        private readonly Func<int, int, int> randomFunc = DefaultRandomFunc;

        public UserInfoService(
            IMemoryCache memoryCache,
            UserInfoSettings settings,
            IGenericRepository<UserInfoEntity> repository,
            IUserSessionService userSessionService,
            IDateTimeProvider dateTimeProvider,
            IUserService userService,
            IMiddleSiteService middleSiteService)
        {
            this.repository = repository;
            this.userSessionService = userSessionService;
            this.dateTimeProvider = dateTimeProvider;
            this.userService = userService;
            this.middleSiteService = middleSiteService;
            this.memoryCache = memoryCache;
            this.settings = settings;
        }

        internal UserInfoService(
            IMemoryCache memoryCache,
            UserInfoSettings settings,
            IGenericRepository<UserInfoEntity> repository,
            IUserSessionService userSessionService,
            IDateTimeProvider dateTimeProvider,
            IUserService userService,
            IMiddleSiteService middleSiteService,
            Func<int, int, int> randomFunc)
            : this(memoryCache, settings, repository, userSessionService, dateTimeProvider, userService, middleSiteService)
        {
            this.randomFunc = randomFunc;
        }

        private static int DefaultRandomFunc(int min, int max)
            => new Random().Next(min, max);

        public async Task<CreateUserInfoResult> CreateUserInfo(string sessionId, string nickname)
        {
            var session = await userSessionService.GetUserSession(sessionId);
            var errorMessage = await ValidateNickname(nickname, session.UserName);

            if (string.IsNullOrEmpty(errorMessage))
            {
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

                return new CreateUserInfoResult();
            }

            return new CreateUserInfoResult(errorMessage);
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            var userInfo = await memoryCache.GetOrCreateAsync(userId, UserInfoCacheFactory);

            return userInfo;
        }

        internal async Task<UserInfoEntity> UserInfoCacheFactory(ICacheEntry cache)
        {
            var user = await GetUserInfoFromDB(cache.Key.ToString());

            if (user == null)
            {
                cache.Dispose();
            }
            else
            {
                cache.SetAbsoluteExpiration(TimeSpan.FromMinutes(settings.CacheTimeout));
            }

            return user;
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

        private Task<UserInfoEntity> GetUserInfoFromDB(string userId) => repository.FindOneAsync(info => info.UserId == userId);

        private int GetAvatarId() => randomFunc(1, maximumAvatarId);

        internal async Task<string> ValidateNickname(string nickname, string username)
        {
            const string maxCharacterNameKey = "maxCharacterName";
            const string cannotTheSameAsUsernameKey = "cannotTheSameAsUsername";
            const string noSpecialCharacterKey = "noSpecialCharacter";
            const string characterNameExistedKey = "characterNameExisted";
            const string pattern = @"^([a-zA-Z0-9]){3,14}$";
            const int minimumNicknameLength = 3;
            const int maximumNicknameLength = 14;

            if (string.IsNullOrEmpty(nickname) || nickname.Length < minimumNicknameLength || nickname.Length > maximumNicknameLength)
            {
                return maxCharacterNameKey;
            }
            else if (nickname.Equals(username))
            {
                return cannotTheSameAsUsernameKey;
            }
            else if (!Regex.IsMatch(nickname, pattern))
            {
                return noSpecialCharacterKey;
            }
            else
            {
                var isNicknameExisted = await middleSiteService.IsNicknameExisted(nickname);

                return isNicknameExisted ? characterNameExistedKey : string.Empty;
            }
        }
    }
}
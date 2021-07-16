using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.MiddleSite;
using GameUser.Features.UserInfos.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Database;
using GameUser.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;

[assembly: InternalsVisibleTo("GameUser.Tests")]

namespace GameUser.Features.UserInfos
{
    public interface IUserInfoService
    {
        Task<UserInfoEntity> GetUserInfo(string userId);

        Task<bool> IsUserExisted(string sessionId);

        Task<bool> CheckUserExistsAndEnterPortal(string sessionId);

        Task<CreateUserInfoResult> CreateUserInfo(string sessionId, string nickname);
    }

    public class UserInfoService : IUserInfoService
    {
        private const int maximumAvatarId = 40;

        private readonly IApi api;
        private readonly IGenericRepository<UserInfoEntity> repository;
        private readonly IUserSessionService userSessionService;
        private readonly IUserService userService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IMiddleSiteService middleSiteService;
        private readonly IMemoryCache memoryCache;
        private readonly UserInfoSettings settings;
        private readonly Func<int, int, int> randomFunc = DefaultRandomFunc;
        private readonly ISentryClient sentryClient;

        public UserInfoService(
            IApi api,
            IMemoryCache memoryCache,
            UserInfoSettings settings,
            IGenericRepository<UserInfoEntity> repository,
            IUserSessionService userSessionService,
            IDateTimeProvider dateTimeProvider,
            IUserService userService,
            IMiddleSiteService middleSiteService,
            ISentryClient sentryClient)
        {
            this.api = api;
            this.repository = repository;
            this.userSessionService = userSessionService;
            this.dateTimeProvider = dateTimeProvider;
            this.userService = userService;
            this.middleSiteService = middleSiteService;
            this.memoryCache = memoryCache;
            this.settings = settings;
            this.sentryClient = sentryClient;
        }

        internal UserInfoService(
            IApi api,
            IMemoryCache memoryCache,
            UserInfoSettings settings,
            IGenericRepository<UserInfoEntity> repository,
            IUserSessionService userSessionService,
            IDateTimeProvider dateTimeProvider,
            IUserService userService,
            IMiddleSiteService middleSiteService,
            Func<int, int, int> randomFunc,
            ISentryClient sentryClient)
            : this(api, memoryCache, settings, repository, userSessionService, dateTimeProvider, userService, middleSiteService, sentryClient)
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
                await EnterPortal(session, nickname, true);

                return new CreateUserInfoResult();
            }

            return new CreateUserInfoResult(errorMessage);
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            if(settings.CacheTimeout > 0)
            {
                return await memoryCache.GetOrCreateAsync(userId, UserInfoCacheFactory);
            }    

            return await GetUserInfoFromDB(userId); ;
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

        public async Task<bool> CheckUserExistsAndEnterPortal(string sessionId)
        {
            var userExists = await IsUserExisted(sessionId);

            if (userExists)
            {
                var userSession = await userSessionService.GetUserSession(sessionId);
                var user = await GetUserInfo(userSession.UserId);

                await EnterPortal(userSession, user.Name, false);
            }

            return userExists;
        }

        protected async Task<bool> EnterPortal(UserSessionEntity userSession, string nickname, bool firstLogin)
        {
            var result = false;
            var request = new EnterPortalRequest
            {
                CharacterName = nickname,
                FirstLogin = firstLogin,
                ObCustId = userSession.MemberId,
                Seq = userSession.OWSeq,
                SiteId = userSession.SiteId
            };

            try
            {
                var response = await api.EnterPortal(request);

                result = response.Result != null && response.Result.IsSuccessful();

                if (!result)
                {
                    var message = $"Notify Login Failure at ${DateTime.Now}\n\t" +
                       $"Session: ${JsonConvert.SerializeObject(userSession)}\n\t" +
                       $"Request: ${JsonConvert.SerializeObject(request)}\n\t" +
                       $"Response: ${JsonConvert.SerializeObject(response)}";

                    sentryClient.CaptureMessage(message, SentryLevel.Error);
                }
                
            }
            catch (Exception exception)
            {
                var message = $"Notify Login Error at ${DateTime.Now}\n\t" +
                    $"Session: ${JsonConvert.SerializeObject(userSession)}\n\t" +
                    $"Request: ${JsonConvert.SerializeObject(request)}\n\t" +
                    $"Error: ${JsonConvert.SerializeObject(exception)}";

                sentryClient.CaptureMessage(message, SentryLevel.Error);
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
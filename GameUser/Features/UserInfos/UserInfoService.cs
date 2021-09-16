using System;
using System.Linq;
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
using MongoDB.Driver;
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

        Task<ChangeUserAvatarResult> UpdateUserAvatar(string sessionId, int avatarId);

        Task<GetUserBalanceResult> GetUserBalance(string sessionId);

        Task<GetUserInfoLobbyResult> GetUserInfoLobby(string sessionId);
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
                var formatedNickname = FormatNickname(localUser.Local.Currency ?? string.Empty, nickname);
                var latestUID = await GetLatestUID();
                var userInfo = new UserInfoEntity
                {
                    UID = latestUID + 1,
                    UserId = session.UserId,
                    Name = formatedNickname,
                    JoinedOn = dateTimeProvider.Now(),
                    Currency = localUser.Local.Currency,
                    AvatarId = GetAvatarId(),
                    IsCash = localUser.Local.IsCash,
                    BrandName = localUser.Local.BrandName,
                    Red = 0,
                    Type = false
                };

                await repository.InsertOneAsync(userInfo);
                await EnterPortal(session, formatedNickname, true);

                return new CreateUserInfoResult();
            }

            return new CreateUserInfoResult(errorMessage);
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            if (settings.CacheTimeout > 0)
            {
                return await memoryCache.GetOrCreateAsync(userId, UserInfoCacheFactory);
            }

            return await GetUserInfoFromDB(userId);
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

        public async Task<ChangeUserAvatarResult> UpdateUserAvatar(string sessionId, int avatarId)
        {
            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession != null)
            {
                await UpdateAvatarIdToDatabase(avatarId, userSession.UserId);

                return new ChangeUserAvatarResult(avatarId);
            }

            return new ChangeUserAvatarResult("Error changing avatar");
        }

        public async Task<GetUserBalanceResult> GetUserBalance(string sessionId)
        {
            const string getBalanceFailedMessage =
                @"Cannot update your account balance. Error Code: SB1902";

            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession != null)
            {
                var queriedUser = await GetUserInfo(userSession.UserId);
                var currency = queriedUser.Currency;
                var response = await GetBalance(userSession, queriedUser.Name);
                var callApiSuccessful = response.Result != null;

                if (callApiSuccessful)
                {
                    var apiErrorMessage = response.Result.ErrorMessage;
                    var apiErrorCode = response.Result.ErrorCode;
                    var balance = response.Result.Balance;

                    if (apiErrorCode != 0 && apiErrorMessage != null)
                    {
                        var clientErrorMessage = ComposeGetBalanceErrorMessage
                            (apiErrorCode,
                            apiErrorMessage);

                      return new GetUserBalanceResult(clientErrorMessage, currency, (decimal)queriedUser.Red);
                    }

                    UpdateUserBalanceToDatabase((double)balance, queriedUser);

                    return new GetUserBalanceResult(balance, currency);
                }
                return new GetUserBalanceResult(getBalanceFailedMessage, currency, 0);
            }
            return new GetUserBalanceResult("Error retrieving data");
        }

        public async Task<GetUserInfoLobbyResult> GetUserInfoLobby(string sessionId)
        {
            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession != null)
            {
                var queriedUser = await GetUserInfoFromDB(userSession.UserId);

                if (queriedUser != null)
                {
                    return new GetUserInfoLobbyResult(
                        queriedUser.AvatarId, 
                        queriedUser.Name, 
                        userSession.Language,
                        queriedUser.IsCash);
                }
            }
            return new GetUserInfoLobbyResult("Error retrieving data");
        }

        private async Task<int> GetLatestUID()
        {
            var uidAssending = Builders<UserInfoEntity>.Sort.Descending(user => user.UID);
            var findOptions = new FindOptions<UserInfoEntity>
            {
                Sort = uidAssending,
                Limit = 1
            };
            var users = await repository.FindAsync(user => true, findOptions);
            var latestUser = users.FirstOrDefault();

            return latestUser?.UID ?? 0;
        }

        private string FormatNickname(string currency, string nickname)
        {
            var regex = new Regex(settings.CurrencyRegex);

            if (regex.IsMatch(currency))
            {
                return $"{settings.NicknamePrefix}{nickname}";
            }

            return nickname;
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
                    var message = $"Notify Login Failure at {DateTime.Now}\n\t" +
                       $"Session: {JsonConvert.SerializeObject(userSession)}\n\t" +
                       $"Request: {JsonConvert.SerializeObject(request)}\n\t" +
                       $"Response: {JsonConvert.SerializeObject(response)}";

                    sentryClient.CaptureMessage(message, SentryLevel.Error);
                }
            }
            catch (Exception exception)
            {
                var message = $"Notify Login Error at {DateTime.Now}\n\t" +
                    $"Session: {JsonConvert.SerializeObject(userSession)}\n\t" +
                    $"Request: {JsonConvert.SerializeObject(request)}\n\t" +
                    $"Error: {JsonConvert.SerializeObject(exception)}";

                sentryClient.CaptureMessage(message, SentryLevel.Error);
            }

            return result;
        }

        protected async Task<MemberApiResponse<GetBalanceResponse>> GetBalance(
            UserSessionEntity userSession,
            string characterName)
        {
            var response = new MemberApiResponse<GetBalanceResponse>();

            var request = new GetBalanceRequest
            {
                ObCustId = userSession.MemberId,
                SiteId = userSession.SiteId,
                Seq = userSession.OWSeq
            };

            try
            {
                var beforeCallMessage = ComposeBeforeCallGetBalanceMessage(
                    userSession,
                    characterName);

                response = await api.GetBalance(request);
            }
            catch (Exception exception)
            {
                var message = $@"Notify Get Balace Error at {DateTime.Now}
                                Session: {JsonConvert.SerializeObject(userSession)}
                                Request: {JsonConvert.SerializeObject(request)}
                                Error: {JsonConvert.SerializeObject(exception)}";

                sentryClient.CaptureMessage(message, SentryLevel.Error);
            }

            return response;
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

        private string ComposeBeforeCallGetBalanceMessage(
            UserSessionEntity userSession, string characterName)
        {
            var beforeCallMessage = $@"Before Action at {DateTime.Now}
                                     Session: {JsonConvert.SerializeObject(userSession)}
                                     Character Name: {JsonConvert.SerializeObject(characterName)}
                                     Language: {JsonConvert.SerializeObject(userSession.Language)}
                                     ObCustId: {JsonConvert.SerializeObject(userSession.MemberId)}
                                     Seq: {JsonConvert.SerializeObject(userSession.OWSeq)}
                                     SiteId: {JsonConvert.SerializeObject(userSession.SiteId)}";

            return beforeCallMessage;
        }

        private async Task<bool> UpdateAvatarIdToDatabase(int avatarId, string userId)
        {
            try
            {
                var update = Builders<UserInfoEntity>.Update.Set(entity => entity.AvatarId, avatarId);

                await repository.UpdateOneAsync(entity => entity.UserId == userId, update);
            }
            catch (Exception exception)
            {
                var message = $"Notify Update Avatar Error at {DateTime.Now}\n\t" +
                    $"UserID: {JsonConvert.SerializeObject(userId)}\n\t" +
                    $"Error: {JsonConvert.SerializeObject(exception)}";

                sentryClient.CaptureMessage(message, SentryLevel.Error);

                return false;
            }
            return true;
        }
        private void UpdateUserBalanceToDatabase(double balance, UserInfoEntity queriedUser)
        => Task.Run(async () =>
        {
            try
            {
                var update = Builders<UserInfoEntity>.Update.Set(entity => entity.Red, balance);

                await repository.UpdateOneAsync(entity => entity.UserId == queriedUser.UserId, update);
            }
            catch (Exception exception)
            {
                sentryClient.CaptureException(exception);
            }
        });

        private string ComposeGetBalanceErrorMessage(
            int errorCode,
            string errorMessage)
        {
            const int disabledAccountError = 2002;

            const int insufficientFundsError = 3002;

            const string disableAccountMessage =
                @"Account has been closed. Please contact your upline for details, thank you.";

            var insufficientFundsMesssage =
                $@"{JsonConvert.SerializeObject(errorMessage)}.
                ({JsonConvert.SerializeObject(errorCode)})";

            if (errorCode == disabledAccountError)
            {
                return disableAccountMessage;
            }
            else if (errorCode == insufficientFundsError && errorMessage != null)
            {
                return insufficientFundsMesssage;
            }
            else
            {
                return
                $@"{JsonConvert.SerializeObject(errorMessage)}.
                ({JsonConvert.SerializeObject(errorCode)})";
            }
        }
    }
}
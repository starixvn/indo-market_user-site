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

        Task<GetUserBalanceResult> GetUserBalance(string sessionId, bool enterLobby);

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

            var queriedUser = await GetUserInfo(userSession.UserId);

            var changedAvatar = await UpdateAvatarId(avatarId, queriedUser);

            if (!changedAvatar)
            {
                return new ChangeUserAvatarResult("Avatar not updated");
            }

            return new ChangeUserAvatarResult(avatarId);
        }

        public async Task<GetUserBalanceResult> GetUserBalance(string sessionId, bool enterLobby)
        {
            const string getBalanceFailedMessage =
                "apiError" +
                "message: Cannot update your account balance." +
                "Error Code:  SB1902";

            var errorMessage = "";

            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession != null)
            {
                var queriedUser = await GetUserInfo(userSession.UserId);

                var response = await GetBalance(userSession, queriedUser.Name);

                var getBalanceSuccessful = response.Result != null && response.Result.IsSuccessful();

                if (!getBalanceSuccessful)
                {
                    return new GetUserBalanceResult(getBalanceFailedMessage);
                }

                if (response.Result.ErrorCode != 0 && response.Result.ErrorMessage != null)
                {

                    errorMessage = ComposeGetBalanceErrorMessage(response.Result.ErrorCode, response.Result.ErrorMessage);

                    return new GetUserBalanceResult(errorMessage);
                }

                var redUpdated = await UpdateUserRed((double)response.Result.Balance, queriedUser);

                if (!redUpdated)
                {
                    sentryClient.CaptureMessage("User red not updated", SentryLevel.Error);
                }

                return new GetUserBalanceResult(response.Result.Balance, queriedUser.Currency);
            }

            return new GetUserBalanceResult("Error retrieving data");
        }

        private Task<bool> UpdateUserRed(double balance, UserInfoEntity queriedUser)
        => Task.Run(() =>
        {
            try
            {
                var update = Builders<UserInfoEntity>.Update.Set(entity => entity.Red, balance);

                repository.UpdateOneAsync(entity => entity.UserId == queriedUser.UserId, update);
            }
            catch (Exception exception)
            {
                sentryClient.CaptureException(exception);

                return false;
            }

            return true;
        });

        public async Task<GetUserInfoLobbyResult> GetUserInfoLobby(string sessionId)
        {
            var userSession = await userSessionService.GetUserSession(sessionId);

            var queriedUser = await GetUserInfo(userSession.UserId);

            if (queriedUser != null)
            {
                return new GetUserInfoLobbyResult(queriedUser.AvatarId, queriedUser.Name);
            }
            return new GetUserInfoLobbyResult("Error retrieving data");
        }

        private async Task<bool> UpdateAvatarId(int avatarId, UserInfoEntity queriedUser)
        {
            try
            {
                var update = Builders<UserInfoEntity>.Update.Set(entity => entity.AvatarId, avatarId);

                await repository.UpdateOneAsync(entity => entity.UserId == queriedUser.UserId, update);
            }
            catch (Exception exception)
            {
                sentryClient.CaptureException(exception);

                return false;
            }
            return true;
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
            };

            var beforeCallMessage = ComposeBeforeCallGetBalanceMessage(userSession, characterName);

            try
            {
                response = await api.GetBalance(request);

                var afterCallMessage = ComposeAfterCallGetBalanceMessage(
                    userSession,
                    beforeCallMessage,
                    response);

                sentryClient.CaptureMessage(afterCallMessage, SentryLevel.Warning);
            }
            catch (Exception exception)
            {
                var message = $"Notify Get Balace Error at {DateTime.Now}\n\t" +
                    $"Session: {JsonConvert.SerializeObject(userSession)}\n\t" +
                    $"Request: {JsonConvert.SerializeObject(request)}\n\t" +
                    $"Error: {JsonConvert.SerializeObject(exception)}";

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
            var beforeCallMessage = $"Before Action at {DateTime.Now}\n\t" +
                       $"Session: {JsonConvert.SerializeObject(userSession)}\n\t" +
                       $"Character Name: {JsonConvert.SerializeObject(characterName)}\n\t" +
                       $"Language: {JsonConvert.SerializeObject(userSession.Language)}\n\t" +
                       $"ObCustId: {JsonConvert.SerializeObject(userSession.MemberId)}\n\t" +
                       $"Seq: {JsonConvert.SerializeObject(userSession.OWSeq)}\n\t" +
                       $"SiteId: {JsonConvert.SerializeObject(userSession.SiteId)}\n\t"
                       ;

            return beforeCallMessage;
        }

        private string ComposeAfterCallGetBalanceMessage(
            UserSessionEntity userSession,
            string beforeCallMessage,
            MemberApiResponse<GetBalanceResponse> response)
        {
            var afterCallMessage = $"Account {JsonConvert.SerializeObject(userSession.SiteId)}_" +
                       $"{JsonConvert.SerializeObject(userSession.MemberId)}" +
                       $"performs Get Balance at {DateTime.Now}\n\t" +
                       beforeCallMessage +
                       $"Action Result: {JsonConvert.SerializeObject(response.Result)}\n\t"
                       ;

            return afterCallMessage;
        }

        private string ComposeGetBalanceErrorMessage(
            int errorCode,
            string errorMessage)
        {
            const string disableAccountMessage = "Account has been closed. " +
                            "Please contact your upline for details, thank you.";

            const int insufficientFundsError = 3002;

            const int disabledAccountError = 2002;

            var insufficientFundsMesssage =
                $"apiError" +
                $"message: { JsonConvert.SerializeObject(errorMessage) }" +
                $"Error Code:  { JsonConvert.SerializeObject(errorCode) }";

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
                return string.Empty;
            }
        }
    }
}
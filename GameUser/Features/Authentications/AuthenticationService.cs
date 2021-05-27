using System;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Constants;
using GameUser.Shared.Wrappers;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;

namespace GameUser.Features.Authentications
{
    public interface IAuthenticationService
    {
        Task<UserSignInResponse> SignIn(VerificationTokenRequest request);

        Task<RefreshSessionResponse> Refresh(RefreshSessionRequest request);

        Task<VerificationSessionResult> Verify(VerificationSessionRequest request);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private const string UUSCurrencyName = "UUS";
        private static readonly int sessionTimeout = 20;
        private readonly IApi api;
        private readonly IUserService userService;
        private readonly IUserSessionService userSessionService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ICultureConfigs cultureConfigs;
        private readonly ISentryClient sentryClient;

        public AuthenticationService(
            IApi api, 
            IUserService userService, 
            IUserSessionService userSessionService, 
            IDateTimeProvider dateTimeProvider,
            ICultureConfigs cultureConfigs,
            ISentryClient sentryClient)
        {
            this.api = api;
            this.userService = userService;
            this.userSessionService = userSessionService;
            this.dateTimeProvider = dateTimeProvider;
            this.cultureConfigs = cultureConfigs;
            this.sentryClient = sentryClient;
        }

        public async Task<UserSignInResponse> SignIn(VerificationTokenRequest request)
        {
            var verificationResult = await api.VerifyToken(request);

            if (verificationResult.IsSuccessful)
            {
                var user = await CreateUserIfNotExist(verificationResult);
                var machedBrandName = string.Equals(verificationResult.BrandName, user.Local.BrandName, StringComparison.OrdinalIgnoreCase);
                var brandNameIsEmpty = string.IsNullOrEmpty(user.Local.BrandName);

                if (machedBrandName || brandNameIsEmpty)
                {
                    if (brandNameIsEmpty)
                    {
                        await userService.UpdateBrandName(user.Local.UserName, verificationResult.BrandName);
                    }

                    var userSession = await AddSessionIfNotExist(user, verificationResult, request);
                    var expiredIn = userSession.ReCheckInTime.AddMinutes(sessionTimeout);
                    var language = ConvertLanguage(userSession.Language);

                    return new UserSignInResponse(true, userSession.UserName, language, userSession.SessionId, userSession.Currency, expiredIn);
                } else
                {
                    sentryClient.CaptureMessage($"Brand verification failed on {DateTime.UtcNow}. User: {JsonConvert.SerializeObject(user.Local)}. SSO Brand Name: {verificationResult.BrandName}", SentryLevel.Error);
                }
            }

            return new UserSignInResponse();
        }

        public async Task<RefreshSessionResponse> Refresh(RefreshSessionRequest request)
        {
            var userSession = await userSessionService.GetUserSession(request.Session);
            var isValidRequest = userSession != null && Equals(request.BrowserUserAgent, userSession.BrowserUserAgent);

            if (isValidRequest && !IsExpiredSession(userSession))
            {
                var now = dateTimeProvider.Now();

                await userSessionService.ReCheckIn(request.Session, now);

                var expiredIn = now.AddMinutes(sessionTimeout);

                return new RefreshSessionResponse(true, expiredIn);
            }

            return new RefreshSessionResponse();
        }

        public async Task<VerificationSessionResult> Verify(VerificationSessionRequest request)
        {
            var userSession = await userSessionService.GetUserSession(request.Session);
            var isValidRequest = userSession != null && Equals(request.BrowserUserAgent, userSession.BrowserUserAgent);

            return !isValidRequest || IsExpiredSession(userSession) ? new VerificationSessionResult
            {
                IsSuccessful = false
            } : new VerificationSessionResult
            {
                IsSuccessful = true,
                Session = userSession
            };
        }

        private async Task<UserEntity> CreateUserIfNotExist(VerificationTokenResponse verifyTokenResult)
        {
            var user = await userService.GetUser(verifyTokenResult.MemberKey.ToLower());

            if (user == null)
            {
                var newUser = new UserEntity
                {
                    Local = new LocalUserEntity
                    {
                        ClientId = verifyTokenResult.ClientId,
                        Currency = verifyTokenResult.Currency,
                        MemberId = verifyTokenResult.MemberId,
                        MemberName = verifyTokenResult.MemberName,
                        UserName = verifyTokenResult.MemberKey.ToLower(),
                        RegDate = dateTimeProvider.Now(),
                        Password = Guid.NewGuid().ToString(),
                        IsCash = verifyTokenResult.IsCash,
                        BrandName = verifyTokenResult.BrandName,
                        SiteName = verifyTokenResult.SiteName
                    }
                };

                user = await userService.CreateUser(newUser);
            }

            return user;
        }

        private async Task<UserSessionEntity> AddSessionIfNotExist(UserEntity user, VerificationTokenResponse verifyTokenResult, VerificationTokenRequest request)
        {
            var session = await userSessionService.GetUserSessionByToken(request.Token);
            var currency = GetMemberCurrency(verifyTokenResult);

            if (session == null)
            {
                var sessionId = $"{Guid.NewGuid()}";
                var now = dateTimeProvider.Now();
                var userSession = new UserSessionEntity
                {
                    BrowserUserAgent = request.BrowserUserAgent,
                    ClientId = verifyTokenResult.ClientId,
                    Currency = currency,
                    Ip = request.Ip,
                    Language = ConvertLanguage(verifyTokenResult.Language),
                    MemberId = verifyTokenResult.MemberId,
                    MemberName = verifyTokenResult.MemberName,
                    OWSeq = verifyTokenResult.Seq,
                    ReCheckInTime = now,
                    SiteId = verifyTokenResult.SiteId,
                    SSOTime = now,
                    SSOToken = request.Token,
                    UserId = user.Id.ToString(),
                    UserName = verifyTokenResult.MemberKey.ToLower(),
                    SessionId = sessionId,
                    BrandName = verifyTokenResult.BrandName
                };

                session = await userSessionService.CreateUserSession(userSession);
            }

            return session;
        }

        private static string GetMemberCurrency(VerificationTokenResponse verifyTokenResult)
        {
            var currency = verifyTokenResult.Currency;

            if (!string.IsNullOrWhiteSpace(currency)
                && currency.ToUpperInvariant() == UUSCurrencyName)
            {
                if(!string.IsNullOrWhiteSpace(verifyTokenResult.Market))
                {
                    currency = $"{currency}-{verifyTokenResult.Market.ToUpperInvariant()}";
                }
                else if (!string.IsNullOrWhiteSpace(verifyTokenResult.Seq) && verifyTokenResult.Seq.Contains(','))
                {
                    var seqWords = verifyTokenResult.Seq.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (seqWords.Any())
                    {
                        currency = $"{currency}-{seqWords.Last()}";
                    }
                }
            }

            return currency;
        }

        private static bool IsExpiredSession(UserSessionEntity userSession)
        {
            if (userSession != null)
            {
                var isExpired = userSession.ReCheckInTime.AddMinutes(sessionTimeout) < DateTime.UtcNow;

                return isExpired;
            }

            return true;
        }

        private string ConvertLanguage(string language)
        {
            if(language == Language.ChineseNewSimplifiedCode)
            {
                language = Language.ChineseSimplifiedCode;
            }

            if (string.IsNullOrWhiteSpace(language) || !cultureConfigs.SupportedCultures.Contains(language))
            {
                return cultureConfigs.DefaultCulture;
            }

            return language;
        }
    }
}
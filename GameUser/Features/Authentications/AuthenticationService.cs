using System;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;
using GameUser.Shared.Wrapper;

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
        private static readonly int sessionTimeout = 20;
        private readonly IApi api;
        private readonly IUserService userService;
        private readonly IUserSessionService userSessionService;
        private readonly IDateTimeProvider dateTimeProvider;

        public AuthenticationService(IApi api, IUserService userService, IUserSessionService userSessionService, IDateTimeProvider dateTimeProvider)
        {
            this.api = api;
            this.userService = userService;
            this.userSessionService = userSessionService;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<UserSignInResponse> SignIn(VerificationTokenRequest request)
        {
            var verificationResult = await api.VerifyToken(request);

            if (verificationResult.IsSuccessful)
            {
                var user = await CreateUserIfNotExist(verificationResult);
                var userSession = await AddSessionIfNotExist(user, verificationResult, request);
                var expiredIn = userSession.ReCheckInTime.AddMinutes(sessionTimeout);

                return new UserSignInResponse(true, userSession.UserName, userSession.Language, userSession.SessionId, userSession.Currency, expiredIn);
            }

            return new UserSignInResponse();
        }

        public async Task<RefreshSessionResponse> Refresh(RefreshSessionRequest request)
        {
            var userSession = await userSessionService.GetUserSession(request.Session);
            var isValidRequest = userSession != null && Equals(request.BrowserUserAgent, userSession.BrowserUserAgent);

            if (isValidRequest && !IsExpiredSession(userSession))
            {
                var now = this.dateTimeProvider.Now();

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
            var user = await userService.GetUser(verifyTokenResult.MemberKey);

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
                        RegDate = this.dateTimeProvider.Now(),
                        Password = Guid.NewGuid().ToString()
                    }
                };

                user = await userService.CreateUser(newUser);
            }

            return user;
        }

        private async Task<UserSessionEntity> AddSessionIfNotExist(UserEntity user, VerificationTokenResponse verifyTokenResult, VerificationTokenRequest request)
        {
            var session = await userSessionService.GetUserSessionByToken(request.Token);

            if (session == null)
            {
                var sessionId = $"{Guid.NewGuid()}";
                var now = this.dateTimeProvider.Now();
                var userSession = new UserSessionEntity
                {
                    BrowserUserAgent = request.BrowserUserAgent,
                    ClientId = verifyTokenResult.ClientId,
                    Currency = verifyTokenResult.Currency,
                    Ip = request.Ip,
                    Language = verifyTokenResult.Language,
                    MemberId = verifyTokenResult.MemberId,
                    MemberName = verifyTokenResult.MemberName,
                    OWSeq = verifyTokenResult.Seq,
                    ReCheckInTime = now,
                    SiteId = verifyTokenResult.SiteId,
                    SSOTime = now,
                    SSOToken = request.Token,
                    UserId = user.Id.ToString(),
                    UserName = verifyTokenResult.MemberKey.ToLower(),
                    SessionId = sessionId
                };

                session = await userSessionService.CreateUserSession(userSession);
            }

            return session;
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
    }
}
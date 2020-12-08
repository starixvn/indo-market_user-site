using System;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;

namespace GameUser.Features.Authentications
{
    public interface IAuthenticationService
    {
        Task<UserSignInResponse> SignIn(VerificationTokenRequest request);

        Task Refresh(RefreshSessionRequest request);

        Task<VerificationSessionResult> Verify(VerificationSessionRequest request);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApi api;
        private readonly IUserService userService;
        private readonly IUserSessionService userSessionService;

        public AuthenticationService(IApi api, IUserService userService, IUserSessionService userSessionService)
        {
            this.api = api;
            this.userService = userService;
            this.userSessionService = userSessionService;
        }

        public async Task<UserSignInResponse> SignIn(VerificationTokenRequest request)
        {
            var verificationResult = await api.VerifyToken(request);

            if (verificationResult.IsSuccessful)
            {
                var user = await CreateUserIfNotExist(verificationResult);
                var userSession = await AddSessionIfNotExist(user, verificationResult, request);

                return new UserSignInResponse
                {
                    Currency = userSession.Currency,
                    IsAuthenticated = true,
                    Language = userSession.Language,
                    Session = userSession.SessionId,
                    Username = userSession.UserName
                };
            }

            return new UserSignInResponse
            {
                IsAuthenticated = false
            };
        }

        public async Task Refresh(RefreshSessionRequest request)
        {
            var userSession = await userSessionService.GetUserSession(request.Session);
            var isValidRequest = userSession != null && Equals(request.BrowserUserAgent, userSession.BrowserUserAgent);

            if (isValidRequest && !IsExpiredSession(userSession))
            {
                await userSessionService.ReCheckIn(request.Session, DateTime.Now);
            }
        }

        public async Task<VerificationSessionResult> Verify(VerificationSessionRequest request)
        {
            var userSession = await userSessionService.GetUserSession(request.Session);

            return IsExpiredSession(userSession) ? new VerificationSessionResult
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
                        UserName = verifyTokenResult.MemberKey,
                        RegDate = DateTime.Now,
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
                    ReCheckInTime = DateTime.Now,
                    SiteId = verifyTokenResult.SiteId,
                    SSOTime = DateTime.Now,
                    SSOToken = request.Token,
                    UserId = user.Id.ToString(),
                    UserName = verifyTokenResult.MemberKey,
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
                const int timeout = 20;
                var isExpired = userSession.ReCheckInTime.AddMinutes(timeout) < DateTime.UtcNow;

                return isExpired;
            }

            return true;
        }
    }
}
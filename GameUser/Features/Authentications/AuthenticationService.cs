using System;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.Authentications.Entity;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;

namespace GameUser.Features.Authentications
{
    public interface IAuthenticationService
    {
        Task<UserSignInResponse> SignIn(UserSignInRequest request);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IApiService apiService;
        private readonly ApiEndpoints apiEndpoints;
        private readonly IUserService userService;
        private readonly IUserSessionService userSessionService;

        public AuthenticationService(IApiService apiService, IUserService userService, IUserSessionService userSessionService, ApiEndpoints endpoints)
        {
            this.apiService = apiService;
            this.userService = userService;
            this.userSessionService = userSessionService;

            apiEndpoints = endpoints;
        }

        public async Task<UserSignInResponse> SignIn(UserSignInRequest request)
        {
            var response = await apiService.Post<VerifyTokenResult>(apiEndpoints.VerifyToken, request);

            if (response.IsSuccessful)
            {
                var verifyTokenResult = response.Result;

                if (verifyTokenResult.IsSuccessful)
                {
                    var user = await CreateUserIfNotExist(verifyTokenResult);
                    var userSession = await AddSessionIfNotExist(user, verifyTokenResult, request);

                    return new UserSignInResponse
                    {
                        Currency = userSession.Currency,
                        IsAuthenticated = true,
                        Language = userSession.Language,
                        Session = userSession.SessionId,
                        Username = userSession.UserName
                    };
                }
                else
                {
                    // Occurs when the token expires
                }
            }

            return new UserSignInResponse
            {
                IsAuthenticated = false
            };
        }

        private async Task<UserEntity> CreateUserIfNotExist(VerifyTokenResult verifyTokenResult)
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

        private async Task<UserSessionEntity> AddSessionIfNotExist(UserEntity user, VerifyTokenResult verifyTokenResult, UserSignInRequest request)
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
    }
}
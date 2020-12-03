using System;
using System.Threading.Tasks;
using GameUser.Features.ApiComminicators;
using GameUser.Features.Authentications.Entity;
using GameUser.Features.Users;

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

        public AuthenticationService(IApiService apiService, IUserService userService, ApiEndpoints endpoints)
        {
            this.apiService = apiService;
            this.userService = userService;
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
                    return await HandleVerifySuccessful(verifyTokenResult);
                }
            }

            return new UserSignInResponse
            {
                IsAuthenticated = false
            };
        }

        private async Task<UserSignInResponse> HandleVerifySuccessful(VerifyTokenResult verifyTokenResult)
        {
            var user = await userService.GetUser(verifyTokenResult.MemberKey);

            if (user == null)
            {
                user = new UserEntity
                {
                    Local = new LocalUserEntity
                    {
                        ClientId = verifyTokenResult.ClientId,
                        Currency = verifyTokenResult.Currency,
                        MemberId = verifyTokenResult.MemberId,
                        MemberName = verifyTokenResult.MemberName,
                        UserName = verifyTokenResult.MemberKey,
                        RegDate = DateTime.Now
                    }
                };

                await userService.CreateUser(user);
            }

            return new UserSignInResponse
            {
                IsAuthenticated = true,
                Currency = user.Local.Currency,
                Language = verifyTokenResult.Language,
                Username = user.Local.UserName,
                Session = "session"
            };
        }
    }
}
using System.Threading.Tasks;
using GameUser.Features.UserInfos;
using GameUser.Features.UserInfos.Models;
using GameUser.Features.UserLanguages;
using GameUser.Features.UserLanguages.Models;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Shared.Wrappers;
using GameUser.Web.Features.APIs.Users.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.Users
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserInfoService userInfoService;
        private readonly IHttpContextAccessor accessor;
        private readonly IHttpRequestWrapper httpRequestWrapper;
        private readonly IUserLanguageService userLanguageService;

        public UserController(IUserInfoService userInfoService, IHttpContextAccessor accessor, IHttpRequestWrapper httpRequestWrapper, IUserLanguageService userLanguageService)
        {
            this.userInfoService = userInfoService;
            this.accessor = accessor;
            this.httpRequestWrapper = httpRequestWrapper;
            this.userLanguageService = userLanguageService;
        }

        [HttpGet("UserInfo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<JsonResult> GetUserInfo(string userId)
        {
            UserInfoEntity userInfo = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userInfo = await userInfoService.GetUserInfo(userId);
            }

            return Json(userInfo);
        }

        [HttpGet("existed")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public Task<bool> IsUserExisted()
        {
            var session = httpRequestWrapper.GetAuthorizationToken(accessor);

            return userInfoService.CheckUserExistsAndEnterPortal(session);
        }

        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> CreateUserInfo(CreateUserRequest request)
        {
            var session = httpRequestWrapper.GetAuthorizationToken(accessor);
            var isUserExisted = await userInfoService.IsUserExisted(session);
            var result = new CreateUserInfoResult();

            if (!isUserExisted)
            {
                result = await userInfoService.CreateUserInfo(session, request.Nickname);
            }

            return Json(result);
        }

        [HttpPost("language")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> ChangeUserLanguage(ChangeUserLanguageRequest request)
        {
            var session = httpRequestWrapper.GetAuthorizationToken(accessor);
            var result = await userLanguageService.ChangeLanguage(new ChangeLanguageResquest { Language = request.Language, SessionId = session });

            return Json(result);
        }

        [HttpPost("avatar")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> ChangeUserAvatar (ChangeUserAvatarRequest request)
        {
            var session = httpRequestWrapper.GetAuthorizationToken(accessor);

            var result = await userInfoService.UpdateUserAvatar(session, request.AvatarId);

            return Json(result);
        }
    }
}
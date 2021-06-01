using GameUser.Features.UserInfos;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Shared.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameUser.Web.Features.APIs.Users
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserInfoService userInfoService;
        private readonly IHttpContextAccessor accessor;
        private readonly IHttpRequestWrapper httpRequestWrapper;

        public UserController(IUserInfoService userInfoService, IHttpContextAccessor accessor, IHttpRequestWrapper httpRequestWrapper)
        {
            this.userInfoService = userInfoService;
            this.accessor = accessor;
            this.httpRequestWrapper = httpRequestWrapper;
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

            return userInfoService.IsUserExisted(session);
        }
    }
}
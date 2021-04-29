using GameUser.Features.UserInfos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameUser.Web.Features.APIs.Users
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserInfoService userInfoService;

        public UserController(IUserInfoService userInfoService)
        {
            this.userInfoService = userInfoService;
        }

        [HttpGet("UserInfo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<JsonResult> GetUserInfo(string userId)
        {
            UserInfoEntity userInfo = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userInfo = await userInfoService.GetUserInfoFromCache(userId);
            }

            return Json(userInfo);
        }
    }
}
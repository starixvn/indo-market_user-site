using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Web.Features.APIs.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameUser.Web.Features.APIs.Users
{
    [ApiController]
    [Route("api/user")]
    public class UserController: Controller
    {
        [HttpPost("IsNicknameExisted")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> IsNicknameExisted(IsNicknameExistedRequest request) {
            return Json(request);
        }
    }
}

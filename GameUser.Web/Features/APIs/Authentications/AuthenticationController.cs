using System.Threading.Tasks;
using GameUser.Features.Authentications;
using GameUser.Features.Authentications.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.Authentications
{
    [ApiController]
    [Route("api/user")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        [HttpPost("signin")]
        public async Task<JsonResult> SignIn(UserSignInRequest request)
        {
            var result = await authenticationService.SignIn(request);

            return Json(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshSession(RefreshSessionRequest request)
        {
            await authenticationService.Refresh(request);

            return Ok();
        }

        [HttpPost("verify")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<JsonResult> VerifySession(VerificationSessionRequest request)
        {
            var result = await authenticationService.Verify(request);

            return Json(result);
        }
    }
}
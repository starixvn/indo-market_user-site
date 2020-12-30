using System.Threading.Tasks;
using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications;
using GameUser.Features.Authentications.Models;
using GameUser.Shared.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.Authentications
{
    [ApiController]
    [Route("api/user")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IHttpContextAccessor accessor;
        private readonly IHttpRequestWrapper httpRequestWrapper;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IHttpContextAccessor accessor, 
            IHttpRequestWrapper httpRequestWrapper)
        {
            this.authenticationService = authenticationService;
            this.accessor = accessor;
            this.httpRequestWrapper = httpRequestWrapper;
        }

        [HttpPost("signin")]
        public async Task<JsonResult> SignIn(VerificationTokenRequest request)
        {
            request.Ip = httpRequestWrapper.GetClientIp(accessor);

            var result = await authenticationService.SignIn(request);

            return Json(result);
        }

        [HttpPost("refresh")]
        public async Task<JsonResult> RefreshSession(RefreshSessionRequest request)
        {
            var result = await authenticationService.Refresh(request);

            return Json(result);
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
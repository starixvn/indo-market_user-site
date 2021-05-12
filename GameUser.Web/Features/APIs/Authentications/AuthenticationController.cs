using GameUser.Features.ApiComminicators.Models;
using GameUser.Features.Authentications;
using GameUser.Features.Authentications.Models;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Shared.Wrappers;
using GameUser.Web.Features.APIs.Authentications.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<JsonResult> SignIn(SignInRequest request)
        {
            var authenticationRequet = new VerificationTokenRequest
            {
                Ip = httpRequestWrapper.GetClientIp(accessor),
                Token = request.Token,
                BrowserUserAgent = httpRequestWrapper.GetUserAgent(accessor)
            };

            var result = await authenticationService.SignIn(authenticationRequet);

            return Json(result);
        }

        [HttpPost("refresh")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> RefreshSession()
        {
            var refreshSessionRequest = new RefreshSessionRequest
            {
                BrowserUserAgent = httpRequestWrapper.GetUserAgent(accessor),
                Session = httpRequestWrapper.GetAuthorizationToken(accessor),
            };

            var result = await authenticationService.Refresh(refreshSessionRequest);

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
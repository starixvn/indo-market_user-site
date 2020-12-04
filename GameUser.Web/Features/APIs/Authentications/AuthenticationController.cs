using System.Threading.Tasks;
using GameUser.Features.Authentications;
using GameUser.Features.Authentications.Entity;
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

        [HttpPost]
        public async Task<JsonResult> SignIn(UserSignInRequest request)
        {
            var result = await authenticationService.SignIn(request);

            return Json(result);
        }
    }
}
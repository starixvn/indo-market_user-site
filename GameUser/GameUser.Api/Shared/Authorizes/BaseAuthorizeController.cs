using Microsoft.AspNetCore.Mvc;

namespace GameUser.Api.Shared.Authorizes
{
    [ApiController]//[ApiController, Authorize]
    public abstract class BaseAuthorizeController : ControllerBase
    {
    }
}

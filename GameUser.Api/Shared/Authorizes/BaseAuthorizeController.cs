using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Api.Shared.Authorizes
{
    [ApiController, Authorize]
    public abstract class BaseAuthorizeController : ControllerBase
    {
    }
}

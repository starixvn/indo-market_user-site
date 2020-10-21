using Microsoft.AspNetCore.Mvc;

namespace UserSite.Api._Shared
{
    [ApiController]//[ApiController, Authorize]
    public abstract class BaseAuthorizeController : ControllerBase
    {
    }
}

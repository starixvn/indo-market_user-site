using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameUser.Web.Features.APIs.Authentications
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet("init")]
        public async Task<IActionResult> InitAuthentication()
        {
            return Ok(true);
        }
    }
}

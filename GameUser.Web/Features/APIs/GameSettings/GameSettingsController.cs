using System.Threading.Tasks;
using GameUser.Features.GameSettings;
using GameUser.Features.UserSessions;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.GameSettings
{
    [ApiController]
    [Route("api/gamesettings")]
    public class GameSettingsController : Controller
    {
        private readonly IGameSettingsService gameSettingsService;
        private readonly IHttpContextAccessor accessor;
        private readonly IHttpRequestWrapper httpRequestWrapper;
        private readonly IUserSessionService userSessionService;

        public GameSettingsController(IGameSettingsService gameSettingsService,
            IUserSessionService userSessionService,
            IHttpContextAccessor accessor,
            IHttpRequestWrapper httpRequestWrapper)
        {
            this.gameSettingsService = gameSettingsService;
            this.accessor = accessor;
            this.httpRequestWrapper = httpRequestWrapper;
            this.userSessionService = userSessionService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> Get()
        {
            var sessionId = httpRequestWrapper.GetAuthorizationToken(accessor);
            var session = await userSessionService.GetUserSession(sessionId);

            var result = await gameSettingsService.GetGameSettings(session);

            return Json(result);
        }
    }
}
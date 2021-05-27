using System.Threading.Tasks;
using GameUser.Features.GameSettings;
using GameUser.Shared.SessionFilterAuthentications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.GameSettings
{
    [ApiController]
    [Route("api/gamesettings")]
    public class GameSettingsController: Controller
    {
        private readonly IGameSettingsService gameSettingsService;

        public GameSettingsController(IGameSettingsService gameSettingsService)
        {
            this.gameSettingsService = gameSettingsService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<JsonResult> Get()
        {
            var result = await gameSettingsService.GetGameSettings();

            return Json(result);
        }
    }
}
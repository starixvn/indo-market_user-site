using System.Collections.Generic;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs;
using GameUser.Features.CategoryConfigs.Models;
using GameUser.Features.FavoriteGame;
using GameUser.Features.UserSessions;
using GameUser.Shared.SessionFilterAuthentications;
using GameUser.Shared.Wrappers;
using GameUser.Web.Features.APIs.Lobby.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameUser.Web.Features.APIs.Lobby
{
    [ApiController]
    [Route("api/lobby")]
    public class LobbyController : Controller
    {
        private readonly IFavoriteGameService favoriteGameService;
        private readonly IHttpContextAccessor accessor;
        private readonly IHttpRequestWrapper httpRequestWrapper;
        private readonly IUserSessionService userSessionService;
        private readonly IGameCategoryService gameCategoryService;

        public LobbyController(
            IFavoriteGameService favoriteGameService,
            IUserSessionService userSessionService,
            IHttpContextAccessor accessor,
            IHttpRequestWrapper httpRequestWrapper,
            IGameCategoryService gameCategoryService)
        {
            this.favoriteGameService = favoriteGameService;
            this.accessor = accessor;
            this.httpRequestWrapper = httpRequestWrapper;
            this.userSessionService = userSessionService;
            this.gameCategoryService = gameCategoryService;
        }

        [HttpGet("Configs")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IList<CategoryConfigModel>> GetLobbyConfigs()
        {
            var sessionId = httpRequestWrapper.GetAuthorizationToken(accessor);
            var session = await userSessionService.GetUserSession(sessionId);

            return await gameCategoryService.GetConfigs(session);
        }

        [HttpPost("FavoriteGame")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task AddFavoriteGame([FromBody] AddFavoriteGameRequest request)
        {
            var sessionId = httpRequestWrapper.GetAuthorizationToken(accessor);
            var session = await userSessionService.GetUserSession(sessionId);
            var userId = session.UserId;

            await favoriteGameService.AddFavoriteGame(userId, request.GameId);
        }

        [HttpDelete("FavoriteGame")]
        [Authorize(AuthenticationSchemes = SessionFilterAuthenticationDefaults.AuthenticationScheme)]
        public async Task RemoveFavoriteGame([FromBody] RemoveFavoriteGameRequest request)
        {
            var sessionId = httpRequestWrapper.GetAuthorizationToken(accessor);
            var session = await userSessionService.GetUserSession(sessionId);
            var userId = session.UserId;

            await favoriteGameService.RemoveFavoriteGame(userId, request.GameId);
        }
    }
}
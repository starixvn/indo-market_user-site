using System.Threading.Tasks;
using GameUser.Features.Authentications;
using GameUser.Features.UserLanguages.Models;
using GameUser.Features.Users;
using GameUser.Features.UserSessions;

namespace GameUser.Features.UserLanguages
{
    public interface IUserLanguageService
    {
        Task<ChangeLanguageResponse> ChangeLanguage(ChangeLanguageResquest request);
    }

    public class UserLanguageService : IUserLanguageService
    {
        private readonly IUserSessionService userSessionService;
        private readonly IUserService userService;
        private readonly ICultureConfigs cultureConfigs;

        public UserLanguageService(IUserSessionService userSessionService, IUserService userService, ICultureConfigs cultureConfigs)
        {
            this.userSessionService = userSessionService;
            this.userService = userService;
            this.cultureConfigs = cultureConfigs;
        }

        public async Task<ChangeLanguageResponse> ChangeLanguage(ChangeLanguageResquest request)
        {
            var language = cultureConfigs.ConvertLanguage(request.Language);
            var session = await userSessionService.GetUserSession(request.SessionId);
            var newSessionId = await userSessionService.ChangeSessionId(session);

            if (!string.IsNullOrEmpty(newSessionId))
            {
                await userService.UpdateLanguage(session.UserName, language);
                await userSessionService.UpdateSessionLanguage(newSessionId, language);
            }

            return new ChangeLanguageResponse
            {
                SessionId = newSessionId,
                Language = language
            };
        }
    }
}
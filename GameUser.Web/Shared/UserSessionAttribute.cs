namespace GameUser.Web.Shared
{
    using GameUser.Features.UserSessions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Threading.Tasks;

    public class UserSessionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var sessionId = context.HttpContext.Request.Query["sessionid"].ToString();
            var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                context.Result = new UnauthorizedResult();                
            }                                 
            
            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession == null)
            {
                context.Result = new UnauthorizedResult();                
            }                                  
        }
    }
}

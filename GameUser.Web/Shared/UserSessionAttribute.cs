namespace GameUser.Web.Shared
{
    using GameUser.Features.UserSessions;
    using GameUser.Web.Shared.Configurations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class UserSessionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly int expiredUserSession = 3; //3 minutes
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var sessionId = context.HttpContext.Request.Query["sessionid"].ToString();
            var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;
            
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                context.Result = new RedirectResult("/Error/NotAuthorized");
                return;
            }

            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession == null)
            {
                context.Result = new RedirectResult("/Error/NotAuthorized");
            }
            else
            {  
                var totalMinutes = (int)DateTime.Now.ToUniversalTime().Subtract(userSession.ReCheckInTime).TotalMinutes;

                if (totalMinutes > expiredUserSession)
                {
                    context.Result = new RedirectResult("/Error/NotAuthorized");
                }
            }
        }
    }
}

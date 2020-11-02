namespace GameUser.Web.Shared
{
    using GameUser.Features.UserSessions;
    using GameUser.Web.Shared.Configurations;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class UserSessionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly int expiredUserSession = 3; //3 minutes

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var sessionId = context.HttpContext.Request.Query["sessionid"].ToString();            
            var verifiedUserSession = true;

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                verifiedUserSession = await VerifyUserSession(sessionId, context);

                if(verifiedUserSession)
                {
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, sessionId)
                        };
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = new DateTimeOffset(DateTime.UtcNow.AddMinutes(30))
                    };

                    await context.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                }
                else
                {
                    context.Result = new RedirectResult("/Error/NotAuthorized");                                        
                }                
            }
            else
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    sessionId = context.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                    verifiedUserSession = await VerifyUserSession(sessionId, context);

                    if(!verifiedUserSession)
                    {
                        context.Result = new RedirectResult("/Error/NotAuthorized");                                                
                    }
                }
                else
                {
                    context.Result = new RedirectResult("/Error/NotAuthorized");
                }                
            }
        }

        public async Task<bool> VerifyUserSession(string sessionId, AuthorizationFilterContext context)
        {
            var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;

            var userSession = await userSessionService.GetUserSession(sessionId);

            if (userSession == null)
            {
                return false;
            }
            else
            {
                var totalMinutes = (int)DateTime.Now.ToUniversalTime().Subtract(userSession.ReCheckInTime).TotalMinutes;

                if (totalMinutes > expiredUserSession)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

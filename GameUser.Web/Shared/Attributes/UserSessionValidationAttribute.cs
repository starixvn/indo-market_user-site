namespace GameUser.Web.Shared.Attributes
{
    using GameUser.Features.UserSessions;
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
    public class UserSessionValidationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly int expiredMinuteUserSession = 3;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {                
                var sessionId = context.HttpContext.Request.Query["sessionid"].ToString();

                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    var verifiedUserSession = await VerifyUserSession(sessionId, context);

                    if (verifiedUserSession)
                    {
                        await SetCookie(context, sessionId);
                    }
                    else
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

        private async Task<bool> VerifyUserSession(string sessionId, AuthorizationFilterContext context)
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

                if (totalMinutes > expiredMinuteUserSession)
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task SetCookie(AuthorizationFilterContext context, string sessionId)
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
    }
}

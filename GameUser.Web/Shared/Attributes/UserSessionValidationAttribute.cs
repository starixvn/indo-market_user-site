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

    public interface IUserSessionValidationAttribute : IAsyncAuthorizationFilter
    {
        public Task<bool> VerifyUserSession(string sessionId, AuthorizationFilterContext context);

        public Task SignIn(string sessionId, AuthorizationFilterContext context);

    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class UserSessionValidationAttribute : Attribute, IUserSessionValidationAttribute
    {
        private readonly int expiredMinuteUserSession = 3;

        private DateTime CurrentDate { get; set; }

        public UserSessionValidationAttribute()
        {
            this.CurrentDate = DateTime.Now;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                if(context.HttpContext.Request.Query != null && context.HttpContext.Request.Query.Any())
                {
                    var sessionId = context.HttpContext.Request.Query["sessionid"].ToString();

                    if (!string.IsNullOrWhiteSpace(sessionId))
                    {
                        var verifiedUserSession = await VerifyUserSession(sessionId, context);

                        if (verifiedUserSession)
                        {
                            await SignIn(sessionId, context);
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
                else
                {
                    context.Result = new RedirectResult("/Error/NotAuthorized");
                }
                
            }
        }

        public async Task<bool> VerifyUserSession(string sessionId, AuthorizationFilterContext context)
        {           
            if (context != null)
            {
                var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;

                var userSession = await userSessionService.GetUserSession(sessionId);

                if (userSession == null)
                {
                    return false;
                }
                else
                {
                    var totalMinutes = (int)CurrentDate.ToUniversalTime().Subtract(userSession.ReCheckInTime).TotalMinutes;

                    if (totalMinutes > expiredMinuteUserSession)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public async Task SignIn(string sessionId, AuthorizationFilterContext context)
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

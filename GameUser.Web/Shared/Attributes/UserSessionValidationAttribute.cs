namespace GameUser.Web.Shared.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GameUser.Features.UserSessions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public interface IUserSessionValidationAttribute : IAsyncAuthorizationFilter
    {
        public Task<bool> VerifyUserSessionAndCheckInTime(string sessionId, AuthorizationFilterContext context);

        public Task SignIn(string sessionId, AuthorizationFilterContext context);
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class UserSessionValidationAttribute : Attribute, IUserSessionValidationAttribute
    {
        private readonly double expiredMinuteUserSession = 1;

        private readonly int expiredMinuteCookie = 30;

        private readonly string sessionIdKey = "sessionid";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var sessionId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == sessionIdKey).Value;

                await SetCookie(context, sessionId);
            }
            else
            {
                var sessionId = context.HttpContext.Request.Query[sessionIdKey].ToString();

                await SetCookie(context, sessionId);
            }
        }

        private async Task SetCookie(AuthorizationFilterContext context, string sessionId)
        {
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                var verifiedUserSession = await VerifyUserSessionAndCheckInTime(sessionId, context);

                if (verifiedUserSession)
                {
                    await SignIn(sessionId, context);
                }
                else
                {
                    await SignOut(context);

                    context.Result = Redirect(context);
                }
            }
            else
            {
                await SignOut(context);

                context.Result = Redirect(context);
            }
        }

        public async Task<bool> VerifyUserSessionAndCheckInTime(string sessionId, AuthorizationFilterContext context)
        {
            if (context != null)
            {
                var currentDateUTC = DateTime.UtcNow;

                var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;

                var userSession = await userSessionService.GetUserSession(sessionId);

                if (userSession == null)
                {
                    return false;
                }
                else
                {
                    var totalMinutes = currentDateUTC.Subtract(userSession.ReCheckInTime).TotalMinutes;

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
                                new Claim(sessionIdKey, sessionId)
                            };
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = new DateTimeOffset(DateTime.UtcNow.AddMinutes(expiredMinuteCookie))
            };

            await context.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task SignOut(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                await context.HttpContext.SignOutAsync();
            }
        }

        private static IActionResult Redirect(AuthorizationFilterContext context)
        {
            string requestedWith = context.HttpContext.Request.Headers["X-Requested-With"];

            if (requestedWith == "XMLHttpRequest")
            {
                return new JsonResult(new { HttpStatusCode.Unauthorized });
            }
            else
            {
                return new RedirectResult(string.Format("/{0}/Error/NotAuthorized", context.RouteData.Values["culture"]));
            }
        }
    }
}
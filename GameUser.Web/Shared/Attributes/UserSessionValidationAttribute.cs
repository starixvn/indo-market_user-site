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
        private readonly int expiredMinuteCookie = 30;

        private readonly string sessionIdKey = "sessionid";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var requestSessionId = context.HttpContext.Request.Query[sessionIdKey].ToString();

                var claimSessionId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == sessionIdKey).Value;

                if (requestSessionId == claimSessionId)
                {
                    await SetCookie(context, claimSessionId);
                }

                await SetCookie(context, requestSessionId);

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
                var userSessionService = context.HttpContext.RequestServices.GetService(typeof(IUserSessionService)) as UserSessionService;

                var userSession = await userSessionService.GetUserSession(sessionId);

                var sessionExpired = userSessionService.IsExpiredSession(userSession);

                if (sessionExpired)
                {
                    return false;
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
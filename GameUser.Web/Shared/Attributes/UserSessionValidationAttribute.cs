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
        public Task SignIn(string sessionId, AuthorizationFilterContext context);
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class UserSessionValidationAttribute : Attribute, IUserSessionValidationAttribute
    {
        private readonly int expiredMinuteCookie = 30;

        private readonly string sessionIdKey = "sessionid";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var sessionToValidate = GetRequestSessionId(context);

            if (UserPreviouslySignedIn(context) && 
                (UnchangedSessionId(sessionToValidate, GetClaimSessionId(context)) || string.IsNullOrWhiteSpace(sessionToValidate)))
            {
                sessionToValidate = GetClaimSessionId(context);
            }

            await ValidateUserRequest(context, sessionToValidate);
        }

        private async Task ValidateUserRequest(AuthorizationFilterContext context, string sessionId)
        {
            var validSession = await SessionValidated(context, sessionId);

            if (validSession)
            {
                await SignIn(sessionId, context);
            }
            else
            {
                await SignOut(context);

                context.Result = Redirect(context);
            }
        }

        private static async Task<bool> VerifyUserSession(string sessionId, AuthorizationFilterContext context)
        {
            if (context != null)
            {
                var userSessionService = context.HttpContext.RequestServices.GetService(
                    typeof(IUserSessionService))
                    as UserSessionService;

                var userSession = await userSessionService.GetUserSession(sessionId);

                var sessionExpired = userSessionService.IsExpiredSession(userSession);

                return !sessionExpired;
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

        private static async Task SignOut(AuthorizationFilterContext context)
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

        private static bool UnchangedSessionId(string requestSessionId, string claimSessionId)
        => requestSessionId == claimSessionId;

        private string GetRequestSessionId(AuthorizationFilterContext context)
        => context.HttpContext.Request.Query[sessionIdKey].ToString();

        private string GetClaimSessionId(AuthorizationFilterContext context)
        => context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == sessionIdKey).Value;

        private static bool UserPreviouslySignedIn(AuthorizationFilterContext context)
        => context.HttpContext.User.Identity.IsAuthenticated;

        private static async Task<bool> SessionValidated(AuthorizationFilterContext context, string sessionId)
        => !string.IsNullOrWhiteSpace(sessionId) && await VerifyUserSession(sessionId, context);
    }
}
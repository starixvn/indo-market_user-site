using GameUser.Features.UserSessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GameUser.Shared.SessionFilterAuthentications
{
    public class SessionFilterAuthenticationHandler : AuthenticationHandler<SessionFilterAuthenticationOptions>
    {
        private readonly IMemoryCache memoryCache;
        private readonly IUserSessionService userSessionService;
        private static readonly int sessionTimeout = 20;
        private static readonly string userAgentKey = "User-Agent";

        public SessionFilterAuthenticationHandler(
            IOptionsMonitor<SessionFilterAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache memoryCache,
            IUserSessionService userSessionService)
            : base(options, logger, encoder, clock)
        {
            this.memoryCache = memoryCache;
            this.userSessionService = userSessionService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            const string authorizationKey = "Authorization";

            if (!Request.Headers.ContainsKey(authorizationKey) || !Request.Headers.ContainsKey(userAgentKey))
            {
                return Fail();
            }

            string sessionId = Request.Headers[authorizationKey];

            if (string.IsNullOrEmpty(sessionId))
            {
                return Fail();
            }

            var session = await memoryCache.GetOrCreateAsync(sessionId, async cache =>
            {
                var currentSession = await userSessionService.GetUserSession(sessionId);

                if (!IsExpiredSession(currentSession))
                {
                    cache.Dispose();
                } else
                {
                    var timeout = currentSession.ReCheckInTime.AddMinutes(sessionTimeout);
                    var relative = timeout.Subtract(currentSession.ReCheckInTime);

                    cache.SetAbsoluteExpiration(relative);
                }

                return currentSession;

            });

            if (IsExpiredSession(session) || !IsSameUserAgent(session))
            {
                return Fail();
            }

            return Success();
        }

        private bool IsExpiredSession(UserSessionEntity userSession)
        {
            if (userSession != null)
            {
                var isExpired = userSession.ReCheckInTime.AddMinutes(sessionTimeout) < DateTime.UtcNow;

                return isExpired;
            }

            return true;
        }

        private bool IsSameUserAgent(UserSessionEntity userSession)
        {
            string requestUserAgent = Request.Headers[userAgentKey];

            return userSession != null && string.Equals(requestUserAgent, userSession.BrowserUserAgent);
        }

        private AuthenticateResult Success()
        {
            var identity = new ClaimsIdentity(Scheme.Name);
            var principal = new System.Security.Principal.GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private AuthenticateResult Fail()
        {
            return AuthenticateResult.Fail("Unauthorized");
        }
    }
}

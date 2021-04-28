using GameUser.Features.UserSessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
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
        private readonly ISentryClient sentryClient;

        private static readonly int sessionTimeout = 20;
        private static readonly string userAgentKey = "User-Agent";
        private static readonly string authorizationKey = "Authorization";

        public SessionFilterAuthenticationHandler(
            IOptionsMonitor<SessionFilterAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache memoryCache,
            IUserSessionService userSessionService,
            ISentryClient sentryClient)
            : base(options, logger, encoder, clock)
        {
            this.memoryCache = memoryCache;
            this.userSessionService = userSessionService;
            this.sentryClient = sentryClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!IsValidRequest())
            {
                return Fail();
            }

            var sessionId = Request.Headers[authorizationKey];
            var session = await GetUserSession(sessionId);

            WriteLog(session);

            if (IsExpiredSession(session) || !IsSameUserAgent(session))
            {
                return Fail();
            }

            return Success();
        }

        private void WriteLog(UserSessionEntity session) {
            if (!IsSameUserAgent(session))
            {
                var userInfo = new
                {
                    session.UserName,
                    session.UserId,
                    session.BrandName,
                    session.BrowserUserAgent,
                    session.ClientId,
                    session.SessionId
                };

                var requestUserAgent = Request.Headers[userAgentKey];

                var message = $"Request verification failed on ${DateTime.UtcNow}. User: ${JsonConvert.SerializeObject(userInfo)}. Browser user agent: ${requestUserAgent}";

                sentryClient.CaptureMessage(message, SentryLevel.Warning);
            }
        }

        private bool IsValidRequest()
        {
            var isValidRequest = false;
            var isContainsSpecialHeaders = Request.Headers.ContainsKey(authorizationKey) && Request.Headers.ContainsKey(userAgentKey);
            
            if (isContainsSpecialHeaders)
            {
                string token = Request.Headers[authorizationKey];

                isValidRequest = !string.IsNullOrEmpty(token);
            }

            return isValidRequest;
        }

        private async Task<UserSessionEntity> GetUserSession(string sessionId)
        {
            var session = await memoryCache.GetOrCreateAsync(sessionId, async cache =>
            {
                var currentSession = await userSessionService.GetUserSession(sessionId);

                if (IsExpiredSession(currentSession))
                {
                    cache.Dispose();
                }
                else
                {
                    var timeout = currentSession.ReCheckInTime.AddMinutes(sessionTimeout);
                    var relative = timeout.Subtract(currentSession.ReCheckInTime);

                    cache.SetAbsoluteExpiration(relative);
                }

                return currentSession;

            });

            return session;
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

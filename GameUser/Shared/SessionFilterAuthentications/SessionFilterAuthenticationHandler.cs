using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GameUser.Features.UserSessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;

namespace GameUser.Shared.SessionFilterAuthentications
{
    public class SessionFilterAuthenticationHandler : AuthenticationHandler<SessionFilterAuthenticationOptions>
    {
        private readonly IUserSessionService userSessionService;
        private readonly ISentryClient sentryClient;

        private const string userAgentKey = "User-Agent";
        private const string authorizationKey = "Authorization";

        public SessionFilterAuthenticationHandler(
            IOptionsMonitor<SessionFilterAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserSessionService userSessionService,
            ISentryClient sentryClient)
            : base(options, logger, encoder, clock)
        {
            this.userSessionService = userSessionService;
            this.sentryClient = sentryClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!IsValidRequest())
            {
                return Fail();
            }

            var authorized = false;

            try
            {
                var sessionId = Request.Headers[authorizationKey];
                var session = await userSessionService.GetUserSession(sessionId);

                authorized = !userSessionService.IsExpiredSession(session) && IsSameUserAgent(session);

                if (!authorized)
                {
                    WriteLog(session);
                }
            }
            catch (Exception exception)
            {
                sentryClient.CaptureException(exception);
            }

            return authorized ? Success() : Fail();
        }

        private void WriteLog(UserSessionEntity session)
        {
            var requestUserAgent = Request.Headers[userAgentKey];
            var sessionId = Request.Headers[authorizationKey];
            var sessionJson = session != null ? JsonConvert.SerializeObject(session) : string.Empty;
            var message = $"Session authentication failed on {DateTime.UtcNow}.\r\n" +
                $"Session: {sessionJson}\r\n" +
                $"Browser user agent: {requestUserAgent}\r\n" +
                $"SessionId: {sessionId}";

            sentryClient.CaptureMessage(message, SentryLevel.Warning);
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

        private static AuthenticateResult Fail() => AuthenticateResult.Fail("Unauthorized");
    }
}
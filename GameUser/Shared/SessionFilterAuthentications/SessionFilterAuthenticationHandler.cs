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

        private static readonly string userAgentKey = "User-Agent";
        private static readonly string authorizationKey = "Authorization";

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

            var sessionId = Request.Headers[authorizationKey];
            var session = await userSessionService.GetUserSession(sessionId);

            WriteLog(session);

            if (userSessionService.IsExpiredSession(session) || !IsSameUserAgent(session))
            {
                return Fail();
            }

            return Success();
        }

        private void WriteLog(UserSessionEntity session)
        {
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
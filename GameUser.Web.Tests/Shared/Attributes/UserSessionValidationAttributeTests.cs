using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GameUser.Web.Shared.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace GameUser.Web.Tests.Shared.Attributes
{
    public class UserSessionValidationAttributeTests
    {
        //private readonly IUserSessionService userSessionService;

        //private readonly UserSessionEntity userSessionEntity;

        public UserSessionValidationAttributeTests()
        {
            //userSessionService = Substitute.For<IUserSessionService>();
        }

        [Fact]
        public async Task UserSessionValidationAttribute_WhenIsNotAuthenticatedAsync_ReturnFalse()
        {
            var attribute = new UserSessionValidationAttribute();
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();

            var context = new AuthorizationFilterContext(
                new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ActionDescriptor()
                ),
                new List<IFilterMetadata>());

            await attribute.OnAuthorizationAsync(context);

            Assert.False(context.HttpContext.User.Identity.IsAuthenticated);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task UserSessionValidationAttribute_WhenSessionIdIsNull_RedirectNotAuthorized(string sessionId)
        {
            var httpContextMock = new Mock<HttpContext>();
            var query = new Mock<IQueryCollection>();

            query.SetupGet(q => q["sessionid"]).Returns(sessionId);

            httpContextMock.Setup(x => x.Request.Query).Returns(query.Object);
            httpContextMock.Setup(x => x.User).Returns(new ClaimsPrincipal());
            httpContextMock.Setup(x => x.User.Identity).Returns(new ClaimsIdentity());
            httpContextMock.Setup(x => x.User.Identity.IsAuthenticated).Returns(false);
            httpContextMock.Setup(x => x.Request.Headers["X-Requested-With"]).Returns(string.Empty);

            var attribute = new UserSessionValidationAttribute();

            var routeData = new RouteData();

            var context = new AuthorizationFilterContext(
                new ActionContext(
                    httpContext: httpContextMock.Object,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ActionDescriptor()
                ),
                new List<IFilterMetadata>());

            await attribute.OnAuthorizationAsync(context);

            Assert.Equal("//Error/NotAuthorized", ((RedirectResult)context.Result).Url);
        }
    }
}
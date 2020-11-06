using FluentAssertions;
using GameUser.Web.Shared.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace GameUser.Web.Tests.Shared.Attributes
{
    public class InternationalizationAttributeTests
    {
        [Fact]
        public void OnActionExecuting_WhenHasCultureRouteData_UseCultureRouteData()
        {
            // Arrange
            var attribute = new InternationalizationAttribute();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteValueDictionary();
            routeData.Add("culture", "xyz");
            var context = new ActionExecutingContext(
                new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
                ),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                NSubstitute.Substitute.For<Controller>());


            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Thread.CurrentThread.CurrentCulture.Name.Should().Be("xyz");
            Thread.CurrentThread.CurrentUICulture.Name.Should().Be("xyz");
        }

        [Fact]
        public void OnActionExecuting_WhenDoesNotHasCultureRouteData_UseEnglish()
        {
            // Arrange
            var attribute = new InternationalizationAttribute();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var httpContext = new DefaultHttpContext();
            var context = new ActionExecutingContext(
                new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
                ),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                NSubstitute.Substitute.For<Controller>());


            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Thread.CurrentThread.CurrentCulture.Name.Should().Be("en");
            Thread.CurrentThread.CurrentUICulture.Name.Should().Be("en");
        }
    }
}
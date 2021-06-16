using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace GameUser.Web.Tests.Shared.Configurations
{
    public class FeatureViewLocationExpanderTests
    {
        [Fact]
        public void ExpandViewLocations_WhenContextNull_ThrowNullException()
        {
            // Arrange
            ViewLocationExpanderContext context = null;
            var viewLocations = new List<string> { "view1", "view2" };
            var expander = new FeatureViewLocationExpander();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => expander.ExpandViewLocations(context, viewLocations).ToList());
        }

        [Fact]
        public void ExpandViewLocations_WhenViewLocationNull_ThrowNullException()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteValueDictionary();
            routeData.Add("culture", "xyz");
            var actionContext = new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
                );

            ViewLocationExpanderContext context = new ViewLocationExpanderContext(actionContext, "ViewName", "ContrllerName", "AreaName", "PageName", false);

            List<string> viewLocations = null;

            var expander = new FeatureViewLocationExpander();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => expander.ExpandViewLocations(context, viewLocations).ToList());
        }

        [Fact]
        public void ExpandViewLocations_WhenActionDescriptorNull_ThrowNullException()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteValueDictionary();
            routeData.Add("culture", "xyz");
            var actionContext = new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
                );

            actionContext.ActionDescriptor = null;

            ViewLocationExpanderContext context = new ViewLocationExpanderContext(actionContext, "ViewName", "ContrllerName", "AreaName", "PageName", false);

            List<string> viewLocations = new List<string> { "view1", "view2" };

            var expander = new FeatureViewLocationExpander();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => expander.ExpandViewLocations(context, viewLocations).ToList());
        }

        [Fact]
        public void ExpandViewLocations_WhenNoError_UpdateViewLocations()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteValueDictionary();
            routeData.Add("culture", "xyz");
            var controllerProperties = new Dictionary<object, object>();
            var featureName = "feature name";
            controllerProperties.Add("feature", featureName);
            var actionContext = new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(routeData),
                    actionDescriptor: new ControllerActionDescriptor { Properties = controllerProperties },
                    modelState: modelState
                );

            ViewLocationExpanderContext context = new ViewLocationExpanderContext(actionContext, "ViewName", "ContrllerName", "AreaName", "PageName", false);

            List<string> viewLocations = new List<string> { "view1{3} abc", "view2 {3} xyz" };

            var expander = new FeatureViewLocationExpander();

            var expectedResult = new List<string> { string.Format("view1{0} abc", featureName), string.Format("view2 {0} xyz", featureName) };

            // Act
            var result = expander.ExpandViewLocations(context, viewLocations).ToList();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
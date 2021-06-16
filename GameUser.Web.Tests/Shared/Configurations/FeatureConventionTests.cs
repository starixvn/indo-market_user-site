using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using GameUser.Features.Avatars;
using GameUser.Features.Feedbacks;
using GameUser.Features.UserInfos;
using GameUser.Features.UserSessions;
using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Shared.Configurations;
using GameUser.Web.Shared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace GameUser.Web.Tests.Shared.Configurations
{
    public class FeatureConventionTests
    {
        [Fact]
        public void Apply_WhenCalled_GetFeatureName()
        {
            // Arrange
            var httpContext = Substitute.For<IHttpContextAccessor>();
            var feedbackService = Substitute.For<IFeedbackService>();
            var appSetting = Substitute.For<IAppSettings>();
            var avatarService = Substitute.For<IAvatarService>();
            var userSessionService = Substitute.For<IUserSessionService>();
            var localizer = Substitute.For<IStringLocalizer<Resources>>();
            var userInfoService = Substitute.For<IUserInfoService>();

            var controller = new FeedbackController(httpContext, feedbackService, appSetting, avatarService, userSessionService, localizer, userInfoService);
            var typeInfo = controller.GetType().GetTypeInfo();
            var featureConvention = new FeatureConvention();
            var controllerModel = new ControllerModel(typeInfo, Substitute.For<IReadOnlyList<object>>());

            // Act
            featureConvention.Apply(controllerModel);

            // Assert
            controllerModel.Properties["feature"].Should().Be("Feedbacks");
        }

        [Fact]
        public void Apply_WhenFeatureNotExist_ReturnEmpty()
        {
            var typeInfo = "asdfad".GetType().GetTypeInfo();
            var featureConvention = new FeatureConvention();
            var controllerModel = new ControllerModel(typeInfo, Substitute.For<IReadOnlyList<object>>());

            // Act
            featureConvention.Apply(controllerModel);

            // Assert
            controllerModel.Properties["feature"].Should().Be("");
        }
    }
}
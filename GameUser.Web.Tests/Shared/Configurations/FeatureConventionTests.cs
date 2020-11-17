using FluentAssertions;
using GameUser.Features.Avatars;
using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Features.UserSessions;
using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NSubstitute;
using Sentry;
using System.Collections.Generic;
using System.Reflection;
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
            var emailConfigs = Substitute.For<IEmailConfigs>();
            var fileUploadService = Substitute.For<IFileUploadService>();
            var emailService = Substitute.For<IEmailService>();
            var sentryClient = Substitute.For<ISentryClient>();
            var avatarService = Substitute.For<IAvatarService>();
            var userSessionService = Substitute.For<IUserSessionService>();            

            var controller = new FeedbackController(httpContext, feedbackService, appSetting, fileUploadService, emailService, emailConfigs, sentryClient, avatarService, userSessionService);
            var typeInfo = controller.GetType().GetTypeInfo();
            var featureConvention = new FeatureConvention();
            var controllerModel = new ControllerModel(typeInfo, NSubstitute.Substitute.For<IReadOnlyList<object>>());

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
            var controllerModel = new ControllerModel(typeInfo, NSubstitute.Substitute.For<IReadOnlyList<object>>());

            // Act
            featureConvention.Apply(controllerModel);

            // Assert
            controllerModel.Properties["feature"].Should().Be("");
        }
    }
}

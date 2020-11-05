using FluentAssertions;
using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Shared.Models;
using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Web.Tests.Features.Feedbacks
{
    public class FeedbackControllerTests
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IFeedbackService feedbackService;
        private readonly IAppSettings appSetting;
        private readonly IEmailConfigs emailConfigs;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;
        private readonly ISentryClient sentryClient;

        public FeedbackControllerTests()
        {
            this.httpContext = Substitute.For<IHttpContextAccessor>();
            this.feedbackService = Substitute.For<IFeedbackService>();
            this.appSetting = Substitute.For<IAppSettings>();
            this.emailConfigs = Substitute.For<IEmailConfigs>();
            this.fileUploadService = Substitute.For<IFileUploadService>();
            this.emailService = Substitute.For<IEmailService>();
            this.sentryClient = Substitute.For<ISentryClient>();
        }

        [Theory]
        [InlineData("culture", "xyz")]
        [InlineData("culture1", "xyz1")]
        public async Task AddGet_WhenCalled_ReturnViewWithModel(string key, string value)
        {
            // Arrange
            var httpRequest = Substitute.For<HttpRequest>();
            httpContext.HttpContext.Request.Returns(httpRequest);
            var routeValues = new RouteValueDictionary();
            routeValues.Add(key, value);
            httpRequest.RouteValues.Returns(routeValues);

            var controller = new FeedbackController(httpContext, feedbackService, appSetting, fileUploadService, emailService, emailConfigs, sentryClient);

            FeedbackRequest feedbackRequest = new FeedbackRequest { NickName = "nick name", SessionId = "sesssion id " };

            var feedbackTypes = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };
            feedbackService.GetTypes().Returns(feedbackTypes);

            var expectedCulture = key == "culture" ? value : "en";

            // Act
            var result = await controller.Add(feedbackRequest) as ViewResult;

            // Assert
            var model = result.Model as InsertFeedbackViewModel;
            model.SessionId.Should().Be(feedbackRequest.SessionId);
            model.NickName.Should().Be(feedbackRequest.NickName);
            model.FeedbackTypeOptions.Should().BeEquivalentTo(feedbackTypes);
            model.Culture.Should().Be(expectedCulture);
        }

        [Fact]
        public async Task AddPost_WhenCalled_AddFeedbackAndSendEmail()
        {
            // Arrange
            var controller = new FeedbackController(httpContext, feedbackService, appSetting, fileUploadService, emailService, emailConfigs, sentryClient);

            var model = new InsertFeedbackViewModel
            {
                NickName = "nick name",
                Feedback = new Feedback()
            };

            var emailRequest = new EmailRequest { Subject = "hiii" };
            feedbackService.GenerateEmail(model.Feedback, model.NickName, emailConfigs).Returns(emailRequest);

            // Act
            var result = await controller.Add(model) as JsonResult;

            // Assert
            await feedbackService.Received().AddFeedbackAsync(model.Feedback);
            await fileUploadService.Received().UploadFilesAsync(model.Feedback.Files, appSetting.StoreFileFullPath());
            await emailService.Received().SendAsync(emailRequest);
            var modelResult = result.Value as UpdateResult;
            modelResult.IsSuccess.Should().Be(true);
        }

        [Fact]
        public async Task AddPost_WhenError_CaptureError()
        {
            // Arrange
            var controller = new FeedbackController(httpContext, feedbackService, appSetting, fileUploadService, emailService, emailConfigs, sentryClient);

            var model = new InsertFeedbackViewModel
            {
                NickName = "nick name",
                Feedback = new Feedback()
            };

            feedbackService.AddFeedbackAsync(Arg.Any<Feedback>()).Throws(new Exception("some error"));

            // Act
            var result = await controller.Add(model) as JsonResult;

            // Assert
            var modelResult = result.Value as UpdateResult;
            modelResult.IsSuccess.Should().Be(false);
            sentryClient.ReceivedWithAnyArgs().CaptureException(default);
        }
    }
}

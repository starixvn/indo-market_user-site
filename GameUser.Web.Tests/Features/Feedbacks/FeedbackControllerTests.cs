using FluentAssertions;
using GameUser.Features.Avatars;
using GameUser.Features.Feedbacks;
using GameUser.Features.UserSessions;
using GameUser.Shared.Models;
using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Features.Feedbacks.Requests;
using GameUser.Web.Features.Feedbacks.ViewModels;
using GameUser.Web.Shared.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
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
        private readonly IAvatarService avatarService;
        private readonly IUserSessionService userSessionService;

        public FeedbackControllerTests()
        {
            this.httpContext = Substitute.For<IHttpContextAccessor>();
            this.feedbackService = Substitute.For<IFeedbackService>();
            this.appSetting = Substitute.For<IAppSettings>();
            this.avatarService = Substitute.For<IAvatarService>();
            this.userSessionService = Substitute.For<IUserSessionService>();
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

            var controller = new FeedbackController(httpContext, feedbackService, appSetting, avatarService, userSessionService);

            GetRequest feedbackRequest = new GetRequest { NickName = "nick name", SessionId = "sesssion id " };

            var feedbackTypes = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };
            feedbackService.GetTypes().Returns(feedbackTypes);

            var expectedCulture = key == "culture" ? value : "en";

            var userInfo = new UserInfoEntity { AvatarId = 1, Name = "abc", UserId = "123", Version = 123 };
            userSessionService.GetUserInfo(feedbackRequest.SessionId).Returns(userInfo);

            // Act
            var result = await controller.Add(feedbackRequest) as ViewResult;

            // Assert
            var model = result.Model as GetFeedbackViewModel;
            model.SessionId.Should().Be(feedbackRequest.SessionId);
            model.NickName.Should().Be(userInfo.Name);
            model.FeedbackTypeOptions.Should().BeEquivalentTo(feedbackTypes);
            model.Culture.Should().Be(expectedCulture);
        }

        [Fact]
        public async Task AddPost_WhenCalled_AddFeedbackAndSendEmail()
        {
            // Arrange
            var controller = new FeedbackController(httpContext, feedbackService, appSetting, avatarService, userSessionService);

            var request = new InsertRequest
            {
                NickName = "nick name",
                Feedback = new FeedbackViewModel()
            };
            var viewModel = new InsertFeedbackViewModel
            {
                Feedback = new Feedback(
                    category: request.Feedback.Category,
                    description: request.Feedback.Description,
                    files: request.Feedback.Files),
                NickName = request.NickName
            };


            var expectedResult = new UpdateResult(true, string.Empty);
            feedbackService.AddFeedbackAsync(Arg.Any<Feedback>(), viewModel.NickName, Arg.Any<bool>(), Arg.Any<string>()).Returns(expectedResult);

            // Act
            var result = await controller.Add(request) as JsonResult;

            // Assert
            var modelResult = result.Value as UpdateResult;
            modelResult.IsSuccess.Should().Be(expectedResult.IsSuccess);
            modelResult.Message.Should().Be(expectedResult.Message);
        }
    }
}

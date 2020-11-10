using FluentAssertions;
using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Shared.Database;
using GameUser.Shared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.Feedbacks
{
    public class FeedbackServiceTests
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackTypeLocalization feedbackTypeLocalization;
        private readonly IStringLocalizer<Resources> localizer;

        public FeedbackServiceTests()
        {
            feedbackRepository = Substitute.For<IGenericRepository<FeedbackEntity>>();
            sentryClient = Substitute.For<ISentryClient>();
            feedbackTypeLocalization = Substitute.For<IFeedbackTypeLocalization>();
            localizer = Substitute.For<IStringLocalizer<Resources>>();
        }        

        [Fact]
        public async Task GetTypes_WhenCalled_ReturnListOfFeedbackTypes()
        {
            // Arrange  
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer);
            var expectedResult = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };

            feedbackTypeLocalization.GetSelectListItems().Returns(expectedResult);

            // Act
            var result = await service.GetTypes();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task AddFeedback_WhenSuccess_CallInsert()
        {
            // Arrange             
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer);
            var feedback = new Feedback(category: "ss", description: "ff", nickname: "abc", files: null);

            FeedbackEntity entity = null;
            await feedbackRepository.InsertOneAsync(Arg.Do<FeedbackEntity>(x => entity = x));

            // Act
            var result = await service.AddFeedbackAsync(feedback);

            // Assert
            result.IsSuccess.Should().Be(true);
            entity.Category.Should().Be(feedback.Category);
            entity.Description.Should().Be(feedback.Description);
            entity.Nickname.Should().Be(feedback.Nickname);
        }

        [Fact]
        public async Task AddFeedback_WhenFailed_LogErrors()
        {
            // Arrange             
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer);
            feedbackRepository.InsertOneAsync(Arg.Any<FeedbackEntity>())
                .Returns(Task.FromException<List<object>>(new Exception("some error")));

            // Act
            var result = await service.AddFeedbackAsync(new Feedback());

            // Assert
            result.IsSuccess.Should().Be(false);
            sentryClient.ReceivedWithAnyArgs().CaptureException(default);
        }

        [Fact]
        public async Task GenerateEmail_WhenCalled_GenerateEmailContent() 
        {
            // Arrange      
            var emailConfig = Substitute.For<IEmailConfigs>();
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer);

            var expectedResult = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };
            feedbackTypeLocalization.GetSelectListItems().Returns(expectedResult);

            var nickname = "nick name";
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");
            var files = new List<IFormFile> { file, file };
            var feedback = new Feedback(category: "NewGame", description: "desc", files: files, nickname: string.Empty);

            emailConfig.ReceiverEmail.Returns("recever@gmail.com");

            var expectedBody = $"Dear SB club, " +
                           $"{Environment.NewLine}{Environment.NewLine}" +
                           $"{feedback.Description} " +
                           $"{Environment.NewLine}{Environment.NewLine}" +
                           $"This is an automated email. Please do not reply to it.";

            // Act
            var result = await service.GenerateEmail(feedback, nickname, emailConfig);

            // Assert
            result.Subject.Should().Be($"Saba_Club_[{DateTime.Now:dd/MM/yyyy}]_{nickname}_{"Request a new game"}");
            result.Body.Should().Be(expectedBody);
            result.Attachments.Should().BeEquivalentTo(files);
            result.ToAddress.Should().Be("recever@gmail.com");
        }
    }
}

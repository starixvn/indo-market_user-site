using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.Emails;
using GameUser.Features.Feedbacks;
using GameUser.Features.FileUploads;
using GameUser.Shared.Database;
using GameUser.Shared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Sentry;
using Xunit;

namespace GameUser.Tests.Features.Feedbacks
{
    public class FeedbackServiceTests
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;
        private readonly IFeedbackTypeLocalization feedbackTypeLocalization;
        private readonly IStringLocalizer<Resources> localizer;
        private readonly IEmailConfigs emailConfigs;
        private readonly IFileUploadService fileUploadService;
        private readonly IEmailService emailService;

        public FeedbackServiceTests()
        {
            feedbackRepository = Substitute.For<IGenericRepository<FeedbackEntity>>();
            sentryClient = Substitute.For<ISentryClient>();
            feedbackTypeLocalization = Substitute.For<IFeedbackTypeLocalization>();
            localizer = Substitute.For<IStringLocalizer<Resources>>();
            emailConfigs = Substitute.For<IEmailConfigs>();
            fileUploadService = Substitute.For<IFileUploadService>();
            emailService = Substitute.For<IEmailService>();
        }

        [Fact]
        public async Task GetTypes_WhenCalled_ReturnListOfFeedbackTypes()
        {
            // Arrange
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer, emailConfigs, fileUploadService, emailService);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddFeedback_WhenSuccess_CallInsert(bool enabledStoreFile)
        {
            // Arrange
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer, emailConfigs, fileUploadService, emailService);

            var expectedResult = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };
            feedbackTypeLocalization.GetSelectListItems(Arg.Any<string>()).Returns(expectedResult);

            var nickname = "nick name";
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");
            var files = new List<IFormFile> { file, file };
            var feedback = new Feedback(category: "NewGame", description: "desc", files: files);

            emailConfigs.ReceiverEmail.Returns("recever@gmail.com");

            var expectedBody = $"Dear Vgaming, " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"{feedback.Description} " +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"------------------------------------------------------" +
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"This is an automated email. Please do not reply to it.";
            var cusId = 123;
            var titleDate = $"[{DateTime.UtcNow.AddHours(8).ToString("yyyy/MM/dd", new CultureInfo("en-US"))}]";

            FeedbackEntity entity = null;
            await feedbackRepository.InsertOneAsync(Arg.Do<FeedbackEntity>(x => entity = x));

            EmailRequest emailRequest = null;
            await emailService.SendAsync(Arg.Do<EmailRequest>(x => emailRequest = x));

            // Act
            var result = await service.AddFeedbackAsync(feedback, nickname, enabledStoreFile, string.Empty, cusId, string.Empty);

            // Assert
            result.IsSuccess.Should().Be(true);
            entity.Category.Should().Be(feedback.Category);
            entity.Description.Should().Be(feedback.Description);
            entity.Nickname.Should().Be(nickname);
            emailRequest.Subject.Should().Be($"Vgaming__{titleDate}_{nickname}_{cusId}_{"Request a new game"}");
            emailRequest.Body.Should().Be(expectedBody);
            emailRequest.Attachments.Should().BeEquivalentTo(files);
            emailRequest.ToAddress.Should().Be("recever@gmail.com");
        }

        [Fact]
        public async Task AddFeedback_WhenFailed_LogErrors()
        {
            // Arrange
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization, localizer, emailConfigs, fileUploadService, emailService);
            var expectedResult = new List<SelectListItem> {
                new SelectListItem { Value = "NewGame", Text = "Request a new game"},
                new SelectListItem { Value = "ReportBug", Text = "Report a bug"},
                new SelectListItem { Value = "OtherIssue", Text = "Report other issues"}
            };
            feedbackTypeLocalization.GetSelectListItems().Returns(expectedResult);

            feedbackRepository.InsertOneAsync(Arg.Any<FeedbackEntity>())
                .Returns(Task.FromException<List<object>>(new Exception("some error")));

            // Act
            var result = await service.AddFeedbackAsync(new Feedback(), string.Empty, true, string.Empty, 123, string.Empty);

            // Assert
            result.IsSuccess.Should().Be(false);
            sentryClient.ReceivedWithAnyArgs().CaptureException(default);
        }
    }
}
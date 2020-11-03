using FluentAssertions;
using GameUser.Features.Feedbacks;
using GameUser.Shared.Repositories;
using NSubstitute;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.Feedbacks
{
    public class FeedbackServiceTests
    {
        private readonly IGenericRepository<FeedbackEntity> feedbackRepository;
        private readonly ISentryClient sentryClient;
        private readonly FeedbackTypeLocalization feedbackTypeLocalization;

        public FeedbackServiceTests()
        {
            feedbackRepository = Substitute.For<IGenericRepository<FeedbackEntity>>();
            sentryClient = Substitute.For<ISentryClient>();
            feedbackTypeLocalization = Substitute.For<FeedbackTypeLocalization>();
        }        

        [Fact]
        public async Task GetTypes_WhenCalled_ReturnListOfFeedbackTypes()
        {
            // Arrange  
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization);
            var expectedResult = new List<string> {
                "Request a new game",
                "Report a bug",
                "Report other issues"
            };

            // Act
            var result = await service.GetTypes();

            // Assert
            var feedbackTypes = result.Select(f => f.Text);
            feedbackTypes.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task AddFeedback_WhenSuccess_CallInsert()
        {
            // Arrange             
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization);
            var feedback = new Feedback { 
                 Category = "ss",
                 Description = "ff",
                 MemberId = 12,
                 MemberName = "abc"
            };

            FeedbackEntity entity = null;
            await feedbackRepository.InsertOneAsync(Arg.Do<FeedbackEntity>(x => entity = x));

            // Act
            var result = await service.AddFeedbackAsync(feedback);

            // Assert
            result.Should().Be(true);
            entity.Category.Should().Be(feedback.Category);
            entity.Description.Should().Be(feedback.Description);
            entity.MemberId.Should().Be(feedback.MemberId);
            entity.MemberName.Should().Be(feedback.MemberName);
        }

        [Fact]
        public async Task AddFeedback_WhenFailed_LogErrors()
        {
            // Arrange             
            var service = new FeedbackService(feedbackRepository, sentryClient, feedbackTypeLocalization);
            feedbackRepository.InsertOneAsync(Arg.Any<FeedbackEntity>())
                .Returns(Task.FromException<List<object>>(new Exception("some error")));

            // Act
            var result = await service.AddFeedbackAsync(new Feedback());

            // Assert
            result.Should().Be(false);
            sentryClient.Received().CaptureException(Arg.Any<Exception>());
        }
    }
}

using AutoFixture;
using FluentAssertions;
using GameUser.Api.Features.Feedbacks;
using GameUser.Features.Feedbacks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Api.Tests.Features.Feedbacks
{
    public class FeedbackControllerTests
    {
        private readonly Fixture fixture = new Fixture();

        [Fact]
        public async Task GetTypes_WhenCalled_ReturnListOfFeedbackTypes()
        {
            // Arrange             
            var feedbackService = Substitute.For<IFeedbackService>();
            var controller = new FeedbackController(feedbackService);

            var expectedResult = new List<FeedbackType> {
                new FeedbackType { Value = "value1", DisplayName = "value 1"},
                new FeedbackType { Value = "value2", DisplayName = "value 2"}
            };

            feedbackService.GetTypes().Returns(expectedResult);

            // Act
            var result = await controller.GetTypes() as OkObjectResult;

            // Assert
            result.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task AddFeedback_WhenCall_CallServiceAddFeedback()
        {
            // Arrange
            var feedbackService = Substitute.For<IFeedbackService>();
            var controller = new FeedbackController(feedbackService);

            var feedback = fixture.Create<Feedback>();

            // Act
            await controller.AddFeedback(feedback);

            // Assert
            await feedbackService.Received().AddFeedback(feedback);
        }

        [Fact]
        public async Task AddFeedback_WhenCall_ReturnAddFeedbackResult()
        {
            // Arrange
            var feedbackService = Substitute.For<IFeedbackService>();
            var controller = new FeedbackController(feedbackService);

            var feedback = fixture.Create<Feedback>();
            var expectedResult = fixture.Create<bool>();
            feedbackService.AddFeedback(feedback).Returns(expectedResult);

            // Act
            var result = await controller.AddFeedback(feedback) as OkObjectResult;

            // Assert
            result.Value.Should().Be(expectedResult);
        }
    }
}

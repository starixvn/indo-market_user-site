using FluentAssertions;
using GameUser.Web.Shared.Models;
using Xunit;

namespace GameUser.Web.Tests.Shared.Models
{
    public class ErrorDetailsTests
    {
        [Fact]
        public void ToString_WhenCalled_SerializeObject()
        {
            // Arrange
            var errorDetails = new ErrorDetails(statusCode: 10, message: "something here");

            // Act
            var result = errorDetails.ToString();

            // Assert
            result.Should().Be("{\"StatusCode\":10,\"Message\":\"something here\"}");
        }
    }
}
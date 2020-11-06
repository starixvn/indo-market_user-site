using FluentAssertions;
using GameUser.Web.Features.Errors;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GameUser.Web.Tests.Features.Errors
{
    public class ErrorControllerTests
    {
        [Fact]
        public void NotAuthorized_WhenCalled_ReturnNotAuthorizedView()
        {
            // Arrange
            var controller = new ErrorController();

            // Act
            var result = controller.NotAuthorized() as ViewResult;

            // Assert
            result.ViewName.Should().BeNull();
        }
    }
}

using FluentAssertions;
using GameUser.Web.Shared.Middlewares;
using GameUser.Web.Shared.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Web.Tests.Shared.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenException_HandleExceptionAsync()
        {
            // Arrange
            var exceptionMiddleware = new ExceptionMiddleware(
                (innerHttpContext) =>
                {
                    throw new Exception("Test");
                });

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await exceptionMiddleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<ErrorDetails>(streamText);

            // Assert
            objResponse.Should().BeEquivalentTo(new ErrorDetails(
                message: "Internal Server Error from the custom middleware.",
                statusCode: 500));
        }
    }
}

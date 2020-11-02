using AutoFixture;
using FluentAssertions;
using GameUser.Features.FileUploads;
using GameUser.Shared.Wrapper;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameUser.Tests.Features.FileUploads
{
    public class FileUploadServiceTests
    {
        private readonly Fixture fixture = new Fixture();

        [Fact]
        public async Task UploadFiles_WhenFilesNull_ReturnEmptyList()
        {
            // Arrange
            var sentryClient = Substitute.For<ISentryClient>();
            var systemIOWrapper  = Substitute.For<ISystemIOWrapper>();
            var service = new FileUploadService(sentryClient, systemIOWrapper);

            List<IFormFile> files = null;
            var baseDirectory = fixture.Create<string>();

            var expectedResult = new List<UploadedFile>();

            // Act
            var result = await service.UploadFilesAsync(files, baseDirectory);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task UploadFiles_WhenSuccess_ReturnSuccessStatuses()
        {
            // Arrange
            var sentryClient = Substitute.For<ISentryClient>();
            var systemIOWrapper = Substitute.For<ISystemIOWrapper>();
            var service = new FileUploadService(sentryClient, systemIOWrapper);

            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");

            var files = new List<IFormFile> { file, file };
            var baseDirectory = fixture.Create<string>();

            // Act
            var result = await service.UploadFilesAsync(files, baseDirectory);

            // Assert
            result.Count(i => i.SavedSuccess).Should().Be(files.Count);
        }

        [Fact]
        public async Task UploadFiles_WhenError_ReturnNotSuccessStatusesAndWriteLog()
        {
            // Arrange
            var sentryClient = Substitute.For<ISentryClient>();
            var systemIOWrapper = Substitute.For<ISystemIOWrapper>();
            var service = new FileUploadService(sentryClient, systemIOWrapper);

            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.txt");

            var files = new List<IFormFile> { file, file };
            var baseDirectory = fixture.Create<string>();

            systemIOWrapper.WriteFile(Arg.Any<IFormFile>(), Arg.Any<string>())
                .Returns(Task.FromException<List<object>>(new Exception("some error")));

            // Act
            var result = await service.UploadFilesAsync(files, baseDirectory);

            // Assert
            result.Count(i => !i.SavedSuccess).Should().Be(files.Count);
            sentryClient.Received().CaptureException(Arg.Any<Exception>());
        }
    }
}

using System;
using FluentAssertions;
using GameUser.Shared.Configurations;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Shared
{
    public class ConfigReaderTests
    {
        [Fact]
        public void GetValue_WhenError_ThrowException()
        {
            // Arrange
            var reader = new ConfigReader(null);

            // Act && Assert
            try
            {
                reader.GetValue<string>("abc");
            }
            catch (Exception e)
            {
                e.Message.Should().Be("Key: abc");
            }
        }

        [Fact]
        public void GetValue_WhenSuccess_ReturnConfigValue()
        {
            // Arrange
            var configuration = NSubstitute.Substitute.For<IConfiguration>();
            var reader = new ConfigReader(configuration);

            configuration["EnabledStoreFile"].Returns("true");

            // Act
            var result = reader.GetValue<bool>("EnabledStoreFile");

            // Assert
            result.Should().Be(true);
        }
    }
}
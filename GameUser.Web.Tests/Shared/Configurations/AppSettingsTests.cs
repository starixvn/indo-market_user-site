using FluentAssertions;
using GameUser.Shared.Configurations;
using GameUser.Web.Shared.Configurations;
using NSubstitute;
using System.IO;
using System.Reflection;
using Xunit;

namespace GameUser.Web.Tests.Shared.Configurations
{
    public class AppSettingsTests
    {
        private readonly IConfigReader configReader;

        public AppSettingsTests()
        {
            this.configReader = Substitute.For<IConfigReader>();
        }

        [Fact]
        public void AppSettings_WhenInit_ReadAppSettings()
        {
            // Arrange            
            configReader.GetValue<string>("ApiHost").Returns("api host");
            configReader.GetValue<string>("ResultUrl").Returns("result url");
            configReader.GetValue<string>("StoreFilePath").Returns("store file path");
            configReader.GetValue<string>("Env").Returns("env");

            // Act
            var result = new AppSettings(configReader);

            // Assert
            result.ApiHost.Should().Be("api host");
            result.ResultUrl.Should().Be("result url");
            result.StoreFilePath.Should().Be("store file path");
            result.Env.Should().Be("env");
        }

        [Fact]
        public void StoreFileFullPath_WhenDev_GetCurrentExecutingFolder()
        {
            // Arrange
            configReader.GetValue<string>("StoreFilePath").Returns("store file path");
            configReader.GetValue<string>("Env").Returns("DEV");

            var appSettings = new AppSettings(configReader);

            // Act
            var result = appSettings.StoreFileFullPath();

            // Assert
            result.Should().Be(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "store file path"));
        }

        [Fact]
        public void StoreFileFullPath_WhenNotDev_GetConfigFolder()
        {
            // Arrange
            configReader.GetValue<string>("StoreFilePath").Returns("store file path");
            configReader.GetValue<string>("Env").Returns("pro");

            var appSettings = new AppSettings(configReader);

            // Act
            var result = appSettings.StoreFileFullPath();

            // Assert
            result.Should().Be("store file path");
        }
    }
}

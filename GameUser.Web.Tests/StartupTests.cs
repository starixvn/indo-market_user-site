using FluentAssertions;
using GameUser.Web.Features.Feedbacks;
using GameUser.Web.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sentry;
using System.Collections.Generic;
using Xunit;

namespace GameUser.Web.Tests
{
    public class StartupTests
    {
        [Fact]
        public void ConfigureServices_WhenCalled_RegisterDependencies()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup(configuration);

            var configurationSection = Substitute.For<IConfigurationSection>();
            configurationSection["ConnectionString"].Returns("mongodb://lmn_backend:LmnBackend@10.10.50.12:47103");
            configurationSection["DatabaseName"].Returns("def");
            configuration.GetSection(nameof(DatabaseSettings)).Returns(configurationSection);

            var cultureSection = Substitute.For<IConfigurationSection>();
            var cultureContentSection = Substitute.For<IConfigurationSection>();
            cultureContentSection.Value.Returns("en");
            cultureSection.GetChildren().Returns(new List<IConfigurationSection>() { cultureContentSection });
            configuration.GetSection("SupportedCultures").Returns(cultureSection);

            configuration["Email:Server"].Returns("Email:Server");
            configuration["Email:Port"].Returns("123");
            configuration["Email:Security"].Returns("false");
            configuration["Email:SenderName"].Returns("Email:SenderName");
            configuration["Email:SenderEmail"].Returns("Email: SenderEmail");
            configuration["Email:ReceiverEmail"].Returns("Email:ReceiverEmail");
            configuration["Email:Account"].Returns("Email:Account");
            configuration["Email:Password"].Returns("Email:Password");
           
            // Act
            startup.ConfigureServices(services);
            services.AddSingleton(typeof(ISentryClient), new SentryClient(new SentryOptions { Dsn = new Dsn("https://60d32dc98cc14367b14d362f7d4cbf0b@sentryuat.sabagame.com/151") }));
            services.AddSingleton(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient<FeedbackController>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var controller = serviceProvider.GetService<FeedbackController>();
            controller.Should().NotBeNull();
        }        
    }
}

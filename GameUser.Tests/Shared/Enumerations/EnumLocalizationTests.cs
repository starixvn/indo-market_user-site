using FluentAssertions;
using GameUser.Features.Feedbacks;
using GameUser.Shared.Enumerations;
using GameUser.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Linq;
using Xunit;

namespace GameUser.Tests.Shared.Enumerations
{
    public class EnumLocalizationTests
    {
        [Fact]
        public void GetSelectListItems_WhenCalled_ReturnListItems()
        {
            // Arrange
            var localizer = NSubstitute.Substitute.For<IStringLocalizer<Resources>>();
            var enumLocalization = new EnumLocalization<FeedbackType, Resources>(localizer);

            // Act
            var result = enumLocalization.GetSelectListItems().ToList();

            // Assert
            result.Count.Should().Be(3);
        }
    }
}

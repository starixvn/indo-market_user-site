using FluentAssertions;
using GameUser.Features.Feedbacks;
using GameUser.Shared.Database;
using Xunit;

namespace GameUser.Tests.Shared.Databases
{
    public class DocumentTests
    {
        [Fact]
        public void GetCollectionName_WhenCalled_ReturnCollectionName()
        {
            // Act
            var result = Document.GetCollectionName(typeof(FeedbackEntity));

            // Assert
            result.Should().Be("feedbacks");
        }

        [Fact]
        public void GetCollectionName_WhenNotBsonAttribute_ReturnNull()
        {
            // Act
            var result = Document.GetCollectionName(typeof(Feedback));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ObjectId_WhenCalled_CanGetSet()
        {
            // Arrange
            var objectId = new MongoDB.Bson.ObjectId();
            var doc = new Document { Id = objectId };

            // Assert
            doc.Id.Should().Be(objectId);
        }
    }
}
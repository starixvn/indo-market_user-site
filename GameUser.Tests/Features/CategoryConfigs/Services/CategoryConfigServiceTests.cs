using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Services;
using GameUser.Shared.Database;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.CategoryConfigs.Services
{
    public class CategoryConfigServiceTests
    {
        private readonly ICategoryConfigService service;
        private readonly IGenericRepository<CategoryConfigEntity> repository;

        public CategoryConfigServiceTests()
        {
            repository = Substitute.For<IGenericRepository<CategoryConfigEntity>>();
            service = new CategoryConfigService(repository);
        }

        [Fact]
        public async Task GetByCategory_WhenCalled_ReturnListOfEntities()
        {
            // Arrange
            var entity = new CategoryConfigEntity
            {
                Active = true,
                CategoryId = "id",
                Default = true,
                GameIconsCollection = new List<GameIconEntity>(),
                Name = "name"
            };
            var configs = new List<CategoryConfigEntity>();

            configs.Add(entity);

            repository.FindAsync(Arg.Any<Expression<Func<CategoryConfigEntity, bool>>>()).Returns(configs);

            // Act
            var result = await service.GetByCategory(string.Empty);

            // Assert
            result.FirstOrDefault().Should().Equals(entity);
        }
    }
}
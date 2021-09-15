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
    public class CategoryServiceTests
    {
        private readonly ICategoryService service;
        private readonly IGenericRepository<CategoryEntity> repository;

        public CategoryServiceTests()
        {
            repository = Substitute.For<IGenericRepository<CategoryEntity>>();
            service = new CategoryService(repository);
        }

        [Fact]
        public async Task TaskGetAll_WhenCalled_ReturnListOfEntities()
        {
            // Arrange
            var entity = new CategoryEntity
            {
                Key = "key",
                Name = "name",
                SortOrder = 1
            };
            var configs = new List<CategoryEntity>();

            configs.Add(entity);

            repository.FindAsync(Arg.Any<Expression<Func<CategoryEntity, bool>>>()).Returns(configs);

            // Act
            var result = await service.GetAll();

            // Assert
            result.FirstOrDefault().Should().Equals(entity);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Services;
using GameUser.Shared.Database;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.CategoryConfigs.Services
{
    public class MarketCategoryServiceTests
    {
        private readonly IMarketCategoryService service;
        private readonly IGenericRepository<MarketCategoryEntity> repository;
        private readonly ICategoryConfigService categoryConfigService;
        private readonly ICategoryService categoryService;

        public MarketCategoryServiceTests()
        {
            repository = Substitute.For<IGenericRepository<MarketCategoryEntity>>();
            categoryConfigService = Substitute.For<ICategoryConfigService>();
            categoryService = Substitute.For<ICategoryService>();

            service = new MarketCategoryService(repository, categoryConfigService, categoryService);
        }

        [Fact]
        public async Task GetConfigs_WhenCalled_ReturnListOfEntities()
        {
            // Arrange
            var category = new CategoryEntity
            {
                Key = "key",
                Name = "name",
                SortOrder = 1,
                Id = new ObjectId()
            };
            var marketCategory = new MarketCategoryEntity
            {
                Id = new ObjectId(),
                MarketId = "market_id",
                CategoryConfigId = "507f191e810c19729de860ea"
            };
            var categoryConfigs = new CategoryConfigEntity
            {
                Active = true,
                CategoryId = "507f191e810c19729de860ea",
                Default = false,
                GameIconsCollection = new List<GameIconEntity>(),
                Id = new ObjectId("507f191e810c19729de860ea"),
                Name = "category"
            };
            var defaultCategoryConfigs = new CategoryConfigEntity
            {
                Active = true,
                CategoryId = "default_category_id",
                Default = true,
                GameIconsCollection = new List<GameIconEntity>(),
                Id = new ObjectId(),
                Name = "default_category"
            };

            var marketCategories = new List<MarketCategoryEntity>();
            var categories = new List<CategoryEntity>();
            var listCategoryConfigs = new List<CategoryConfigEntity>();

            defaultCategoryConfigs.GameIconsCollection.Add(new GameIconEntity());
            categories.Add(category);
            marketCategories.Add(marketCategory);
            listCategoryConfigs.Add(categoryConfigs);
            listCategoryConfigs.Add(defaultCategoryConfigs);

            categoryService.GetAll().Returns(categories);
            categoryConfigService.GetByCategory(Arg.Any<string>()).Returns(listCategoryConfigs);
            repository.FindAsync(Arg.Any<Expression<Func<MarketCategoryEntity, bool>>>()).Returns(marketCategories);

            // Act
            var result = await service.GetConfigs("market_id");

            // Assert
            result.Should().HaveCount(1);
            result.FirstOrDefault().Games.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetConfigs_MappingIsEmpty_ReturnDefaultConfigs()
        {
            // Arrange
            var category = new CategoryEntity
            {
                Key = "key",
                Name = "name",
                SortOrder = 1,
                Id = new ObjectId()
            };
            var categoryConfigs = new CategoryConfigEntity
            {
                Active = true,
                CategoryId = "507f191e810c19729de860ea",
                Default = false,
                GameIconsCollection = new List<GameIconEntity>(),
                Id = new ObjectId("507f191e810c19729de860ea"),
                Name = "category"
            };
            var defaultCategoryConfigs = new CategoryConfigEntity
            {
                Active = true,
                CategoryId = "default_category_id",
                Default = true,
                GameIconsCollection = new List<GameIconEntity>(),
                Id = new ObjectId(),
                Name = "default_category"
            };

            var marketCategories = new List<MarketCategoryEntity>();
            var categories = new List<CategoryEntity>();
            var listCategoryConfigs = new List<CategoryConfigEntity>();

            defaultCategoryConfigs.GameIconsCollection.Add(new GameIconEntity());
            categories.Add(category);
            listCategoryConfigs.Add(categoryConfigs);
            listCategoryConfigs.Add(defaultCategoryConfigs);

            categoryService.GetAll().Returns(categories);
            categoryConfigService.GetByCategory(Arg.Any<string>()).Returns(listCategoryConfigs);
            repository.FindAsync(Arg.Any<Expression<Func<MarketCategoryEntity, bool>>>()).Returns(marketCategories);

            // Act
            var result = await service.GetConfigs("market_id");

            // Assert
            result.Should().HaveCount(1);
            result.FirstOrDefault().Games.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetConfigs_SettingsIsEmtpy_ReturnCategoryHasEmptyGame()
        {
            // Arrange
            var category = new CategoryEntity
            {
                Key = "key",
                Name = "name",
                SortOrder = 1,
                Id = new ObjectId()
            };

            var marketCategories = new List<MarketCategoryEntity>();
            var categories = new List<CategoryEntity>();
            var listCategoryConfigs = new List<CategoryConfigEntity>();

            categories.Add(category);

            categoryService.GetAll().Returns(categories);
            categoryConfigService.GetByCategory(Arg.Any<string>()).Returns(listCategoryConfigs);
            repository.FindAsync(Arg.Any<Expression<Func<MarketCategoryEntity, bool>>>()).Returns(marketCategories);

            // Act
            var result = await service.GetConfigs("market_id");

            // Assert
            result.Should().HaveCount(1);
            result.FirstOrDefault().Games.Should().HaveCount(0);
        }
    }
}
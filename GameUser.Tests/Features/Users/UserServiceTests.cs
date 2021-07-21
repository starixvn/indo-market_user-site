using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using GameUser.Features.Users;
using GameUser.Shared.Database;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace GameUser.Tests.Features.Users
{
    public class UserServiceTests
    {
        private readonly UserService service;
        private readonly IGenericRepository<UserEntity> repository;

        public UserServiceTests()
        {
            repository = Substitute.For<IGenericRepository<UserEntity>>();
            service = new UserService(repository);
        }

        [Fact]
        public async Task CreateUser_WhenCalled_ReturnUserEntity()
        {
            // Arrange
            var entity = new UserEntity
            {
                Local = new LocalUserEntity
                {
                    BrandName = "VG",
                    ClientId = "1234",
                    Currency = "IN",
                    IsCash = false,
                    MemberId = 1234,
                    MemberName = "Name",
                    SiteName = "123",
                    UserName = "username"
                }
            };

            repository.InsertOneAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(entity));
            repository.FindOneAsync(Arg.Any<Expression<Func<UserEntity, bool>>>()).Returns(entity);

            // Act
            var result = await service.CreateUser(entity);

            // Assert
            result.Should().Be(entity);
        }

        [Fact]
        public async Task GetUser_WhenCalled_ReturnUserEntity()
        {
            // Arrange
            var entity = new UserEntity
            {
                Local = new LocalUserEntity
                {
                    BrandName = "VG",
                    ClientId = "1234",
                    Currency = "IN",
                    IsCash = false,
                    MemberId = 1234,
                    MemberName = "Name",
                    SiteName = "123",
                    UserName = "username"
                }
            };

            repository.FindOneAsync(Arg.Any<Expression<Func<UserEntity, bool>>>()).Returns(entity);

            // Act
            var result = await service.GetUser(entity.Local.UserName);

            // Assert
            result.Should().Be(entity);
        }

        [Fact]
        public async Task UpdateBrandName_WhenCalled_ReturnUserEntity()
        {
            // Arrange
            var entity = new UserEntity
            {
                Local = new LocalUserEntity
                {
                    BrandName = "Saba",
                    ClientId = "1234",
                    Currency = "IN",
                    IsCash = false,
                    MemberId = 1234,
                    MemberName = "Name",
                    SiteName = "123",
                    UserName = "username"
                }
            };

            var expectedEnttiy = new UserEntity
            {
                Local = new LocalUserEntity
                {
                    BrandName = "VG",
                    ClientId = "1234",
                    Currency = "IN",
                    IsCash = false,
                    MemberId = 1234,
                    MemberName = "Name",
                    SiteName = "123",
                    UserName = "username"
                }
            };

            repository.UpdateOneAsync(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<UpdateDefinition<UserEntity>>()).Returns(Task.FromResult(entity));
            repository.FindOneAsync(Arg.Any<Expression<Func<UserEntity, bool>>>()).Returns(expectedEnttiy);

            // Act
            await service.UpdateBrandName(entity.Local.UserName, expectedEnttiy.Local.BrandName);
            var result = await service.GetUser(entity.Local.UserName);

            // Assert
            result.Local.BrandName.Should().Be(expectedEnttiy.Local.BrandName);
        }
    }
}
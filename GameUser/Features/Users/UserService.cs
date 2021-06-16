using System.Threading.Tasks;
using GameUser.Shared.Database;
using MongoDB.Driver;

namespace GameUser.Features.Users
{
    public interface IUserService
    {
        Task<UserEntity> CreateUser(UserEntity userEntity);

        Task<UserEntity> GetUser(string userName);

        Task UpdateBrandName(string username, string brandName);

        Task UpdateLanguage(string username, string langauge);
    }

    public class UserService : IUserService
    {
        private readonly IGenericRepository<UserEntity> userRepository;

        public UserService(IGenericRepository<UserEntity> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<UserEntity> CreateUser(UserEntity userEntity)
        {
            await userRepository.InsertOneAsync(userEntity);

            var user = await GetUser(userEntity.Local.UserName);

            return user;
        }

        public async Task<UserEntity> GetUser(string userName)
        {
            var user = await userRepository.FindOneAsync(user => user.Local.UserName == userName);

            return user;
        }

        public async Task UpdateBrandName(string username, string brandName)
        {
            var update = Builders<UserEntity>.Update
                .Set(user => user.Local.BrandName, brandName);

            await UpdateUser(username, update);
        }

        public async Task UpdateLanguage(string username, string langauge)
        {
            var update = Builders<UserEntity>.Update
                .Set(user => user.Local.Language, langauge);

            await UpdateUser(username, update);
        }

        protected async Task<UserEntity> UpdateUser(string username, UpdateDefinition<UserEntity> userUpdater)
        {
            await userRepository.UpdateOneAsync(user => user.Local.UserName == username, userUpdater);

            var user = await GetUser(username);

            return user;
        }
    }
}
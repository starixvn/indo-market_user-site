using System.Threading.Tasks;
using GameUser.Shared.Database;

namespace GameUser.Features.Users
{
    public interface IUserService
    {
        Task<UserEntity> CreateUser(UserEntity userEntity);

        Task<UserEntity> GetUser(string userName);
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
    }
}
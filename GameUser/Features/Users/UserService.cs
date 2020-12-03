using System.Threading.Tasks;
using GameUser.Shared.Database;

namespace GameUser.Features.Users
{
    public interface IUserService
    {
        Task CreateUser(UserEntity userEntity);

        Task<UserEntity> GetUser(string userName);
    }

    public class UserService : IUserService
    {
        private readonly IGenericRepository<UserEntity> userRepository;

        public UserService(IGenericRepository<UserEntity> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task CreateUser(UserEntity userEntity)
        {
            await this.userRepository.InsertOneAsync(userEntity);
        }

        public async Task<UserEntity> GetUser(string userName)
        {
            var user = await this.userRepository.FindOneAsync(user => user.Local.UserName == userName);

            return user;
        }
    }
}
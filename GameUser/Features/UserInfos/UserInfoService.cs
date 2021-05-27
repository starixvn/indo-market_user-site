using GameUser.Shared.Database;
using System.Threading.Tasks;

namespace GameUser.Features.UserInfos
{
    public interface IUserInfoService
    {
        Task<UserInfoEntity> GetUserInfo(string userId);
    }

    public class UserInfoService : IUserInfoService
    {
        private readonly IGenericRepository<UserInfoEntity> repository;

        public UserInfoService(IGenericRepository<UserInfoEntity> repository)
        {
            this.repository = repository;
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            var userInfo = await repository.FindOneAsync(info => info.UserId == userId);

            return userInfo;
        }
    }
}
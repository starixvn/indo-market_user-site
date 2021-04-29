using GameUser.Shared.Database;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace GameUser.Features.UserInfos
{
    public interface IUserInfoService
    {
        Task<UserInfoEntity> GetUserInfo(string userId);

        Task<UserInfoEntity> GetUserInfoFromCache(string userId);
    }

    public class UserInfoService : IUserInfoService
    {
        private readonly IGenericRepository<UserInfoEntity> repository;
        private readonly UserInfoSettings settings;
        private readonly IMemoryCache memoryCache;

        public UserInfoService(IGenericRepository<UserInfoEntity> repository, IMemoryCache memoryCache, UserInfoSettings settings)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
            this.settings = settings;
        }

        public async Task<UserInfoEntity> GetUserInfo(string userId)
        {
            var userInfo = await repository.FindOneAsync(info => info.UserId == userId);

            return userInfo;
        }

        public async Task<UserInfoEntity> GetUserInfoFromCache(string userId)
        {
            var info = await memoryCache.GetOrCreateAsync(userId, cache =>
            {
                var timeout = TimeSpan.FromMinutes(settings.CacheTimeout);

                cache.SetSlidingExpiration(timeout);

                return GetUserInfo(userId);

            });

            return info;
        }
    }
}
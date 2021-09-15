using System.Collections.Generic;
using System.Threading.Tasks;
using GameUser.Shared.Database;
using MongoDB.Driver;

namespace GameUser.Features.FavoriteGame
{
    public interface IFavoriteGameService
    {
        Task<FavoriteGameEntity> GetFavoriteGames(string userId);

        Task AddFavoriteGame(string userId, int gameId);

        Task RemoveFavoriteGame(string userId, int gameId);
    }

    public class FavoriteGameService : IFavoriteGameService
    {
        private readonly IGenericRepository<FavoriteGameEntity> repository;

        public FavoriteGameService(IGenericRepository<FavoriteGameEntity> repository)
        {
            this.repository = repository;
        }

        public async Task AddFavoriteGame(string userId, int gameId)
        {
            await CreateFavoriteGameIfNotExist(userId);

            var entity = await GetFavoriteGames(userId);
            var games = entity.Games;

            if (!games.Contains(gameId))
            {
                games.Add(gameId);
            }

            var update = Builders<FavoriteGameEntity>.Update.Set(f => f.Games, games);

            await repository.UpdateOneAsync(f => f.UserId == userId, update);
        }

        public async Task RemoveFavoriteGame(string userId, int gameId)
        {
            var entity = await GetFavoriteGames(userId);
            var games = entity.Games;

            games.Remove(gameId);

            var update = Builders<FavoriteGameEntity>.Update.Set(f => f.Games, games);

            await repository.UpdateOneAsync(f => f.UserId == userId, update);
        }

        public async Task<FavoriteGameEntity> GetFavoriteGames(string userId)
        {
            var entity = await repository.FindOneAsync(f => f.UserId == userId);

            return entity ?? new FavoriteGameEntity
            {
                UserId = userId,
                Games = new List<int>()
            };
        }

        private async Task CreateFavoriteGameIfNotExist(string userId)
        {
            var entity = await repository.FindOneAsync(f => f.UserId == userId);

            if (entity == null)
            {
                await repository.InsertOneAsync(new FavoriteGameEntity
                {
                    UserId = userId,
                    Games = new List<int>()
                });
            }
        }
    }
}
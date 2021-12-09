namespace GameUser.Features.GameExtraConfigs
{
    using GameUser.Shared.Database;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public interface IGameExtraConfigService
    {
        Task<IEnumerable<GameExtraConfigEntity>> GetGameExtraConfigs();
    }
    public class GameExtraConfigService : IGameExtraConfigService
    {
        private readonly IGenericRepository<GameExtraConfigEntity> repository;

        public GameExtraConfigService(IGenericRepository<GameExtraConfigEntity> repository)
        {
            this.repository = repository;
        }

        public Task<IEnumerable<GameExtraConfigEntity>> GetGameExtraConfigs()
              => repository.FindAsync(entity => true);
    }
}

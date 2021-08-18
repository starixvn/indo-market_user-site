using System.Collections.Generic;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Shared.Database;

namespace GameUser.Features.CategoryConfigs.Services
{
    public interface ICategoryConfigService
    {
        Task<IEnumerable<CategoryConfigEntity>> GetByCategory(string categoryId);
    }

    public class CategoryConfigService : ICategoryConfigService
    {
        private readonly IGenericRepository<CategoryConfigEntity> repository;

        public CategoryConfigService(IGenericRepository<CategoryConfigEntity> repository)
        {
            this.repository = repository;
        }

        public Task<IEnumerable<CategoryConfigEntity>> GetByCategory(string categoryId)
            => repository.FindAsync(entity => entity.Active && entity.CategoryId == categoryId);
    }
}
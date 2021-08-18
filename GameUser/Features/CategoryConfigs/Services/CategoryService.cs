using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Shared.Database;

namespace GameUser.Features.CategoryConfigs.Services
{
    public interface ICategoryService
    {
        Task<IList<CategoryEntity>> GetAll();

        Task<CategoryEntity> GetById(string categoryId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<CategoryEntity> repository;

        public CategoryService(IGenericRepository<CategoryEntity> repository)
        {
            this.repository = repository;
        }

        public async Task<IList<CategoryEntity>> GetAll()
         => (await repository.FindAsync(entity => true)).OrderBy(entity => entity.SortOrder).ToList();

        public Task<CategoryEntity> GetById(string categoryId)
            => repository.FindOneAsync(entity => entity.Id.ToString() == categoryId);
    }
}
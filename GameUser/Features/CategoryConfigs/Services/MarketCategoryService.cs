using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.CategoryConfigs.Entities;
using GameUser.Features.CategoryConfigs.Models;
using GameUser.Shared.Database;
using MongoDB.Driver;

namespace GameUser.Features.CategoryConfigs.Services
{
    public interface IMarketCategoryService
    {
        Task<IEnumerable<CategoryConfigModel>> GetConfigs(string marketId);
    }

    public class MarketCategoryService : IMarketCategoryService
    {
        private readonly IGenericRepository<MarketCategoryEntity> repository;
        private readonly ICategoryConfigService categoryConfigService;
        private readonly ICategoryService categoryService;

        public MarketCategoryService(
            IGenericRepository<MarketCategoryEntity> repository,
            ICategoryConfigService categoryConfigService,
            ICategoryService categoryService)
        {
            this.repository = repository;
            this.categoryConfigService = categoryConfigService;
            this.categoryService = categoryService;
        }

        public async Task<IEnumerable<CategoryConfigModel>> GetConfigs(string marketId)
        {
            var mappings = await GetMappingsByMarket(marketId);
            var categories = await categoryService.GetAll();
            var result = new List<CategoryConfigModel>();

            foreach (var category in categories)
            {
                var config = await GetConfigByCateogry(mappings, category.Id.ToString());
                var icons = config?.GameIconsCollection ?? new List<GameIconEntity>();

                result.Add(new CategoryConfigModel(category, icons));
            }

            return result;
        }

        private async Task<CategoryConfigEntity> GetConfigByCateogry(IEnumerable<MarketCategoryEntity> mappings, string categoryId)
        {
            var configs = await categoryConfigService.GetByCategory(categoryId);
            var config = configs
                .Where(config => config.Default || mappings.Any(mapping => mapping.CategoryConfigId == config.Id.ToString()))
                .OrderBy(config => config.Default)
                .FirstOrDefault();

            return config;
        }

        private async Task<IList<MarketCategoryEntity>> GetMappingsByMarket(string marketId)
        {
            var mappings = await repository.FindAsync(mapping => mapping.MarketId == marketId);

            return mappings.ToList();
        }
    }
}
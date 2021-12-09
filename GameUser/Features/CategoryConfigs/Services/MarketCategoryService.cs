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
        Task<IEnumerable<CategoryConfigModel>> GetConfigs(string marketId, bool isCash);
    }

    public class MarketCategoryService : IMarketCategoryService
    {
        private readonly IGenericRepository<MarketCategoryEntity> marketCategoryRepository;
        private readonly ICategoryConfigService categoryConfigService;
        private readonly ICategoryService categoryService;

        public MarketCategoryService(
            IGenericRepository<MarketCategoryEntity> repository,
            ICategoryConfigService categoryConfigService,
            ICategoryService categoryService)
        {
            this.marketCategoryRepository = repository;
            this.categoryConfigService = categoryConfigService;
            this.categoryService = categoryService;
        }

        public async Task<IEnumerable<CategoryConfigModel>> GetConfigs(string marketId, bool isCash)
        {
            var marketCategories = await GetMarketCategories(marketId);
            var categories = await categoryService.GetAll();
            var result = new List<CategoryConfigModel>();

            foreach (var category in categories)
            {
                var config = await GetConfigCategory(marketCategories, category.Id.ToString(), isCash);
                var icons = config?.GameIconsCollection ?? new List<GameIconEntity>();
                result.Add(new CategoryConfigModel(category, icons));
            }

            return result;
        }

        private async Task<CategoryConfigEntity> GetConfigCategory(IList<MarketCategoryEntity> marketCategories, string categoryId, bool isCash)
        {
            var configs = await categoryConfigService.GetByCategory(categoryId);
            var configsToList = configs.ToList();
            var marketCategory = GetMarketCategorySettingMapping(configsToList, marketCategories, isCash);
            var config = configsToList
                .Where(config => config.Default || Equals(config.Id.ToString(), marketCategory?.CategoryConfigId))
                .OrderBy(config => config.Default)
                .FirstOrDefault();

            return config;
        }

        private static MarketCategoryEntity GetMarketCategorySettingMapping (List<CategoryConfigEntity> categoryConfigs,
            IList<MarketCategoryEntity> marketCategories, bool isCash)
        {
            var marketSettingsMapping = marketCategories.Where(m => categoryConfigs.Any(c => c.Id.ToString() == m.CategoryConfigId)).ToList();
            var customerType = CustomerType.FromIsCash(isCash);
            var anyCashMarketCategoryConfig = marketSettingsMapping.Any(m => Equals(m.OfferedCustomerType, customerType.Value));
            var selectedCustomerType = anyCashMarketCategoryConfig ? customerType.Value : CustomerType.AllValue;
            return marketSettingsMapping.FirstOrDefault(map => Equals(map.OfferedCustomerType, selectedCustomerType));
        }

        private async Task<IList<MarketCategoryEntity>> GetMarketCategories(string marketId)
        {
            var marketCategories = await marketCategoryRepository.FindAsync(mapping => mapping.MarketId == marketId);
            return marketCategories.ToList();
        }      
    }
}
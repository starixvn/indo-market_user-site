using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.CategoryConfigs.Entities
{
    [BsonIgnoreExtraElements]
    [BsonCollection("market_categories")]
    public class MarketCategoryEntity : Document
    {
        [BsonElement("marketId")]
        public string MarketId { get; set; }

        [BsonElement("categoryConfigId")]
        public string CategoryConfigId { get; set; }
    }
}
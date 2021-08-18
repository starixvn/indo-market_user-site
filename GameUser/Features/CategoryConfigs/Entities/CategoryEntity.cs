using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.CategoryConfigs.Entities
{
    [BsonIgnoreExtraElements]
    [BsonCollection("game_categories")]
    public class CategoryEntity : Document
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("key")]
        public string Key { get; set; }

        [BsonElement("sortOrder")]
        public int SortOrder { get; set; }
    }
}
using System.Collections.Generic;
using GameUser.Shared.Database;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.CategoryConfigs.Entities
{
    [BsonIgnoreExtraElements]
    [BsonCollection("category_configs")]
    public class CategoryConfigEntity : Document
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("active")]
        public bool Active { get; set; }

        [BsonElement("default")]
        public bool Default { get; set; }

        [BsonElement("categoryId")]
        public string CategoryId { get; set; }

        [BsonElement("gameIconsCollection")]
        public IList<GameIconEntity> GameIconsCollection { get; set; }
    }
}
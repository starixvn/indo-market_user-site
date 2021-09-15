using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.CategoryConfigs.Entities
{
    [BsonCollection("game_icon")]
    public class GameIconEntity
    {
        [BsonElement("gameName")]
        public string Name { get; set; }

        [BsonElement("gameId")]
        public int GameId { get; set; }

        [BsonElement("iconSize")]
        public string IconSize { get; set; }

        [BsonElement("sortOrder")]
        public int SortOrder { get; set; }

        [BsonElement("tag")]
        public string Tag { get; set; }
    }
}
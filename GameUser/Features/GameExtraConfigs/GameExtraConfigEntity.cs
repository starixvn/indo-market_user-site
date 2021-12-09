namespace GameUser.Features.GameExtraConfigs
{
    using GameUser.Shared.Database;
    using MongoDB.Bson.Serialization.Attributes;

    [BsonIgnoreExtraElements]
    [BsonCollection("game_extra_configs")]
    public class GameExtraConfigEntity : Document
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("gameId")]
        public int GameId { get; set; }

        [BsonElement("value")]
        public string Value { get; set; }
    }
}

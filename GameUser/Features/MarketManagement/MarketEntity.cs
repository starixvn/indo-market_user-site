using System.Collections.Generic;
using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.MarketManagement
{
    [BsonIgnoreExtraElements]
    [BsonCollection("markets")]
    public class MarketEntity : Document
    {
        [BsonElement("marketName")]
        public string MarketName { get; set; }

        [BsonElement("enabled")]
        public bool Enabled { get; set; }

        [BsonElement("currencies")]
        public IEnumerable<string> Currencies { get; set; }
    }
}
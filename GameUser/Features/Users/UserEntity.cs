using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.Users
{
    [BsonIgnoreExtraElements]
    [BsonCollection("users")]
    public class UserEntity: Document
    {
        [BsonElement("local")]
        public LocalUserEntity Local { get; set; }

        [BsonElement("__v")]
        public int Version { get; set; }
    }
}
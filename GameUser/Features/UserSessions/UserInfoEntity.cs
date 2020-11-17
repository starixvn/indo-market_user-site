using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.UserSessions
{
    [BsonIgnoreExtraElements]
    [BsonCollection("userinfos")]
    public class UserInfoEntity : Document
    {
        [BsonElement("__v")]
        public int Version { get; set; }

        [BsonElement("id")]
        public string UserId { get; set; }

        [BsonElement("avatarId")]
        public int AvatarId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}

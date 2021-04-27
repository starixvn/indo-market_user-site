using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GameUser.Features.UserInfos
{
    [BsonIgnoreExtraElements]
    class UserInfoEntity: Document
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("UID")]
        public int UID { get; set; }

        [BsonElement("avatarId")]
        public int AvatarId { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("id")]
        public string UserId { get; set; }

        [BsonElement("joinedOn")]
        public DateTime JoinedOn { get; set; }

        [BsonElement("red")]
        public double Balance { get; set; }

        [BsonElement("type")]
        public bool Type { get; set; }
    }
}

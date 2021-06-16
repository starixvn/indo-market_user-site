using System;
using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.UserInfos
{
    [BsonIgnoreExtraElements]
    [BsonCollection("userinfos")]
    public class UserInfoEntity : Document
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
        public double Red { get; set; }

        [BsonElement("type")]
        public bool Type { get; set; }

        [BsonElement("isCash")]
        public bool IsCash { get; set; }

        [BsonElement("brandName")]
        public string BrandName { get; set; }
    }
}
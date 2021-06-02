using System;
using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.Users
{
    [BsonIgnoreExtraElements]
    public class LocalUserEntity : Document
    {
        [BsonElement("username")]
        public string UserName { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("regDate")]
        public DateTime RegDate { get; set; }

        [BsonElement("memberId")]
        public int MemberId { get; set; }

        [BsonElement("memberName")]
        public string MemberName { get; set; }

        [BsonElement("clientId")]
        public string ClientId { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("isCash")]
        public bool IsCash { get; set; }

        [BsonElement("brandName")]
        public string BrandName { get; set; }

        [BsonElement("siteName")]
        public string SiteName { get; set; }

        [BsonElement("language")]
        public string Language { get; set; }
    }
}
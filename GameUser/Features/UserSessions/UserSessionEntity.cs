using GameUser.Shared.Attributes;
using GameUser.Shared.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GameUser.Features.UserSessions
{
    [BsonCollection("usersessions")]
   public class UserSessionEntity : Document
    {
        [BsonElement("ssoToken")]
        public string SSOToken { get; set; }

        [BsonElement("sessionId")]
        public string SessionId { get; set; }

        [BsonElement("owSeq")]
        public string OWSeq { get; set; }

        [BsonElement("username")]
        public string UserName { get; set; }

        [BsonElement("memberId")]
        public int MemberId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("ssoTime")]
        public DateTime SSOTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("recheckInTime")]
        public DateTime ReCheckInTime { get; set; }

        [BsonElement("ip")]
        public string Ip { get; set; }

        [BsonElement("browserUserAgent")]
        public string BrowserUserAgent { get; set; }

        [BsonElement("siteId")]
        public string SiteId { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("language")]
        public string Language { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }

        [BsonElement("clientId")]
        public string ClientId { get; set; }

        [BsonElement("memberName")]
        public string MemberName { get; set; }

        [BsonElement("__v")]
        public int Version { get; set; }
    }
}

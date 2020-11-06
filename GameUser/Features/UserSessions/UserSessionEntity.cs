using GameUser.Shared.Attributes;
using GameUser.Shared.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GameUser.Features.UserSessions
{
    [BsonCollection("usersessions")]
   public class UserSessionEntity : Document
    {
        [BsonElement("sessionId")]
        public string SessionId { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("recheckInTime")]
        public DateTime ReCheckInTime { get; set; }
    }
}

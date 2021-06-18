namespace GameUser.Features.Annoucements
{
    using GameUser.Shared.Database;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;

    [BsonIgnoreExtraElements]
    [BsonCollection("announcements")]
    public class AnnouncementEntity: Document
    {
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("contents")]
        public Dictionary<string, string> Contents { get; set; }

        [BsonElement("messageType")]
        public string MessageType { get; set; }

        [BsonElement("status")]
        public bool Status { get; set; }

        [BsonElement("enabledMarkets")]
        public List<string> EnabledMarkets { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("time")]
        public DateTime Time { get; set; }

        public int __v { get; set; }
    }
}

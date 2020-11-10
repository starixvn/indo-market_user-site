using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GameUser.Features.Feedbacks
{
    [BsonCollection("feedbacks")]
    public class FeedbackEntity : Document
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Time { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Nickname { get; set; }
    }
}

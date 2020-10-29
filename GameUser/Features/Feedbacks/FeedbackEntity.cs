using GameUser.Attributes;
using GameUser.Shared.Entities;
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
        public int MemberId { get; set; }
        public string MemberName { get; set; }
    }
}

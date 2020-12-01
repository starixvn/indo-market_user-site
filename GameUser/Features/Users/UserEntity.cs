using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameUser.Features.Users
{
    [BsonIgnoreExtraElements]
    [BsonCollection("users")]
    public class UserEntity
    {
        [BsonElement("local")]
        public LocalUserEntity Local { get; set; }

        [BsonElement("__v")]
        public int Version { get; set; }
    }
}

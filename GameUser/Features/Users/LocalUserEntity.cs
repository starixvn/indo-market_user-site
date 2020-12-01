using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameUser.Features.Users
{
    
    public class LocalUserEntity: Document
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
        public int ClientId { get; set; }

        [BsonElement("currency")]
        public string Currency { get; set; }
    }
}

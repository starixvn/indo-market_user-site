using System.Collections.Generic;
using GameUser.Shared.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace GameUser.Features.FavoriteGame
{
    [BsonIgnoreExtraElements]
    [BsonCollection("favorite_game")]
    public class FavoriteGameEntity : Document
    {
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("games")]
        public IList<int> Games { get; set; }
    }
}
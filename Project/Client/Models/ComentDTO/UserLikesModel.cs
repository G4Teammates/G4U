using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models.ComentDTO
{
    public class UserLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}

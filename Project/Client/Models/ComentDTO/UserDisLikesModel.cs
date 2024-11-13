using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models.ComentDTO
{
    public class UserDisLikesModel
    {
        [BsonElement("userName")]
        public string UserName { get; set; }
    }
}

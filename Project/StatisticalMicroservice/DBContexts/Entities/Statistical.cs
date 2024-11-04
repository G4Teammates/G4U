using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StatisticalMicroservice.DBContexts.Entities
{
    public class Statistical
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("totalWebsite")]
        public TotalWebsiteInfo TotalWebsite { get; set; }
        /*[BsonElement("totalUser")]
        public List<UserInfo> TotalUser { get; set; }*/
        [BsonElement("createAt")]
        public DateTime CreateAt { get; set; }
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}

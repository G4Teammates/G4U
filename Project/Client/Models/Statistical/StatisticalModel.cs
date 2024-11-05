using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace Client.Models.Statistical
{
    public class StatisticalModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("totalWebsite")]
        public TotalWebsiteInfoModel TotalWebsite { get; set; }
        /*[BsonElement("totalUser")]
        public List<UserInfo> TotalUser { get; set; }*/
        [BsonElement("createAt")]
        public DateTime CreateAt { get; set; }
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;

namespace StatisticalMicroservice.DBContexts.Entities
{
    public class UserInfo
    {
        [BsonElement("userName")]   
        public string UserName { get; set; }
        [BsonElement("revenue")]
        public int Revenue { get; set; }
        [BsonElement("views")]
        public int Views { get; set; }
        [BsonElement("products")]
        public int Products { get; set; }
        [BsonElement("solds")]
        public int Solds { get; set; }
    }
}

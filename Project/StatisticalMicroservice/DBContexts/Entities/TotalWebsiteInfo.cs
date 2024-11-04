using MongoDB.Bson.Serialization.Attributes;

namespace StatisticalMicroservice.DBContexts.Entities
{
    public class TotalWebsiteInfo
    {
        [BsonElement("totalRevenue")]
        public int TotalRevenue { get; set; }
        [BsonElement("totalViews")]
        public int TotalViews { get; set; }
        [BsonElement("totalProducts")]
        public int TotalProducts { get; set; }
        [BsonElement("totalSolds")]
        public int TotalSolds { get; set; }
    }
}

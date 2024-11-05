using MongoDB.Bson.Serialization.Attributes;

namespace Client.Models.Statistical
{
    public class TotalWebsiteInfoModel
    {
        [BsonElement("totalRevenue")]
        public int TotalRevenue { get; set; }
        [BsonElement("totalViews")]
        public int TotalViews { get; set; }
        [BsonElement("totalProducts")]
        public int TotalProducts { get; set; }
        [BsonElement("totalSolds")]
        public int TotalSolds { get; set; }
        [BsonElement("totalUsers")]
        public int TotalUsers { get; set; }
    }
}

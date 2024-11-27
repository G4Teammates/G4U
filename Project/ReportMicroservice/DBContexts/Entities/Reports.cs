using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ReportMicroservice.DBContexts.Enum;

namespace ReportMicroservice.DBContexts.Entities
{
    public class Reports
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("username")]
        public required string UserName { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("related")]
        public string Related { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("createAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("status")]
        public ReportsStatus Status { get; set; }

    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Client.Models.Enum.ReportEnum;

namespace Client.Models
{
    public class ReportsModel
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public required string UserName { get; set; }

        public string Description { get; set; }

        public string Related { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public ReportsStatus Status { get; set; } 
    }
}

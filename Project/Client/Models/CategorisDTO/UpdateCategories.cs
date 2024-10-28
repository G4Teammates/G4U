using CategoryMicroservice.DBContexts.Enum;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.CategorisDTO
{
    public class UpdateCategories
    {
        
        public string Id { get; set; }


        public required string Name { get; set; }

        public required CategoryType Type { get; set; }


        public string? Description { get; set; }


        public CategoryStatus Status { get; set; }
    }
}

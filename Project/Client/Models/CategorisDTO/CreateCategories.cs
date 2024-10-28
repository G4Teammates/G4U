using CategoryMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.CategorisDTO
{
    public class CreateCategories
    {
        public required string Name { get; set; }

        public required CategoryType Type { get; set; }


        public string? Description { get; set; }


        public CategoryStatus Status { get; set; }
    }
}

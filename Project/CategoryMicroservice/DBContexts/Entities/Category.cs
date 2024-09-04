using CategoryMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.DBContexts.Entities
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage ="Name is require")]
        [MaxLength(100)]
        public required string Name { get; set; }
        public CategoryType Type { get; set; }
        public string? Description { get; set; }
        public CategoryStatus Status { get; set; }
        public virtual ICollection<CategoryDetail>? CategoryDetails { get; set; }
    }
}

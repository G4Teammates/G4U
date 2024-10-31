using CategoryMicroservice.DBContexts.Enum;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.CategorisDTO
{
    public class CategoriesModel
    {
        /// <summary>
        /// Unique identifier for the category.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Name of the category.
        /// </summary>
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(32, ErrorMessage = "Category name must be at least {2} and at most {1} characters long.", MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Category name cannot contain numbers or special characters.")]
        public required string Name { get; set; }

        /// <summary>
        /// The Type of the category.
        /// </summary>
        [Required(ErrorMessage = "Category type is required.")]
        public required CategoryType Type { get; set; }

        /// <summary>
        /// The Description of the category.
        /// </summary>
        [MaxLength(100000, ErrorMessage = "Description must be at most {1} characters long.")]
        public string? Description { get; set; }

        /// <summary>
        /// The Status of the category.
        /// </summary>
        public CategoryStatus Status { get; set; }
    }
}

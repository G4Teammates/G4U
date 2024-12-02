
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
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "The Name must be at least {2} and at max {1} characters long.")]
        [RegularExpression(@"^(?!.*\s{2})[a-zA-Z0-9\s]+$", ErrorMessage = "The Name cannot contain special characters or consecutive spaces.")]
        public string Name { get; set; }


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

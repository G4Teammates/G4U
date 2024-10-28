using CategoryMicroservice.DBContexts.Enum;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.CategorisDTO
{
    public class CategoriesModel
    {
        /// <summary>
        /// Unique identifier for the category.
        /// <br/>
        /// Định danh duy nhất cho danh mục.
        /// </summary>
        public string Id { get; set; } 

        /// <summary>
        /// The Name of the category.
        /// <br/>
        /// Tên danh mục.
        /// </summary>
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        public required string Name { get; set; }
        /// <summary>
        /// The Type of the category.
        /// <br/>
        /// Loại danh mục.
        /// </summary>
        /// 


        public required CategoryType Type { get; set; }

        /// <summary>
        /// The Description of the category.
        /// <br/>
        /// Mô tả danh mục.
        /// </summary>
        [MaxLength(100000, ErrorMessage = "The {0} must be max {1} characters long.")]
        public string? Description { get; set; }

        /// <summary>
        /// The Status of the category.
        /// <br/>
        /// Trạng thái danh mục.
        /// </summary>
        public CategoryStatus Status { get; set; }
    }
}

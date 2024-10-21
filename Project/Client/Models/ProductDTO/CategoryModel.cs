using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
   public class CategoryModel
    {
        /// <summary>
        /// The unique identifier of the category.
        /// <br/>
        /// Mã định danh duy nhất của danh mục.
        /// </summary>
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        public string CategoryName { get; set; }
    }
}

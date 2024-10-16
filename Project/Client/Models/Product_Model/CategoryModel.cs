namespace Client.Models.Product_Model
{
    /// <summary>
    /// Represents a category to which products can be assigned.
    /// <br/>
    /// Đại diện cho một danh mục mà các sản phẩm có thể được gán vào.
    /// </summary>
    public class CategoryModel
    {
        /// <summary>
        /// The unique identifier of the category.
        /// <br/>
        /// Mã định danh duy nhất của danh mục.
        /// </summary>
        public string? CategoryName { get; set; }
    }
}

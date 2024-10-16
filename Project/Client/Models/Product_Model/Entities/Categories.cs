using Newtonsoft.Json;

namespace Client.Models.Product_Model.Entities
{
    public class Categories
    {
        /// <summary>
        /// The unique identifier of the category.
        /// <br/>
        /// Mã định danh duy nhất của danh mục.
        /// </summary>
          [JsonProperty("categoryName")]
        public string? CategoryName { get; set; }
    }
}

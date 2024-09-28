using MongoDB.Bson.Serialization.Attributes;

namespace ProductMicroservice.DBContexts.Entities
{
    public class Categories
    {
        /// <summary>
        /// The unique identifier of the category.
        /// <br/>
        /// Mã định danh duy nhất của danh mục.
        /// </summary>
        [BsonElement("categoryId")]
        public string? CategoryId { get; set; }
    }
}

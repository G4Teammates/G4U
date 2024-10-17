using CategoryMicroservice.DBContexts.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CategoryMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Category
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();
    //    [Required(ErrorMessage ="Name is require")]
    //    [MaxLength(100)]
    //    public required string Name { get; set; }
    //    public CategoryType Type { get; set; }
    //    public string? Description { get; set; }
    //    public CategoryStatus Status { get; set; }
    //    public virtual ICollection<CategoryDetail>? CategoryDetails { get; set; }
    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents a category in the system.
    /// <br/>
    /// Đại diện cho một danh mục trong hệ thống.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique identifier for the category.
        /// <br/>
        /// Định danh duy nhất cho danh mục.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// The name of the category.
        /// <br/>
        /// Tên của danh mục.
        /// </summary>
        [BsonElement("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The type of the category such as: Tag, Category, Genre
        /// <br/>
        /// Loại của danh mục bao gồm: Tag, Category, Genre
        /// </summary>
        [BsonElement("type")]
        public CategoryType Type { get; set; }

        /// <summary>
        /// A description of the category.
        /// <br/>
        /// Mô tả của danh mục.
        /// </summary>
        [BsonElement("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The status of the category such as: Active, Inactive, Block, Deleted
        /// <br/>
        /// Trạng thái của danh mục bao gồm : Active, Inactive, Block, Deleted
        /// </summary>
        [BsonElement("status")]
        public CategoryStatus Status { get; set; }

    }

    #endregion
}

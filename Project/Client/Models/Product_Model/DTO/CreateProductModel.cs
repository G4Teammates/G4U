using Client.Models.Product_Model.Enum;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.Product_Model.DTO
{
    public class CreateProductModel
    {
        /*public string Id { get; set; } = ObjectId.GenerateNewId().ToString();*/

        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string Name { get; set; } = string.Empty;

        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal Price { get; set; }


        /*[Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int Sold { get; set; }*/


        /*public InteractionModel? Interactions { get; set; }*/

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; }

        /*[BsonElement("links")]
        public ICollection<CreateLinkModel>? Links { get; set; }*/

        public ICollection<CategoryModel>? Categories { get; set; }

        public PlatformType Platform { get; set; }


        public ProductStatus Status { get; set; }


        /*public DateTime CreatedAt { get; set; } = DateTime.UtcNow;*/


        /*public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;*/

        /*public required string UserId { get; set; }*/

        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
        public ScanFileRequest Request { get; set; } = new ScanFileRequest();
        public decimal GetPrice()
        {
            return Price - Price * (decimal)Discount / 100;
        }
    }
}

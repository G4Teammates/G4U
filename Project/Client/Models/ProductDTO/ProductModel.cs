using Client.Models.Enum.ProductEnum;
using MongoDB.Bson;
using ProductMicroservice.Models;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
    public class ProductModel
    {

        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "The Name must be at least {2} and at max {1} characters long.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "The Name cannot contain special characters.")]   /*Khong dc chua ky tu dac biet*/
        public required string Name { get; set; }



        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "The {0} must be a valid positive integer without any special characters or decimal points.")]
        public required decimal Price { get; set; }



        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int Sold { get; set; } = 0;

        public InteractionModel? Interactions { get; set; } = new InteractionModel { NumberOfLikes = 0, NumberOfViews = 0 };

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; } = 0;

        public PlatformType Platform { get; set; } = PlatformType.Unknown;

        public ICollection<LinkModel>? Links { get; set; }

        public ICollection<CategoryModel>? Categories { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Inactive;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public required string UserName { get; set; }


		public string QrCode { get; set; }

        /*public string BarCode { get; set; }*/

		public decimal GetPrice()
        {
            return Price - (Price * (decimal)Discount / 100);
        }
    }
}

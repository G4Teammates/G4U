using Client.Models.Enum.ProductEnum;
using Client.Models.ProductDTO;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
    public class UpdateProductModel
    {

        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "The Name must be at least {2} and at max {1} characters long.")]
        [RegularExpression(@"^(?!.*\s{2})[a-zA-Z0-9\s]+$", ErrorMessage = "The Name cannot contain special characters or consecutive spaces.")]
        public string Name { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Sold { get; set; }
        public UpdateInteractionModel? Interactions { get; set; } = new UpdateInteractionModel();
        public float Discount { get; set; }
        public List<LinkModel> Links { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public PlatformType Platform { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } = new List<IFormFile>(); // Để xử lý file upload
        public IFormFile? gameFile { get; set; } // Giả sử bạn có model này cho game file 
        public string UserName { get; set; }
    }
}

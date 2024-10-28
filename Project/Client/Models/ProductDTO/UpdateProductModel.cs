
using Client.Models.Enum.ProductEnum;
using Client.Models.ProductDTO;
using ProductMicroservice.Models;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
    public class UpdateProductModel
    {

            public string Id { get; set; }
            public string Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public int Sold { get; set; }
            public InteractionModel? Interactions { get; set; } = new InteractionModel();
            public float Discount { get; set; }
            public List<LinkModel> Links { get; set; }
            public List<string> Categories { get; set; } = new List<string>();
            public PlatformType Platform { get; set; }
            public ProductStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<IFormFile>? ImageFiles { get; set; } // Để xử lý file upload
            public IFormFile? gameFile { get; set; } // Giả sử bạn có model này cho game file 
            public string UserName { get; set; }
    }
}

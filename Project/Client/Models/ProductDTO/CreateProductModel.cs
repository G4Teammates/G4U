using Client.Models.Enum.ProductEnum;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Client.Models.ProductDTO
{
    public class CreateProductModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "The Name must be at least {2} and at max {1} characters long.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "The Name cannot contain special characters.")]   /*Khong dc chua ky tu dac biet*/
        public string Name { get; set; }

        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal Price { get; set; }

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; }

        public List<string> Categories { get; set; } = new List<string>();

        public int Platform { get; set; } // Sử dụng enum nếu có sẵn

        public int Status { get; set; } // Sử dụng enum nếu có sẵn

        public List<IFormFile> imageFiles { get; set; } = new List<IFormFile>();

        public IFormFile gameFile { get; set; } // Giả sử bạn có model này cho game file



        public string? Username { get; set; } // Tên người dùng


    }
}

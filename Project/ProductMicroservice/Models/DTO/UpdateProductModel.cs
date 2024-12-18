﻿using ProductMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProductMicroservice.Models.DTO
{
    public class UpdateProductModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string Name { get; set; }


        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal Price { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int Sold { get; set; }


        public InteractionModel? Interactions { get; set; }

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        [Range(0, 100, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public float Discount { get; set; }

        public ICollection<LinkModel> Links {  get; set; }   
        public ICollection<CategoryModel>? Categories { get; set; }

        public PlatformType Platform { get; set; } = PlatformType.Unknown;


        public ProductStatus Status { get; set; } = ProductStatus.Inactive;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? WinrarPassword { get; set; }

        public required string UserName { get; set; }

        public decimal GetPrice()
        {
            return Price - (Price * (decimal)Discount / 100);
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ProductMicroservice.Models.Message
{
    public class OrderItemsResponse
    {
        public ICollection<ProductSoldModel> ProductSoldModels { get; set; } 
        public bool IsExist { get; set; }

    }
}

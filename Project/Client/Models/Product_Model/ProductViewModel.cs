using Client.Models.Product_Model.DTO;
using Client.Models.Product_Model.Entities;

namespace Client.Models.Product_Model
{
    public class ProductViewModel
    {
        public CreateProductModel? CreateProductModel { get; set; }
        public UpdateProductModel? UpdateProductModel { get; set; }
        public ICollection<ProductModel?> Products { get; set; }
        public Products Product { get; set; }
    }
}

using Client.Models.Product_Model.DTO;

namespace Client.Models.Product_Model
{
    public class ProductViewModel
    {
        public CreateProductModel? CreateProductModel { get; set; }
        public UpdateProductModel? UpdateProductModel { get; set; }
        public ICollection<ProductModel?> Products { get; set; }
    }
}

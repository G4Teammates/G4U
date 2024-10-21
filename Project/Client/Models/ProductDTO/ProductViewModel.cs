

namespace Client.Models.ProductDTO
{
    public class ProductViewModel
    {
        public CreateProductModel? CreateProductModel { get; set; }
        public UpdateProductModel? UpdateProductModel { get; set; }
        public ICollection<ProductModel>? Product { get; set; }
    }
}

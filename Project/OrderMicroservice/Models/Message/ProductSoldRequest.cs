namespace OrderMicroservice.Models.Message
{
    public class ProductSoldRequest
    {
        public ICollection<ProductSoldModel> ProductSoldModels { get; set; }
        public bool IsExist { get; set; }
    }
}

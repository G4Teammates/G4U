using Client.Models.Enum.OrderEnum;
using Client.Models.ProductDTO;

namespace Client.Models.OrderModel
{
    public class CartModel
    {
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Pending;
        public OrderModel? Order { get; set; }
        public ICollection<ProductModel> Products { get; set; }

    }
}

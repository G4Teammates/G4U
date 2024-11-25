using OrderMicroservice.Models.OrderModel;

namespace OrderMicroservice.Models.PaymentModel.PayOsModel
{
    public class VietQRRequest
    {
        public string Id { get; set; }
        public long Amount { get; set; }
        public ICollection<OrderItemModel> Items { get; set; }
    }
}

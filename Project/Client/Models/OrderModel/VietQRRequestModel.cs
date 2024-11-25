namespace Client.Models.OrderModel
{
    public class VietQRRequestModel
    {
        public string Id { get; set; }
        public long Amount { get; set; }
        public ICollection<OrderItemModel> Items { get; set; }
    }
}

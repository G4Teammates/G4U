namespace Client.Models.OrderModel
{
    public class OrderViewModel
    {
        public ICollection<OrderModel> Orders { get; set; }
        public ICollection<OrderItemModel> Items { get; set; }
    }
}

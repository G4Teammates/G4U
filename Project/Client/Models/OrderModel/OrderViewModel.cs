namespace Client.Models.OrderModel
{
    public class OrderViewModel
    {
        public ICollection<OrderModel> Orders { get; set; }
        public ICollection<OrderItemModel> Items { get; set; }

        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public int pageCount { get; set; }
    }
}

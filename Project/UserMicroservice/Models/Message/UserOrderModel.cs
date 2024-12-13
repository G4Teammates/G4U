namespace UserMicroservice.Models.Message
{
    public class UserOrderModel
    {
        public string PublisherName { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalProfit { get; set; }
    }
}

namespace Client.Models.OrderModel
{
    public class PaidModel
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatusModel Status { get; set; }
    }
}

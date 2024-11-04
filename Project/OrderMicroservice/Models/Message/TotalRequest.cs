namespace OrderMicroservice.Models.Message
{
    public class TotalRequest
    {
        public DateTime updateAt { get; set; }
        public decimal totalRevenue { get; set; }
    }
}

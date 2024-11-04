namespace ProductMicroservice.Models.Message
{
    public class TotalRequest
    {
        public int totalProducts { get; set; }

        public int totalSolds { get; set; }
        public int totalViews { get; set; }

        public DateTime updateAt { get; set; }
    }
}

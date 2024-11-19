namespace StatisticalMicroservice.Model.Message
{
    public class TotalGroupByUserResponse
    {
        public OrderGroupByUserData orderdata { get; set; }
        public ProductGroupByUserData productdata { get; set; }
    }
}

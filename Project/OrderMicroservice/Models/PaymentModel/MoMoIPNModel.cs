namespace OrderMicroservice.Models.PaymentModel
{
    public class MoMoIPNModel
    {
        public string partnerCode { get; set; }
        public string accessKey { get; set; }
        public long amount { get; set; }
        public string partnerRefId { get; set; }
        public string partnerTransId { get; set; }
        public string transType { get; set; }
        public string momoTransId { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        public long responseTime { get; set; }
        public string signature { get; set; }
        public string storeId { get; set; }
    }


}

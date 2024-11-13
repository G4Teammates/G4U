namespace OrderMicroservice.Models.PaymentModel.MoMo
{
    public class MoMoSignature
    {
        public string AccessKey { get; set; }
        public long Amount { get; set; }
        public string? OrderId { get; set; }
        public string? PartnerCode { get; set; }
        public long? TransId { get; set; }


        public MoMoSignature(MoMoRequest request, string accessKey)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            Amount = request.amount;
            OrderId = request.orderId;
            PartnerCode = request.partnerCode;
            AccessKey = accessKey;
        }  
        public MoMoSignature(MoMoIPNResquest request, string accessKey)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            Amount = request.Amount;
            OrderId = request.OrderId;
            PartnerCode = request.PartnerCode;
            AccessKey = accessKey;
        } 
        public MoMoSignature()
        {
        }
    }

}

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace OrderMicroservice.Models.PaymentModel.MoMo
{
    public class MoMoRequest
    {
        public string? orderInfo { get; set; }
        public string? partnerCode { get; set; }
        public string? redirectUrl { get; set; }
        public string? ipnUrl { get; set; }
        public long amount { get; set; }
        public string? orderId { get; set; }
        public string? requestId { get; set; }
        public string? extraData { get; set; }
        public string? partnerName { get; set; }
        public string? storeId { get; set; }
        public string? paymentCode { get; set; }
        public string? orderGroupId { get; set; }
        public bool autoCapture { get; set; }
        public string? lang { get; set; }
        public string? signature { get; set; }
        public string? requestType { get; set; }



    }

}

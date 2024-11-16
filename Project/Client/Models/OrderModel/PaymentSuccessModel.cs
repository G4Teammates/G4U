using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace Client.Models.OrderModel
{
    public class PaymentSuccessModel
    {
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public string TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }
        public DateTime ResponseLocalDateTime =>  DateTimeOffset.FromUnixTimeMilliseconds(ResponseTime).LocalDateTime;
        public DateTime ResponseUtcDateTime =>  DateTimeOffset.FromUnixTimeMilliseconds(ResponseTime).UtcDateTime;
    }
}


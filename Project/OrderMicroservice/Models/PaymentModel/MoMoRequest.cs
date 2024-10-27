using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace OrderMicroservice.Models.PaymentModel
{
    public class MoMoRequest
    {
        public string? orderInfo { get; set; }
        public string? partnerCode { get; set; }
        public string? redirectUrl { get; set; }
        public string? ipnUrl { get; set; }
        public decimal amount { get; set; }
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

        public string MakeSignature(string accessKey, string secretKey)
        {
            // Tạo chuỗi `rawHash` từ các giá trị cần có trong chữ ký
            string rawHash = "accessKey=" + accessKey +
                             "&amount=" + amount +
                             "&extraData=" + (extraData ?? "") +
                             "&ipnUrl=" + (ipnUrl ?? "") +
                             "&orderId=" + orderId +
                             "&orderInfo=" + orderInfo +
                             "&partnerCode=" + partnerCode +
                             "&redirectUrl=" + (redirectUrl ?? "") +
                             "&requestId=" + requestId +
                             "&requestType=" + requestType;

            UTF8Encoding encoding = new UTF8Encoding();

            // Chuyển đổi chuỗi `rawHash` và `secretKey` thành mảng byte
            byte[] textBytes = encoding.GetBytes(rawHash);
            byte[] keyBytes = encoding.GetBytes(secretKey);

            byte[] hashBytes;

            // Sử dụng HMACSHA256 để tính chữ ký
            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            // Trả về chuỗi `signature` dưới dạng hexadecimal, tất cả chữ thường
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

    }

}

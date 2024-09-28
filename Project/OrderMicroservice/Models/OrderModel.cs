using MongoDB.Bson;
using OrderMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderMicroservice.Models
{
    /// <summary>
    /// Represents an order placed by a user, including details about payment, status, and items purchased.
    /// <br/>
    /// Đại diện cho một đơn hàng được đặt bởi người dùng, bao gồm thông tin về thanh toán, trạng thái, và các mặt hàng đã mua.
    /// </summary>
    public class OrderModel
    {
        /// <summary>
        /// Unique identifier for the order.
        /// <br/>
        /// Định danh duy nhất cho đơn hàng.
        /// </summary>
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// Payment Transaction Id. Obtained from Payment 3rd Party Service (e.g., PayPal, Stripe, Bank).
        /// <br/>
        /// Mã giao dịch thanh toán lấy từ dịch vụ thanh toán bên thứ ba (ví dụ: PayPal, Stripe, Ngân hàng).
        /// </summary>
        public required string PaymentTransactionId { get; set; }

        /// <summary>
        /// Total price of the order.
        /// <br/>
        /// Tổng giá trị của đơn hàng.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The {0} must have no more than 2 decimal places")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Total profit from the order.
        /// <br/>
        /// Tổng lợi nhuận từ đơn hàng.
        /// </summary>
        public decimal TotalProfit => TotalPrice * 0.9m;

        /// <summary>
        /// Status of the order, including: Pending, Processing, Shipping, Completed, Cancelled.
        /// <br/>
        /// Trạng thái của đơn hàng, bao gồm: Đang chờ, Đang xử lý, Đang vận chuyển, Hoàn thành, Đã hủy.
        /// </summary>
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        
        /// <summary>
        /// Name of the payment method (e.g., Momo, Vnpay, ViettinBank).
        /// <br/>
        /// Tên phương thức thanh toán (ví dụ: Momo, Vnpay, ViettinBank).
        /// </summary>
        public required string PaymentName { get; set; }

        /// <summary>
        /// Status of the payment, including: Pending, Paid, Failed.
        /// <br/>
        /// Trạng thái của thanh toán, bao gồm: Đang chờ, Đã thanh toán, Thất bại.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Method of payment, including: CreditCard, DebitCard, NetBanking, UPI, Wallet.
        /// <br/>
        /// Phương thức thanh toán, bao gồm: Thẻ tín dụng, Thẻ ghi nợ, Ngân hàng trực tuyến, UPI, Ví điện tử.
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Pending;

        /// <summary>
        /// The date and time when the order was created.
        /// <br/>
        /// Ngày và giờ khi đơn hàng được tạo ra.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the order was last updated.
        /// <br/>
        /// Ngày và giờ khi đơn hàng được cập nhật lần cuối.
        /// </summary>
        public DateTime UpdateAt { get; set; }
        public required string UserId { get; set; }
        public required ICollection<OrderItemModel> Items { get; set; }
    }
}

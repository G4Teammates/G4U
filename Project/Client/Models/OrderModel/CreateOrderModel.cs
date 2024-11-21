using Client.Models.Enum.OrderEnum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.OrderModel
{
    public class CreateOrderModel
    {

        /// <summary>
        /// Payment Transaction Id. Obtained from Payment 3rd Party Service (e.g., PayPal, Stripe, Bank).
        /// <br/>
        /// Mã giao dịch thanh toán lấy từ dịch vụ thanh toán bên thứ ba (ví dụ: PayPal, Stripe, Ngân hàng).
        /// </summary>
        public string? PaymentTransactionId { get; set; }

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
        [JsonIgnore]
        public decimal TotalProfit => TotalPrice - (TotalPrice * 0.1m);

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
        public string PaymentName { get; set; } = "Pending";

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
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The identifier of the user who placed the order.
        /// <br/>
        /// Định danh của người dùng đã đặt đơn hàng.
        /// </summary>
        public string? CustomerId { get; set; }

        /// <summary>
        /// A list of items included in the order.
        /// <br/>
        /// Danh sách các sản phẩm có trong đơn hàng.
        /// </summary>
        public ICollection<OrderItemModel>? Items { get; set; }

    }
}

using Client.Models.Enum.OrderEnum;

namespace Client.Models.OrderModel
{
    public class PaymentStatusModel
    {
        /// <summary>
        /// Status of the order, including: Pending, Processing, Shipping, Completed, Cancelled.
        /// <br/>
        /// Trạng thái của đơn hàng, bao gồm: Đang chờ, Đang xử lý, Đang vận chuyển, Hoàn thành, Đã hủy.
        /// </summary>
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;


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
        /// Name of the payment method (e.g., Momo, Vnpay, ViettinBank).
        /// <br/>
        /// Tên phương thức thanh toán (ví dụ: Momo, Vnpay, ViettinBank).
        /// </summary>
        public string PaymentName { get; set; } = "Pending";


    }
}

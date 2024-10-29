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


    }
}

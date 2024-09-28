using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrderMicroservice.DBContexts.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderMicroservice.DBContexts.Entities
{
    #region SQL
    //public class Order
    //{
    //    public Guid Id { get; set; }
    //    public string PaymentTransactionId { get; set; }
    //    [Column(TypeName = "decimal(18,2)")]
    //    public decimal TotalPrice { get; set; }
    //    public OrderStatus OrderStatus { get; set; }
    //    public PaymentStatus PaymentStatus { get; set; }
    //    public required string PaymentMethod { get; set; }

    //    public DateTime CreatedAt { get; set; }
    //    public DateTime UpdatedAt { get; set; }
    //    //Collection Table
    //    public Guid CartId { get; set; }
    //    //Product Table
    //    public Guid ProductId { get; set; }

    //}
    #endregion

    #region noSQL
    /// <summary>
    /// Represents an order placed by a user, including details about payment, status, and items purchased.
    /// <br/>
    /// Đại diện cho một đơn hàng được đặt bởi người dùng, bao gồm thông tin về thanh toán, trạng thái, và các mặt hàng đã mua.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier for the order.
        /// <br/>
        /// Định danh duy nhất cho đơn hàng.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        /// <summary>
        /// Payment Transaction Id. Obtained from Payment 3rd Party Service (e.g., PayPal, Stripe, Bank).
        /// <br/>
        /// Mã giao dịch thanh toán lấy từ dịch vụ thanh toán bên thứ ba (ví dụ: PayPal, Stripe, Ngân hàng).
        /// </summary>
        [BsonElement("paymentTransactionId")]
        public required string PaymentTransactionId { get; set; }

        /// <summary>
        /// Total price of the order.
        /// <br/>
        /// Tổng giá trị của đơn hàng.
        /// </summary>
        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Total profit from the order.
        /// <br/>
        /// Tổng lợi nhuận từ đơn hàng.
        /// </summary>
        [BsonElement("totalProfit")]
        public decimal TotalProfit { get; set; }

        /// <summary>
        /// Status of the order, including: Pending, Processing, Shipping, Completed, Cancelled.
        /// <br/>
        /// Trạng thái của đơn hàng, bao gồm: Đang chờ, Đang xử lý, Đang vận chuyển, Hoàn thành, Đã hủy.
        /// </summary>
        [BsonElement("orderStatus")]
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// Name of the payment method (e.g., Momo, Vnpay, ViettinBank).
        /// <br/>
        /// Tên phương thức thanh toán (ví dụ: Momo, Vnpay, ViettinBank).
        /// </summary>
        [BsonElement("paymentName")]
        public required string PaymentName { get; set; }

        /// <summary>
        /// Status of the payment, including: Pending, Paid, Failed.
        /// <br/>
        /// Trạng thái của thanh toán, bao gồm: Đang chờ, Đã thanh toán, Thất bại.
        /// </summary>
        [BsonElement("paymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// Method of payment, including: CreditCard, DebitCard, NetBanking, UPI, Wallet.
        /// <br/>
        /// Phương thức thanh toán, bao gồm: Thẻ tín dụng, Thẻ ghi nợ, Ngân hàng trực tuyến, UPI, Ví điện tử.
        /// </summary>
        [BsonElement("paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// The date and time when the order was created.
        /// <br/>
        /// Ngày và giờ khi đơn hàng được tạo ra.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the order was last updated.
        /// <br/>
        /// Ngày và giờ khi đơn hàng được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The identifier of the user who placed the order.
        /// <br/>
        /// Định danh của người dùng đã đặt đơn hàng.
        /// </summary>
        [BsonElement("customerId")]
        public required string CustomerId { get; set; }

        /// <summary>
        /// A list of items included in the order.
        /// <br/>
        /// Danh sách các sản phẩm có trong đơn hàng.
        /// </summary>
        [BsonElement("items")]
        public required ICollection<OrderItems> Items { get; set; }
    }

    #endregion 
}

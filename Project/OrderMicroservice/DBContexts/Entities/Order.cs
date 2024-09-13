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
    public class Order
    {
        /// <summary>
        /// Id of Order
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }

        /// <summary>
        /// Payment Transaction Id. Get from Payment 3rd Party Service (Paypal, Stripe, Bank..)
        /// </summary>
        [BsonElement("paymentTransactionId")]
        public required string PaymentTransactionId { get; set; }

        /// <summary>
        /// Total Price of Order
        ///</summary>>
        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Status of Order includes: Pending, Processing, Shipping, Completed, Cancelled
        /// </summary>
        [BsonElement("orderStatus")]
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// Name of Payment(Momo, Vnpay, ViettinBank,...)
        /// </summary>
        [BsonElement("paymentName")]
        public required string PaymentName { get; set; }

        /// <summary>
        /// Status of Payment includes: Pending, Paid, Failed
        /// </summary>
        [BsonElement("paymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// Method of Payment includes: CreditCard, DebitCard, NetBanking, UPI, Wallet
        /// </summary>
        [BsonElement("paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// List Product Id of Order
        /// </summary>
        //Get from Collection Table
        [BsonElement("products")]
        public required List<Guid> Products { get; set; }

        /// <summary>
        /// User Id of Order
        /// </summary>
        [BsonElement("userId")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The date and time when the order was created.    
        ///  <br/>
        /// Ngày và giờ khi đơn hàng được tạo ra.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// The date and time when the order was last updated status.
        /// <br/>
        /// Ngày và giờ khi đơn hàng cập nhật trạng thái lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        
    }
    #endregion 
}

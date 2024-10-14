using MongoDB.Bson.Serialization.Attributes;

namespace OrderMicroservice.DBContexts.Entities
{
    /// <summary>
    /// Represents an item within an order, including product details and quantity.
    /// <br/>
    /// Đại diện cho một sản phẩm trong đơn hàng, bao gồm thông tin sản phẩm và số lượng.
    /// </summary>
    public class OrderItems
    {
        /// <summary>
        /// The unique identifier of the product.
        /// <br/>
        /// Mã định danh duy nhất của sản phẩm.
        /// </summary>
        [BsonElement("productId")]
        public required string ProductId { get; set; }

        public required string ProductName { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal TotalProfit { get; set; }

        public required string PublisherId { get; set; }

        /*public */

        /// <summary>
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }

}

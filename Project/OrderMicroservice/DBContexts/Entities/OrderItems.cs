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

        /// <summary>
        /// Name of product
        /// <br/>
        /// Tên của sản phẩm
        /// </summary>
        [BsonElement("productName")]
        public required string ProductName { get; set; }

        /// <summary>
        /// Unit price of each product
        /// <br/>
        /// Giá của 1 sản phẩm
        /// </summary>
        [BsonElement("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Total price of each order include many product
        /// <br/>
        /// Tổng giá của mỗi hóa đơn bao gồm nhiều sản phẩm
        /// <br/>
        /// <see cref="TotalPrice"/> = <see cref="Price"/> * <see cref="Quantity"/>
        /// </summary>
        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// The total profit of publisher on each product. Publisher receive 90% total price product
        /// <br/>
        /// Tổng lợi nhuận của người đăng game trên mỗi sản phẩm. Người đăng game nhận 90% tổng giá trị của sản phẩm
        /// <br/>
        /// <see cref="TotalProfit"/> = <see cref="TotalPrice"/> * <see cref="0.9"/>
        /// </summary>
        [BsonElement("totalProfit")]
        public decimal TotalProfit { get; set; }

        /// <summary>
        /// The id of publisher
        /// <br/>
        /// Mã định danh của người đăng game
        /// </summary>
        [BsonElement("publisherId")]
        public required string PublisherId { get; set; }
        
        /// <summary>
        /// Username of publisher
        /// <br/>
        /// Username của người đăng game
        /// </summary>
        public required string PublisherName { get; set; }


        /// <summary>
        /// The quantity of the product ordered.
        /// <br/>
        /// Số lượng sản phẩm được đặt hàng.
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }

}

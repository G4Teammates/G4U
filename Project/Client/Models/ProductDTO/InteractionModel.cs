using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
    public class InteractionModel
    {
        /// <summary>
        /// The number of times the item has been played or accessed.
        /// <br/>
        /// Số lần sản phẩm đã được chơi hoặc truy cập.
        /// </summary>
        /*[Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int NumberOfPlays { get; set; } = 0;*/


        /// <summary>
        /// The number of views the product has received.
        /// <br/>
        /// Số lượt xem mà sản phẩm đã nhận được.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int NumberOfViews { get; set; }

        /// <summary>
        /// The number of likes or positive feedback the item has received.
        /// <br/>
        /// Số lượt thích hoặc phản hồi tích cực mà sản phẩm nhận được.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int NumberOfLikes { get; set; } = 0;
    }
}

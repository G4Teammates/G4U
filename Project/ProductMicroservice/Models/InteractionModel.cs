using System.ComponentModel.DataAnnotations;

namespace ProductMicroservice.Models
{
    /// <summary>
    /// Represents the interactions or engagements of users with an item, 
    /// such as plays, likes, and other forms of user feedback.
    /// <br/>
    /// Đại diện cho các tương tác hoặc sự tham gia của người dùng với một mục, 
    /// chẳng hạn như số lần chơi, lượt thích, và các phản hồi khác từ người dùng.
    /// </summary>
    public class InteractionModel
    {
        /// <summary>
        /// The number of times the item has been played or accessed.
        /// <br/>
        /// Số lần sản phẩm đã được chơi hoặc truy cập.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int NumberOfPlays { get; set; }

        /// <summary>
        /// The number of likes or positive feedback the item has received.
        /// <br/>
        /// Số lượt thích hoặc phản hồi tích cực mà sản phẩm nhận được.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "The {0} must be greater or equal than {1}")]
        public int NumberOfLikes { get; set; }

    }
}

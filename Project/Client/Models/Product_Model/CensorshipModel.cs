using Client.Models.Product_Model.Enum;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.Product_Model
{
    /// <summary>
    /// Represents the censorship information related to the product or content.
    /// <br/>
    /// Đại diện cho thông tin kiểm duyệt liên quan đến sản phẩm hoặc nội dung.
    /// </summary>
    public class CensorshipModel
    {
        /// <summary>
        /// The name of the provider responsible for the censorship or classification.
        /// <br/>
        /// Tên của nhà cung cấp chịu trách nhiệm kiểm duyệt hoặc phân loại.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string ProviderName { get; set; }

        /// <summary>
        /// An optional description of the censorship decision or classification.
        /// <br/>
        /// Mô tả tùy chọn về quyết định kiểm duyệt hoặc phân loại.
        /// </summary>
        [MaxLength(10000, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }

        /// <summary>
        /// The status or result of the censorship classification.
        /// <br/>
        /// Trạng thái hoặc kết quả của phân loại kiểm duyệt.
        /// </summary>
        public CensorshipStatus Status { get; set; }
    }
}

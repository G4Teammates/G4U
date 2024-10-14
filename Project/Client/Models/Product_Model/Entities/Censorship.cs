using Client.Models.Product_Model.Enum;

namespace Client.Models.Product_Model.Entities
{
    /// <summary>
    /// Represents the censorship information related to the product or content.
    /// <br/>
    /// Đại diện cho thông tin kiểm duyệt liên quan đến sản phẩm hoặc nội dung.
    /// </summary>
    public class Censorship
    {
        /// <summary>
        /// The name of the provider responsible for the censorship or classification.
        /// <br/>
        /// Tên của nhà cung cấp chịu trách nhiệm kiểm duyệt hoặc phân loại.
        /// </summary>
        public required string ProviderName { get; set; }

        /// <summary>
        /// An optional description of the censorship decision or classification.
        /// <br/>
        /// Mô tả tùy chọn về quyết định kiểm duyệt hoặc phân loại.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The status or result of the censorship classification.
        /// <br/>
        /// Trạng thái hoặc kết quả của phân loại kiểm duyệt.
        /// </summary>
        public CensorshipStatus Status { get; set; }
    }
}

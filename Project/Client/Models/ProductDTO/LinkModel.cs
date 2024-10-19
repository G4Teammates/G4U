using Client.Models.Enum.ProductEnum;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.ProductDTO
{
    public class LinkModel
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        /// <summary>
        /// The name of the link provider (social media or other platforms such as Facebook, Google Drive, TerraBox, GitHub, etc.).<br/>
        /// Tên của nhà cung cấp liên kết (mạng xã hội hoặc các nền tảng khác như Facebook, Google Drive, TerraBox, GitHub, v.v.).
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public required string ProviderName { get; set; }

        /// <summary>
        /// The URL of the link.<br/>
        /// URL của liên kết.
        /// </summary>
        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public required string Url { get; set; }

        /// <summary>
        /// The type of the link, which could represent various categories (e.g., social or product-related).<br/>
        /// Loại của liên kết, có thể đại diện cho các danh mục khác nhau (ví dụ: liên quan đến mạng xã hội hoặc sản phẩm).
        /// </summary>
        public LinkType Type { get; set; }

        /// <summary>
        /// The status of the link (Active, Inactive, Block, Deleted).<br/>
        /// Trạng thái của liên kết (Kích hoạt, Không kích hoạt, Bị chặn, Đã xóa).
        /// </summary>
        public LinkStatus Status { get; set; }

        /// <summary>
        /// Represents the censorship information related to the product or content.
        /// <br/>
        /// Đại diện cho thông tin kiểm duyệt liênnn quan đến sản phẩm hoặc nội dung.
        /// </summary>
        public required CensorshipModel Censorship { get; set; }
    }
}

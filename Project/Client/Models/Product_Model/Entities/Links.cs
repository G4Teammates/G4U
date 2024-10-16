using Client.Models.Product_Model.Enum;
using Newtonsoft.Json;

namespace Client.Models.Product_Model.Entities
{
    /// <summary>
    /// Represents a Link entity in the system. Each link is associated with a user and can optionally be tied to a product.<br/>
    /// Đại diện cho một thực thể liên kết trong hệ thống. Mỗi liên kết được gắn với một người dùng và có thể được liên kết với một sản phẩm.
    /// </summary>
    public class Links
    {
        /// <summary>
        /// Unique identifier for the link.<br/>
        /// Định danh duy nhất cho liên kết.
        /// </summary>
        [JsonProperty("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The name of the link provider (social media or other platforms such as Facebook, Google Drive, TerraBox, GitHub, etc.).<br/>
        /// Tên của nhà cung cấp liên kết (mạng xã hội hoặc các nền tảng khác như Facebook, Google Drive, TerraBox, GitHub, v.v.).
        /// </summary>
        [JsonProperty("providerName")]
        public required string ProviderName { get; set; }

        /// <summary>
        /// The URL of the link.<br/>
        /// URL của liên kết.
        /// </summary>
        [JsonProperty("url")]
        public required string Url { get; set; }

        /// <summary>
        /// The type of the link, which could represent various categories (e.g., social or product-related).<br/>
        /// Loại của liên kết, có thể đại diện cho các danh mục khác nhau (ví dụ: liên quan đến mạng xã hội hoặc sản phẩm).
        /// </summary>
        [JsonProperty("type")]
        public LinkType Type { get; set; }

        /// <summary>
        /// The platform where the product is available (e.g., Window, Android, WebGL,...).
        /// <br/>
        /// Nền tảng nơi sản phẩm có sẵn (ví dụ: Window, Android, WebGL,...).
        /// </summary>
        /*[BsonElement("platform")]
        public PlatformType Platform { get; set; }*/

        /// <summary>
        /// The status of the link (Active, Inactive, Block, Deleted).<br/>
        /// Trạng thái của liên kết (Kích hoạt, Không kích hoạt, Bị chặn, Đã xóa).
        /// </summary>
        [JsonProperty("status")]
        public LinkStatus Status { get; set; }

        /// <summary>
        /// Represents the censorship information related to the product or content.
        /// <br/>
        /// Đại diện cho thông tin kiểm duyệt liên quan đến sản phẩm hoặc nội dung.
        /// </summary>
        [JsonProperty("censorship")]
        public required Censorship Censorship { get; set; }
    }
}

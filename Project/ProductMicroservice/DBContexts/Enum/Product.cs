namespace ProductMicroservice.DBContexts.Enum
{
    /// <summary>
    /// Represents the status of a product in the system.
    /// <br/>
    /// Đại diện cho trạng thái của một sản phẩm trong hệ thống.
    /// </summary>
    public enum ProductStatus
    {
        /// <summary>
        /// The product is inactive and not available for sale or use.
        /// <br/>
        /// Sản phẩm không hoạt động và không có sẵn để bán hoặc sử dụng.
        /// </summary>
        Inactive,

        /// <summary>
        /// The product is active and available for sale or use.
        /// <br/>
        /// Sản phẩm đang hoạt động và có sẵn để bán hoặc sử dụng.
        /// </summary>
        Active,

        /// <summary>
        /// The product is blocked due to violations or restrictions.
        /// <br/>
        /// Sản phẩm bị khóa do vi phạm hoặc hạn chế.
        /// </summary>
        Block,

        /// <summary>
        /// The product has been permanently deleted from the system.
        /// <br/>
        /// Sản phẩm đã bị xóa vĩnh viễn khỏi hệ thống.
        /// </summary>
        Deleted
    }

    /// <summary>
    /// Represents the type of platform a product is compatible with.
    /// <br/>
    /// Đại diện cho loại nền tảng mà sản phẩm tương thích.
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// The platform type is unknown or unspecified.
        /// <br/>
        /// Loại nền tảng không xác định hoặc chưa được chỉ định.
        /// </summary>
        Unknown,

        /// <summary>
        /// The product is downloadable and can be installed on a device.
        /// <br/>
        /// Sản phẩm có thể tải xuống và cài đặt trên thiết bị.
        /// </summary>
        Downloadable,

        /// <summary>
        /// The product is HTML-based and can be viewed in a web browser.
        /// <br/>
        /// Sản phẩm dựa trên HTML và có thể xem trong trình duyệt web.
        /// </summary>
        HTML,

        /// <summary>
        /// The product is built with WebGL technology, primarily for 3D content in browsers.
        /// <br/>
        /// Sản phẩm được xây dựng bằng công nghệ WebGL, chủ yếu để hiển thị nội dung 3D trong trình duyệt.
        /// </summary>
        WebGL,

        /// <summary>
        /// The product is compatible with Windows operating systems.
        /// <br/>
        /// Sản phẩm tương thích với hệ điều hành Windows.
        /// </summary>
        Window,

        /// <summary>
        /// The product is compatible with Android operating systems.
        /// <br/>
        /// Sản phẩm tương thích với hệ điều hành Android.
        /// </summary>
        Android,

        /// <summary>
        /// The product is compatible with iOS operating systems.
        /// <br/>
        /// Sản phẩm tương thích với hệ điều hành iOS.
        /// </summary>
        iOS,

        /// <summary>
        /// The product is compatible with macOS operating systems.
        /// <br/>
        /// Sản phẩm tương thích với hệ điều hành macOS.
        /// </summary>
        MacOS,

        /// <summary>
        /// The product is compatible with Linux operating systems.
        /// <br/>
        /// Sản phẩm tương thích với hệ điều hành Linux.
        /// </summary>
        Linux,

        /// <summary>
        /// The product is compatible with other platforms not listed above.
        /// <br/>
        /// Sản phẩm tương thích với các nền tảng khác không được liệt kê ở trên.
        /// </summary>
        Other
    }

}

namespace Client.Models.Product_Model.Enum
{
    public enum LinkStatus
    {
        Active,
        Inactive,
        Block,
        Deleted
    }
    public enum LinkType
    {
        /// <summary>
        /// Liên kết nội bộ, dùng cho các server
        /// </summary>
        Internal,
        /// <summary>
        /// Liên kết với các dịch vụ third-party hoặc cloud
        /// </summary>
        External
    }
}

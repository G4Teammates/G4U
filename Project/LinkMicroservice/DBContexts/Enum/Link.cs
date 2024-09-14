namespace LinkMicroservice.DBContexts.Enum
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
        Internal,  // Liên kết nội bộ, dùng cho các server
        External   // Liên kết với các dịch vụ third-party hoặc cloud
    }

}

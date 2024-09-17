namespace ProductMicroservice.DBContexts.Enum
{
    public enum ProductStatus
    {
        Inactive,
        Active,
        Block,
        Deleted
    }

    public enum PlatformType
    {
        Unknown,
        Downloadable,
        HTML,
        WebGL,
        Window,
        Android,
        iOS,
        MacOS,
        Linux,
        Other
    }
}

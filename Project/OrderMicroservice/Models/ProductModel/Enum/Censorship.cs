namespace OrderMicroservice.Models.ProductModel.Enum
{
    public enum CensorshipStatus
    {
        /// <summary>
        /// Content is analysing and processing
        /// <br/>
        /// Nội dung này đang được phân tích và xử lí
        /// </summary>
        Processing,

        /// <summary>
        /// Content is accessible and has no restrictions.
        /// <br/>
        /// Nội dung có thể truy cập và không có hạn chế.
        /// </summary>
        Access,

        /// <summary>
        /// Content is flagged for virus or malware issues.
        /// <br/>
        /// Nội dung bị gắn cờ vì có vấn đề về virus hoặc phần mềm độc hại.
        /// </summary>
        Virus,

        /// <summary>
        /// Content is not supported by the platform.
        /// <br/>
        /// Nội dung không được nền tảng hỗ trợ.
        /// </summary>
        Unsupport,

        /// <summary>
        /// Content is flagged for promoting hate speech.
        /// <br/>
        /// Nội dung bị gắn cờ vì khuyến khích phát ngôn thù địch.
        /// </summary>
        Hate,

        /// <summary>
        /// Content is flagged for promoting self-harm.
        /// <br/>
        /// Nội dung bị gắn cờ vì khuyến khích tự gây hại.
        /// </summary>
        SelfHarm,

        /// <summary>
        /// Content is flagged for sexual content.
        /// <br/>
        /// Nội dung bị gắn cờ vì có nội dung tình dục.
        /// </summary>
        Sexual,

        /// <summary>
        /// Content is flagged for violent content.
        /// <br/>
        /// Nội dung bị gắn cờ vì có nội dung bạo lực.
        /// </summary>
        Violence
    }

}

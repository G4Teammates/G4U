namespace ProductMicroservice.Models.Initialization
{
    public static class initializationModel
    {
        public static string cloudNameCloudinary { get; set; } = "dl97ondjc";
        public static string apiKeyCloudinary { get; set; }
        public static string apiSecretCloudinary { get; set; }
        public static string EndpointContentSafety { get; set; }
        public static string ApiKeyContentSafety { get; set; }
        public static string VirusTotalApiKey { get; set; }
        public static string VirusTotalScanUrl { get; set; } = "https://www.virustotal.com/api/v3/files";
        public static string VirusTotalReportUrl { get; set; } = "https://www.virustotal.com/api/v3/files/{id}";
        public static string serviceAccountJsonContent { get; set; }
        public static string folderId {  get; set; }
    }
}

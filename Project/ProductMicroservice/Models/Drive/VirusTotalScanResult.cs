using Newtonsoft.Json;

namespace ProductMicroservice.Models.Drive
{
    public class VirusTotalScanResult
    {
        [JsonProperty("data")]
        public VirusTotalScanData Data { get; set; }
    }
    public class VirusTotalScanData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
    // Model cho kết quả báo cáo
    public class VirusTotalReportResult
    {
        [JsonProperty("data")]
        public VirusTotalReportData Data { get; set; }
    }

    public class VirusTotalReportData
    {
        [JsonProperty("attributes")]
        public VirusTotalReportAttributes Attributes { get; set; }
    }

    public class VirusTotalReportAttributes
    {
        [JsonProperty("last_analysis_stats")]
        public VirusTotalAnalysisStats LastAnalysisStats { get; set; }
    }

    public class VirusTotalAnalysisStats
    {
        [JsonProperty("harmless")]
        public int Harmless { get; set; }

        [JsonProperty("malicious")]
        public int Malicious { get; set; }

        [JsonProperty("suspicious")]
        public int Suspicious { get; set; }

        [JsonProperty("undetected")]
        public int Undetected { get; set; }
    }
    public class UploadUrlResult
    {
        [JsonProperty("data")]
        public string UploadUrl { get; set; }
    }
}

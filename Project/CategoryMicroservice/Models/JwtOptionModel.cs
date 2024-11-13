namespace CategoryMicroservice.Models
{
    public class JwtOptionModel
    {
        public static string Issuer { get; set; } = string.Empty;
        public static string Audience { get; set; } = string.Empty;
        public static string Secret { get; set; } = string.Empty;
    }
}

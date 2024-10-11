namespace Client.Utility
{
    public class StaticTypeApi
    {
        public const string? APIGateWay = "https://localhost:7260";
        public static string? ApiUrl { get; set; }
        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }

}

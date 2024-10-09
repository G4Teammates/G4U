namespace Client.Utility
{
    public class StaticTypeApi
    {
        public static string? ApiUrl { get; set; } = "https://localhost:7107/";
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

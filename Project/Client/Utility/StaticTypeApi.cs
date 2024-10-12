namespace Client.Utility
{
    public class StaticTypeApi
    {
        public static string? APIGateWay;
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

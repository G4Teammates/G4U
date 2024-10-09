using System.Security.AccessControl;
using static Client.Utility.StaticTypeApi;

namespace Client.Models
{
    public class RequestModel
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string? Url { get; set; }
        public object? Data { get; set; }
        public string? AccessToken { get; set; }
    }
}

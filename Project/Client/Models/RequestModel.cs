using Client.Models.ProductDTO;
using static Client.Utility.StaticTypeApi;

namespace Client.Models
{
    public class RequestModel
    {
        internal CreateProductModel data;

        public ApiType ApiType { get; set; } = ApiType.GET;
        public string? Url { get; set; }
        public object? Data { get; set; }
        public string? AccessToken { get; set; }
    }
}

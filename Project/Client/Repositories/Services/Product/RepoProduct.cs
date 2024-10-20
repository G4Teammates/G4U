using Client.Models;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Product;
using Client.Utility;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Client.Repositories.Services.Product
{
    public class RepoProduct : IRepoProduct
    {
        public readonly IBaseService _baseService;
        private readonly HttpClient _httpClient;
        public RepoProduct(IBaseService baseService, HttpClient httpClient)
        {
            _baseService = baseService;
            _httpClient = httpClient;
        }

        public async Task<ResponseModel> DeleteProductAsysnc(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Product/" + Id
            });
        }

        public async Task<ResponseModel?> GetAllProductAsync()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product"
            });
        }

        public async Task<ResponseModel?> GetProductByIdAsync(string Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product/" + Id
            });
        }

        public async Task<ResponseModel> UpdateProductAsync(List<IFormFile> imageFiles,UpdateProductModel product,  ScanFileRequest request)
        {
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.Sold,
                    product.Interactions.NumberOfLikes,
                    product.Interactions.NumberOfViews,
                    product.Discount,
                    Categories = product.Categories,
                    Platform = (int)product.Platform,
                    Status = (int)product.Status,
                    CreatedAt = product.CreatedAt,
                    UserName = product.UserName,
                    GameFile = request.gameFile, // Assuming gameFile is a string; adjust if necessary
                    ImageFiles = imageFiles // Add the image files
                },
                Url = StaticTypeApi.APIGateWay + "/Product"
            });

        }

        /*public async Task<ResponseModel> SearchProductAsync(string searchString)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/search={searchString}");
                if (response.IsSuccessStatusCode)
                {
                    // Đọc phản hồi và chuyển đổi sang ResponseModel
                    var content = await response.Content.ReadFromJsonAsync<ResponseModel>();

                    // Kiểm tra nếu thành công, thì lấy dữ liệu trong ResponseResultModel
                    if (content.IsSuccess)
                    {
                        var resultData = await response.Content.ReadFromJsonAsync<ResponseResultModel<List<ProductModel>>>();
                        content.Result = resultData.result; // Gán danh sách sản phẩm vào Result
                    }

                    return content;
                }

                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Error retrieving search results"
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> SortAsync(string sort)
        {
            var response = await _httpClient.GetAsync($"/Product/sort={sort}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResponseModel>();
        }

        public async Task<ResponseModel> FilterAsync(decimal? minrange, decimal? maxrange, int? sold, bool? discount, int? platform, string? category)
        {
            var query = $"?minrange={minrange}&maxrange={maxrange}&sold={sold}&discount={discount}&platform={platform}&category={category}";
            var response = await _httpClient.GetAsync($"/Product/filter{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResponseModel>();
        }*/

    }
}

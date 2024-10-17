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

        public async Task<ResponseModel> UpdateProductAsync(UpdateProductModel product, List<IFormFile> imageFiles, ScanFileRequest request)
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

    }
}

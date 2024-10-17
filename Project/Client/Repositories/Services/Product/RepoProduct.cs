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

        public async Task<ResponseModel?> UpdateProductAsync(string id,
                                                            string name,
                                                            string description,
                                                            decimal price,
                                                            int sold,
                                                            int numOfView,
                                                            int numOfLike,
                                                            float discount,
                                                            List<string> categories,
                                                            int platform,
                                                            int status,
                                                            DateTime createAt,
                                                            List<IFormFile> imageFiles,
                                                            ScanFileRequest request,
                                                            string username)
        {
            var formData = new MultipartFormDataContent();

            // Thêm các tham số vào formData
            formData.Add(new StringContent(id), "id");
            formData.Add(new StringContent(name), "name");
            formData.Add(new StringContent(description), "description");
            formData.Add(new StringContent(price.ToString()), "price");
            formData.Add(new StringContent(sold.ToString()), "sold");
            formData.Add(new StringContent(numOfView.ToString()), "numOfView");
            formData.Add(new StringContent(numOfLike.ToString()), "numOfLike");
            formData.Add(new StringContent(discount.ToString()), "discount");
            formData.Add(new StringContent(platform.ToString()), "platform");
            formData.Add(new StringContent(status.ToString()), "status");
            formData.Add(new StringContent(createAt.ToString("o")), "createAt");
            formData.Add(new StringContent(username), "username");

            // Thêm các danh mục vào formData
            foreach (var category in categories)
            {
                formData.Add(new StringContent(category), "categories");
            }

            // Thêm tệp hình ảnh vào formData
            foreach (var file in imageFiles)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(fileContent, "imageFiles", file.FileName);
            }

            // Thêm tệp trò chơi từ ScanFileRequest
            if (request != null && request.gameFile != null)
            {
                var gameFileContent = new StreamContent(request.gameFile.OpenReadStream());
                gameFileContent.Headers.ContentType = new MediaTypeHeaderValue(request.gameFile.ContentType);
                formData.Add(gameFileContent, "request.gameFile", request.gameFile.FileName);
            }

            // Gửi yêu cầu PUT thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Url = StaticTypeApi.APIGateWay + "/Product",
                Data = formData
            });

            return response;
        }

    }
}

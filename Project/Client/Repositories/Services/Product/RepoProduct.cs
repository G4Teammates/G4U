using Client.Models;
using Client.Models.ProductDTO;
using Client.Repositories.Interfaces.Product; // Đảm bảo không gian tên này được sử dụng
using Client.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductMicroservice.Repostories; // Giữ lại không gian tên này nếu cần
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Repositories.Interfaces;
using IRepoProduct = Client.Repositories.Interfaces.Product.IRepoProduct;

namespace Client.Repositories.Services.Product
{
    public class RepoProduct : IRepoProduct // Chỉ định không gian tên đầy đủ
    {
        public readonly IBaseService _baseService;
        private readonly HttpClient _httpClient;

        public RepoProduct(IBaseService baseService, HttpClient httpClient)
        {
            _baseService = baseService;
            _httpClient = httpClient;
        }

        public async Task<ResponseModel> CreateProductAsync(
            string name,
            string description,
            decimal price,
            float discount,
            List<string> categories,
            int platform,
            int status,
            List<IFormFile> imageFiles,
            ScanFileRequest request,
            string username)
        {
            // Tạo MultipartFormDataContent để gửi dữ liệu
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(name), "name");
            formData.Add(new StringContent(description ?? string.Empty), "description");
            formData.Add(new StringContent(price.ToString()), "price");
            formData.Add(new StringContent(discount.ToString()), "discount");
            formData.Add(new StringContent(platform.ToString()), "platform");
            formData.Add(new StringContent(status.ToString()), "status");
            formData.Add(new StringContent(username), "username");

            // Thêm các danh mục vào formData
            foreach (var category in categories)
            {
                formData.Add(new StringContent(category), "categories");
            }

            // Thêm các tệp hình ảnh vào formData
            foreach (var file in imageFiles)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "imageFiles",
                    FileName = file.FileName
                };
                formData.Add(fileContent);
            }

            // Thêm tệp game vào formData
            if (request?.gameFile != null)
            {
                var gameFileContent = new StreamContent(request.gameFile.OpenReadStream());
                gameFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "gameFile",
                    FileName = request.gameFile.FileName
                };
                formData.Add(gameFileContent);
            }

            // Gửi yêu cầu POST thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = formData,
                Url = StaticTypeApi.APIGateWay + "/Product"
            });

            return response;
        }



        //public async Task<ResponseModel> CreateProductAsync(
        //    string name,
        //    string description,
        //    decimal price,
        //    float discount,
        //    ICollection<CategoryModel> categories,
        //    int platform,
        //    int status,
        //    List<IFormFile> imageFiles,
        //    ScanFileRequest request,
        //    string username)
        //{
        //    // Tạo MultipartFormDataContent để gửi yêu cầu
        //    var formData = new MultipartFormDataContent();
        //    formData.Add(new StringContent(name), "name");
        //    formData.Add(new StringContent(description ?? string.Empty), "description");
        //    formData.Add(new StringContent(price.ToString()), "price");
        //    formData.Add(new StringContent(discount.ToString()), "discount");
        //    formData.Add(new StringContent(platform.ToString()), "platform");
        //    formData.Add(new StringContent(status.ToString()), "status");
        //    formData.Add(new StringContent(username), "username");
        //    formData.Add(new StringContent(categories.ToString()), "categories");

        //    // Gửi tệp hình ảnh
        //    foreach (var file in imageFiles)
        //    {
        //        var fileContent = new StreamContent(file.OpenReadStream())
        //        {
        //            Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
        //        };
        //        formData.Add(fileContent, "imageFiles", file.FileName);
        //    }

        //    // Gửi tệp game từ ScanFileRequest
        //    if (request?.gameFile != null)
        //    {
        //        var gameFileContent = new StreamContent(request.gameFile.OpenReadStream())
        //        {
        //            Headers = { ContentType = new MediaTypeHeaderValue(request.gameFile.ContentType) }
        //        };
        //        formData.Add(gameFileContent, "gameFile", request.gameFile.FileName);
        //    }

        //    // Gửi yêu cầu POST thông qua base service
        //    var response = await _baseService.SendAsync(new RequestModel()
        //    {
        //        ApiType = StaticTypeApi.ApiType.POST,
        //        Url = StaticTypeApi.APIGateWay + "/Product",
        //        Data = formData
        //    });

        //    return response;
        //}

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

        public async Task<ResponseModel> UpdateProductAsync(string id,
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

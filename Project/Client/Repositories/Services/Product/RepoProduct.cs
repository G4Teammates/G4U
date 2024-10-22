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

namespace Client.Repositories.Services.Product
{
    public class RepoProduct : Interfaces.Product.IRepoProduct // Chỉ định không gian tên đầy đủ
    {
        public readonly IBaseService _baseService;
        private readonly HttpClient _httpClient;

        public RepoProduct(IBaseService baseService, HttpClient httpClient)
        {
            _baseService = baseService;
            _httpClient = httpClient;
        }

        public async Task<ResponseModel> CreateProductAsync(List<IFormFile> imageFiles, CreateProductModel Product, IFormFile gameFiles, string username)
        {
            // Tạo MultipartFormDataContent để gửi dữ liệu
            var formData = new MultipartFormDataContent();

            // Thêm các thông tin sản phẩm vào formData
            formData.Add(new StringContent(Product.Name), "name");
            formData.Add(new StringContent(Product.Description ?? string.Empty), "description");
            formData.Add(new StringContent(Product.Price.ToString()), "price");
            formData.Add(new StringContent(Product.Discount.ToString()), "discount");
            formData.Add(new StringContent(((int)Product.Platform).ToString()), "platform");
            formData.Add(new StringContent(((int)Product.Status).ToString()), "status");
            formData.Add(new StringContent(username), "username");

            // Thêm các danh mục vào formData
            if (Product.Categories != null)
            {
                foreach (var category in Product.Categories)
                {
                    formData.Add(new StringContent(category.CategoryName.ToString()), "categories"); // Giả sử mỗi danh mục có thuộc tính Id
                }
            }

            // Thêm tệp hình ảnh vào formData
            foreach (var file in imageFiles)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                formData.Add(fileContent, "imageFiles", file.FileName);
            }

            // Thêm tệp game vào formData
            if (gameFiles != null)
            {
                var gameFileContent = new StreamContent(gameFiles.OpenReadStream());
                gameFileContent.Headers.ContentType = new MediaTypeHeaderValue(gameFiles.ContentType);
                formData.Add(gameFileContent, "gameFiles", gameFiles.FileName);
            }

            // Gửi yêu cầu POST thông qua base service
            var response = await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Url = StaticTypeApi.APIGateWay + "/Product",
                Data = formData
            });

            return response;
        }


        public Task<ResponseModel> DeleteProductAsync(string Id)
        {
            throw new NotImplementedException();
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

            public async Task<ResponseModel?> SearchProductAsync(string? query) // Sửa tên phương thức
            {
            // Thực hiện gửi yêu cầu tìm kiếm qua API Gateway
                return await _baseService.SendAsync(new RequestModel
                {
                    ApiType = StaticTypeApi.ApiType.GET,
                    Url = StaticTypeApi.APIGateWay + $"/Product/search" // Encode URL để tránh lỗi
                });
            }

        public Task<ResponseModel> UpdateProductAsync(List<IFormFile> imageFiles, UpdateProductModel product, ScanFileRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

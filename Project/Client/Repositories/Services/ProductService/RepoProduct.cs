using AutoMapper;
using Client.Models;
using Client.Models.Product_Model.Entities;
using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.ProductInterface;
using Client.Utility;
using static Client.Utility.StaticTypeApi;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Client.Models.Product_Model;
using Client.Models.Product_Model.Enum;
using Client.Models.Product_Model.DTO;

namespace Client.Repositories.Services.ProductService
{
    public class RepoProduct : IRepoProduct
    {
        public readonly IBaseService _baseService;
        public RepoProduct(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseModel?> CreateProductAsync(
                                                                string name,
                                                                string description,
                                                                decimal price,
                                                                float discount,
                                                                List<string> categories,
                                                                int platform,
                                                                int status,
                                                                List<IFormFile> imageFiles,
                                                                ScanFileRequest request)
        {
            // Chuẩn bị dữ liệu multipart form
            using var content = new MultipartFormDataContent();

            // Thêm tên, mô tả, giá cả và các thông tin khác vào multipart form
            content.Add(new StringContent(name), "name");
            content.Add(new StringContent(description), "description");
            content.Add(new StringContent(price.ToString()), "price");
            content.Add(new StringContent(discount.ToString()), "discount");
            content.Add(new StringContent(platform.ToString()), "platform");
            content.Add(new StringContent(status.ToString()), "status");

            // Thêm danh sách danh mục sản phẩm vào
            foreach (var category in categories)
            {
                content.Add(new StringContent(category), "categories");
            }

            // Thêm các tệp ảnh vào multipart form
            foreach (var file in imageFiles)
            {
                var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                content.Add(fileContent, "imageFiles", file.FileName);
            }

            // Thêm file game từ request vào
            if (request?.gameFile != null)
            {
                var gameFileStream = request.gameFile.OpenReadStream();
                var gameFileContent = new StreamContent(gameFileStream);
                content.Add(gameFileContent, "request.gameFile", request.gameFile.FileName);
            }

            // Gọi API với dữ liệu đã chuẩn bị
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = content,  // Sử dụng MultipartFormDataContent làm Data
                Url = StaticTypeApi.APIGateWay + "/Product"
            });
        }


        public async Task<ResponseModel?> DeleteProductAsync(int id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Product" + id
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

        public async Task<ResponseModel?> GetProductByIdAsync(int Id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,
                Url = StaticTypeApi.APIGateWay + "/Product" + Id
            });
        }


        public async Task<ResponseModel?> UpdateProductAsync(UpdateProductModel productDTO)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = productDTO,
                Url = StaticTypeApi.APIGateWay + "/Product"
            });
        }
    }
}

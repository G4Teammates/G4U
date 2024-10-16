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
        public async Task<ResponseModel?> CreateProductAsync(CreateProductModel createProduct)
        {
            // Chuẩn bị dữ liệu multipart form
            using var content = new MultipartFormDataContent();

            // Thêm tên, mô tả, giá cả và các thông tin khác vào multipart form
            content.Add(new StringContent(createProduct.Name), "name");

            if (!string.IsNullOrEmpty(createProduct.Description))
            {
                content.Add(new StringContent(createProduct.Description), "description");
            }

            content.Add(new StringContent(createProduct.Price.ToString()), "price");
            content.Add(new StringContent(createProduct.Discount.ToString()), "discount");
            content.Add(new StringContent(createProduct.Platform.ToString()), "platform");
            content.Add(new StringContent(createProduct.Status.ToString()), "status");

            // Thêm danh sách danh mục sản phẩm vào
            if (createProduct.Categories != null)
            {
                foreach (var category in createProduct.Categories)
                {
                    content.Add(new StringContent(category.CategoryName), "categories");
                }
            }

            // Thêm các tệp ảnh vào multipart form
            if (createProduct.ImageFiles != null)
            {
                foreach (var file in createProduct.ImageFiles)
                {
                    var stream = file.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    content.Add(fileContent, "imageFiles", file.FileName);
                }
            }

            // Thêm file game từ request vào nếu có
            if (createProduct.Request?.gameFile != null)
            {
                var gameFileStream = createProduct.Request.gameFile.OpenReadStream();
                var gameFileContent = new StreamContent(gameFileStream);
                content.Add(gameFileContent, "request.gameFile", createProduct.Request.gameFile.FileName);
            }

            // Gọi API với dữ liệu đã chuẩn bị
            return await _baseService.SendAsync(new RequestModel
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Data = content,  // Sử dụng MultipartFormDataContent làm Data
                Url = StaticTypeApi.APIGateWay + "/Product"
            });
        }



        public async Task<ResponseModel?> DeleteProductAsync(string id)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.DELETE,
                Url = StaticTypeApi.APIGateWay + "/Product/" + id
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

        public async Task<ResponseModel?> GetCategoriesAsync()
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.GET,  // Sử dụng phương thức GET để lấy dữ liệu
                Url = StaticTypeApi.APIGateWay + "/Categories" // Đường dẫn API lấy danh mục sản phẩm
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


        public async Task<ResponseModel?> UpdateProductAsync(UpdateProductModel product)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.PUT,
                Data = product,
                Url = StaticTypeApi.APIGateWay + "/Product/"
            }); ;
        }


    }
}
